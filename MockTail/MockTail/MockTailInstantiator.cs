using MockTail.Constructors;
using MockTail.Dependencies;
using NSubstitute;
using System.Reflection;

namespace MockTail.MockTail;

internal static class MockTailInstantiator
{
    internal static T Build<T>(DependencyContainer<T> dependencyContainer)
    {
        ConstructorInfo bestMatchingConstructor = ConstructorSelector.GetBestMatchingConstructor(typeof(T), dependencyContainer.GetMannualyConfiguredDependencyNames())
            ?? throw new InvalidOperationException($"No suitable constructor found for type {typeof(T).FullName}.");
        IEnumerable<object> source = PrepareDependenciesFor(bestMatchingConstructor, dependencyContainer);
        return (T)bestMatchingConstructor.Invoke(source.ToArray());
    }

    internal static T BuildMock<T>(DependencyContainer<T> dependencyContainer) where T : class
    {
        ConstructorInfo bestMatchingConstructor = ConstructorSelector.GetBestMatchingConstructor(typeof(T), dependencyContainer.GetMannualyConfiguredDependencyNames())
            ?? throw new InvalidOperationException($"No suitable constructor found for type {typeof(T).FullName}.");
        IEnumerable<object> source = PrepareDependenciesFor(bestMatchingConstructor, dependencyContainer);
        return Substitute.For<T>(source.ToArray());
    }

    private static IEnumerable<object> PrepareDependenciesFor<T>(ConstructorInfo constructor, DependencyContainer<T> dependencyContainer)
    {
        List<object> list = [];
        ParameterInfo[] parameters = constructor.GetParameters();
        foreach (ParameterInfo parameterInfo in parameters)
        {
            object configuredDependency = dependencyContainer.GetConfiguredDependency(parameterInfo.ParameterType, parameterInfo.Name);
            list.Add(configuredDependency);
        }
        return list;
    }
}
