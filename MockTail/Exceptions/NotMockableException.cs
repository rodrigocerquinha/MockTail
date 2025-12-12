namespace MockTail.Exceptions;

public class NotMockableException(Type type) : Exception
{
    private readonly Type type = type;

    public override string Message => $"The type '{type.Name}' is not mockable.";
}
