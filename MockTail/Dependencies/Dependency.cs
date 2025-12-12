namespace MockTail.Dependencies;

internal abstract class Dependency(Type type, bool manuallyConfigured)
{
    internal Type Type { get; set; } = type;

    internal bool ManuallyConfigured { get; set; } = manuallyConfigured;

    internal abstract object RealValue { get; }
}