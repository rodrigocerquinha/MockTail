using NSubstitute;
using System.Reflection;

namespace MockTail.Extensions;

internal static class TypeExtensions
{
    internal static bool TryGetGenericTypeDefinition(this Type type, out Type? genericType)
    {
        genericType = null;
        TypeInfo typeInfo = type.GetTypeInfo();
        if (typeInfo.IsGenericType)
        {
            genericType = typeInfo.GetGenericTypeDefinition();
            return true;
        }
        return false;
    }

    internal static bool IsMockable(this Type type)
    {
        TypeInfo typeInfo = type.GetTypeInfo();
        if (!typeInfo.IsInterface && !typeInfo.IsAbstract)
        {
            return !typeInfo.IsSealed;
        }
        return true;
    }

    internal static object? DefaultValue(this Type type)
    {
        if (type.GetTypeInfo().IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    internal static object? CreateMock(this Type type)
    {
        return Substitute.For([type], Array.Empty<object>());
    }
}
