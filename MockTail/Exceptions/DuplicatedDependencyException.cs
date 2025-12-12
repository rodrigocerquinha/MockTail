namespace MockTail.Exceptions;

public class DuplicatedDependencyException(Type depType) : Exception
{
    private readonly Type depType = depType;

    public override string Message => $"There is more than one dependency with the type '{depType.Name}', you need to specify the dependency type along with the name.";
}