using MockTail.Extensions;

namespace MockTail.Dependencies;

internal class RealDependency : Dependency
{
    internal object Value { get; set; }

    internal override object RealValue => Value;

    internal RealDependency(Type type, bool manuallyConfigured)
        : this(type.DefaultValue() ?? throw new ArgumentNullException(nameof(type), "DefaultValue() returned null."), type, manuallyConfigured)
    {
    }

    internal RealDependency(object Value, Type type, bool manuallyConfigured)
        : base(type, manuallyConfigured)
    {
        this.Value = Value;
    }
}