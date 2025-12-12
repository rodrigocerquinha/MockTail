using MockTail.Extensions;

namespace MockTail.Dependencies;

internal class MockDependency : Dependency
{
    internal object SubstituteInstance { get; set; }

    internal override object RealValue => SubstituteInstance;

    internal MockDependency(Type type, bool manuallyConfigured)
        : this(type.CreateMock() ?? throw new ArgumentNullException(nameof(type), "CreateMock() returned null."), type, manuallyConfigured)
    {
    }

    internal MockDependency(object substituteInstance, Type type, bool manuallyConfigured)
        : base(type, manuallyConfigured)
    {
        SubstituteInstance = substituteInstance;
    }
}
