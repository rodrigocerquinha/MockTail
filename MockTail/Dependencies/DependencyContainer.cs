using MockTail.Exceptions;
using MockTail.Extensions;
using System.Reflection;

namespace MockTail.Dependencies;

internal class DependencyContainer<T>
{
    private readonly Type type;

    private Dictionary<Type, Dictionary<string, Dependency>> dependencies = [];

    internal DependencyContainer()
    {
        type = typeof(T);
        InitializeAllDependencies();
    }

    internal DepType Get<DepType>() where DepType : class
    {
        return Get<DepType>(null, null);
    }

    internal DepType Get<DepType>(string name) where DepType : class
    {
        return Get<DepType>(name, null);
    }

    internal DepType GetLazy<DepType>() where DepType : class
    {
        return Get<DepType>(null, typeof(Lazy<DepType>));
    }

    internal DepType GetLazy<DepType>(string name) where DepType : class
    {
        return Get<DepType>(name, typeof(Lazy<DepType>));
    }

    internal DepType GetFunc<DepType>() where DepType : class
    {
        return Get<DepType>(null, typeof(Func<DepType>));
    }

    internal DepType GetFunc<DepType>(string name) where DepType : class
    {
        return Get<DepType>(name, typeof(Func<DepType>));
    }

    internal void Set<DepType>(DepType service)
    {
        Type typeFromHandle = typeof(DepType);
        ValidateThatOnlyExistsOneDepedencyOfType(typeFromHandle);
        Dictionary<string, Dependency> dictionary = dependencies[typeFromHandle];
        var firstKey = dictionary.FirstOrDefault().Key;
        if (firstKey is null)
        {
            throw new InvalidOperationException($"No dependency key found for type {typeFromHandle}.");
        }
        dictionary[firstKey] = new RealDependency(service!, typeFromHandle, manuallyConfigured: true);
    }

    internal void Set<DepType>(string name, DepType service)
    {
        Type typeFromHandle = typeof(DepType);
        ValidateThatExistsADependencyWithThatTypeAndName(typeFromHandle, name);
        if (service is null)
        {
            throw new ArgumentNullException(nameof(service), $"Cannot set a null service for dependency type {typeFromHandle} and name '{name}'.");
        }
        dependencies[typeFromHandle][name] = new RealDependency(service, typeFromHandle, manuallyConfigured: true);
    }

    private DepType Get<DepType>(string? name = null, Type? genericTypeWrapper = null) where DepType : class
    {
        Type requestedType = typeof(DepType);
        Type dependencyType = genericTypeWrapper ?? requestedType;
        ValidateMockability(requestedType);
        Dependency dependency;
        if (string.IsNullOrEmpty(name))
        {
            ValidateThatOnlyExistsOneDepedencyOfType(dependencyType);
            dependency = dependencies[dependencyType].Values.FirstOrDefault()
                ?? throw new DependencyNotAvailableException(dependencyType);
        }
        else
        {
            ValidateThatExistsADependencyWithThatTypeAndName(dependencyType, name);
            dependency = dependencies[dependencyType][name];
        }
        MockDependency mockDependency = ValidateThatDependencyIsAMockDependency(dependency, requestedType, name);
        return (DepType)mockDependency.SubstituteInstance;
    }

    private static MockDependency ValidateThatDependencyIsAMockDependency(Dependency dependency, Type requestedType, string name)
    {
        if (dependency is MockDependency mockDependency)
        {
            return mockDependency;
        }
        throw new DependencyNotAvailableException(requestedType, name);
    }

    internal Dictionary<Type, IEnumerable<string>> GetMannualyConfiguredDependencyNames()
    {
        Dictionary<Type, IEnumerable<string>> dictionary = [];
        foreach (KeyValuePair<Type, Dictionary<string, Dependency>> dependency in dependencies)
        {
            dictionary[dependency.Key] = from x in dependency.Value
                                         where x.Value.ManuallyConfigured
                                         select x.Key;
        }
        return dictionary;
    }

    internal object GetConfiguredDependency(Type type, string name)
    {
        return dependencies[type][name].RealValue;
    }

    private void InitializeAllDependencies()
    {
        dependencies = [];
        ConstructorInfo[] constructors = System.Reflection.TypeExtensions.GetConstructors(type);
        for (int i = 0; i < constructors.Length; i++)
        {
            ParameterInfo[] parameters = constructors[i].GetParameters();
            foreach (ParameterInfo obj in parameters)
            {
                Type parameterType = obj.ParameterType;
                var name = obj.Name;
                if (string.IsNullOrEmpty(name))
                {
                    // Skip parameters with null or empty names to avoid CS8604
                    continue;
                }
                if (!dependencies.TryGetValue(parameterType, out Dictionary<string, Dependency>? value))
                {
                    value = [];
                    dependencies.Add(parameterType, value);
                }

                value.Add(name, InitializeDependency(parameterType));
            }
        }
    }

    private Dependency InitializeDependency(Type parameterType)
    {
        if (parameterType.TryGetGenericTypeDefinition(out var genericType))
        {
            Type type = parameterType.GenericTypeArguments[0];
            if (type.IsMockable())
            {
                if ((object?)genericType == typeof(Lazy<>))
                {
                    return new LazyMockDependency(type, manuallyConfigured: false);
                }
                if ((object?)genericType == typeof(Func<>))
                {
                    return new FuncMockDependency(type, manuallyConfigured: false);
                }
            }
        }
        if (parameterType.IsMockable())
        {
            return new MockDependency(parameterType, manuallyConfigured: false);
        }
        return new RealDependency(parameterType, manuallyConfigured: false);
    }

    private static void ValidateMockability(Type depType)
    {
        if (!depType.IsMockable())
        {
            throw new NotMockableException(depType);
        }
    }

    private void ValidateDependencyTypeExistence(Type depType)
    {
        if (!dependencies.ContainsKey(depType))
        {
            throw new DependencyNotAvailableException(depType);
        }
    }

    private void ValidateThatOnlyExistsOneDepedencyOfType(Type depType)
    {
        ValidateDependencyTypeExistence(depType);
        if (dependencies[depType].Count > 1)
        {
            throw new DuplicatedDependencyException(depType);
        }
    }

    private void ValidateThatExistsADependencyWithThatTypeAndName(Type depType, string name)
    {
        ValidateDependencyTypeExistence(depType);
        if (!dependencies[depType].ContainsKey(name))
        {
            throw new DependencyNotAvailableException(depType, name);
        }
    }
}