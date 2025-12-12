namespace MockTail.Exceptions;

public class DependencyNotAvailableException(Type depType, string? depName = null) : Exception
{
    private readonly Type depType = depType;

    private readonly string? depName = depName;

    public override string Message
    {
        get
        {
            if (string.IsNullOrEmpty(depName))
            {
                return $"There is no dependency with the type '{depType.Name}'.";
            }
            return $"There is no dependency with the type '{depType.Name}' and name '{depName}'.";
        }
    }
}
