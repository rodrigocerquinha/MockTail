using System.Linq.Expressions;

namespace MockTail.Dependencies;

internal class FuncMockDependency : MockDependency
{
    internal override object RealValue
    {
        get
        {
            Type delegateType = typeof(Func<>).MakeGenericType(base.Type);
            BlockExpression body = Expression.Block(Expression.Constant(base.SubstituteInstance, base.Type));
            return Expression.Lambda(delegateType, body).Compile();
        }
    }

    internal FuncMockDependency(Type type, bool manuallyConfigured)
        : base(type, manuallyConfigured)
    {
    }

    internal FuncMockDependency(object substituteInstance, Type type, bool manuallyConfigured)
        : base(substituteInstance, type, manuallyConfigured)
    {
    }
}