using MockTail.Dependencies;
using MockTail.Exceptions;
using MockTail.Extensions;

namespace MockTail.MockTail;

public class MockTail<T> where T : class
{
    private readonly DependencyContainer<T> dependencyContainer;

    public MockTail()
    {
        dependencyContainer = new DependencyContainer<T>();
    }

    public T Build()
    {
        return MockTailInstantiator.Build(dependencyContainer);
    }

    public T BuildMock()
    {
        Type type = typeof(T);
        if (!type.IsMockable())
        {
            throw new NotMockableException(type);
        }

        return MockTailInstantiator.BuildMock<T>(dependencyContainer);
    }

    public TService Get<TService>() where TService : class
    {
        return dependencyContainer.Get<TService>();
    }

    public TService Get<TService>(string name) where TService : class
    {
        return dependencyContainer.Get<TService>(name);
    }

    public TService GetLazy<TService>() where TService : class
    {
        return dependencyContainer.GetLazy<TService>();
    }

    public TService GetLazy<TService>(string name) where TService : class
    {
        return dependencyContainer.GetLazy<TService>(name);
    }

    public TService GetFunc<TService>() where TService : class
    {
        return dependencyContainer.GetFunc<TService>();
    }

    public TService GetFunc<TService>(string name) where TService : class
    {
        return dependencyContainer.GetFunc<TService>(name);
    }

    public MockTail<T> Set<TService>(TService service)
    {
        dependencyContainer.Set(service);
        return this;
    }

    public MockTail<T> Set<TService>(string name, TService service)
    {
        dependencyContainer.Set(name, service);
        return this;
    }
}