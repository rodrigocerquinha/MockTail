using System.Reflection;

namespace MockTail.Constructors;

internal static class ConstructorSelector
{
    internal static ConstructorInfo? GetBestMatchingConstructor(Type type, Dictionary<Type, IEnumerable<string>> mannualyConfiguredArgumentNames)
    {
        ConstructorInfo? constructorInfo = null;
        ConstructorInfo[] constructors = TypeExtensions.GetConstructors(type);
        foreach (ConstructorInfo constructorInfo2 in constructors)
        {
            if (IsCandidateBetterThanCurrentBestConstructor(constructorInfo, constructorInfo2, mannualyConfiguredArgumentNames))
            {
                constructorInfo = constructorInfo2;
            }
        }
        return constructorInfo;
    }

    private static bool IsCandidateBetterThanCurrentBestConstructor(ConstructorInfo? currentBest, ConstructorInfo candidate, Dictionary<Type, IEnumerable<string>> mannualyConfiguredArgumentNames)
    {
        if (currentBest is null)
        {
            return true;
        }
        if (!ConstructorHasAllManuallyConfiguredArgumentNames(candidate, mannualyConfiguredArgumentNames))
        {
            return false;
        }
        return currentBest.GetParameters().Length < candidate.GetParameters().Length;
    }

    private static bool ConstructorHasAllManuallyConfiguredArgumentNames(ConstructorInfo candidate, Dictionary<Type, IEnumerable<string>> mannualyConfiguredArgumentNames)
    {
        ParameterInfo[] parameters = candidate.GetParameters();
        foreach (KeyValuePair<Type, IEnumerable<string>> argumentType in mannualyConfiguredArgumentNames)
        {
            foreach (string argumentName in argumentType.Value)
            {
                if (!parameters.Any((ParameterInfo x) => x.Name == argumentName && (object)x.ParameterType == argumentType.Key))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
