using MockTail.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace MockTail.Dependencies;

internal class LazyMockDependency : MockDependency
{
    internal override object RealValue
    {
        get
        {
            Type delegateType = typeof(Func<>).MakeGenericType(base.Type);
            BlockExpression body = Expression.Block(Expression.Constant(base.SubstituteInstance, base.Type));
            LambdaExpression lambdaExpression = Expression.Lambda(delegateType, body);
            var lazyType = typeof(Lazy<>).MakeGenericType(base.Type);
            var constructors = System.Reflection.TypeExtensions.GetConstructors(lazyType);
            var ctor = constructors?.FirstOrDefault(IsTheLazyConstructorToUseTheLambdaExpression);
            if (ctor is null)
            {
                throw new InvalidOperationException($"No suitable constructor found for type {lazyType.FullName}.");
            }
            return ctor.Invoke([lambdaExpression.Compile()]);
        }
    }

    internal LazyMockDependency(Type type, bool manuallyConfigured)
        : base(type, manuallyConfigured)
    {
    }

    internal LazyMockDependency(object substituteInstance, Type type, bool manuallyConfigured)
        : base(substituteInstance, type, manuallyConfigured)
    {
    }

    private bool IsTheLazyConstructorToUseTheLambdaExpression(ConstructorInfo co)
    {
        var parameters = co.GetParameters();
        if (parameters.Length == 1 && parameters[0].ParameterType.TryGetGenericTypeDefinition(out var genericType))
        {
            return (object?)genericType == typeof(Func<>);
        }
        return false;
    }
}
