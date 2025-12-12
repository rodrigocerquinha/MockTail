# MockTail 🍹

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![C# 12.0](https://img.shields.io/badge/C%23-12.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![NSubstitute](https://img.shields.io/badge/NSubstitute-5.3.0-green.svg)](https://nsubstitute.github.io/)

> **Automatic Mock & Dependency Injection Framework for .NET Testing**

MockTail is a powerful testing utility that automatically creates instances of classes with their dependencies mocked, eliminating boilerplate code and simplifying unit test setup.

---

## 🚀 Quick Start

```csharp
// Your class under test
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public User GetUser(int id) => _repository.GetById(id);
}

// Traditional approach (verbose)
[Test]
public void GetUser_TraditionalWay()
{
    var mockRepository = Substitute.For<IUserRepository>();
    var mockEmailService = Substitute.For<IEmailService>();
    var service = new UserService(mockRepository, mockEmailService);

    mockRepository.GetById(1).Returns(new User { Id = 1, Name = "John" });

    var result = service.GetUser(1);

    Assert.That(result.Name, Is.EqualTo("John"));
}

// MockTail approach (clean & simple)
[Test]
public void GetUser_WithMockTail()
{
    var mockTail = new MockTail<UserService>();
    var service = mockTail.Build();

    var mockRepository = mockTail.Get<IUserRepository>();
    mockRepository.GetById(1).Returns(new User { Id = 1, Name = "John" });

    var result = service.GetUser(1);

    Assert.That(result.Name, Is.EqualTo("John"));
}
```

## 📦 Installation

Coming soon to NuGet:
```bash
dotnet add package MockTail
```

For now, clone and reference the project:
```bash
git clone https://github.com/rodrigocerquinha/MockTail.git
```

## ✨ Features

### 🎯 Automatic Dependency Resolution
MockTail automatically discovers and mocks all constructor dependencies, saving you from manual mock creation.

### 🧠 Smart Constructor Selection
Intelligently selects the best constructor based on:
- Number of parameters (prefers constructors with more parameters)
- Manually configured dependencies (prioritizes constructors using your custom mocks)

### 🔄 Multiple Dependency Patterns
Supports various dependency injection patterns:
- **Direct dependencies**: `IService service`
- **Lazy dependencies**: `Lazy<IService> service`
- **Factory dependencies**: `Func<IService> serviceFactory`

### 🎨 Fluent API
Clean, readable builder pattern:
```csharp
var mockTail = new MockTail<MyService>()
    .Set(customDependency1)
    .Set("parameterName", customDependency2);
```

### 🛡️ Type-Safe
Full compile-time type checking ensures correctness.

---

## 📚 Usage Examples

### Basic Usage
```csharp
var mockTail = new MockTail<UserService>();
var service = mockTail.Build();

// Access and configure mocks
var mockRepo = mockTail.Get<IUserRepository>();
mockRepo.GetById(Arg.Any<int>()).Returns(new User());
```

### Manual Dependency Configuration
```csharp
// Create a custom mock with specific setup
var customLogger = Substitute.For<ILogger>();
customLogger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

// Use it in your service
var mockTail = new MockTail<UserService>()
    .Set(customLogger);

var service = mockTail.Build();
```


### Named Dependencies (Multiple Parameters of Same Type)
```csharp
public class ReportService
{
    public ReportService(ILogger primaryLogger, ILogger auditLogger) { }
}

// Configure each one separately
var mockTail = new MockTail<ReportService>()
    .Set("primaryLogger", primaryLoggerMock)
    .Set("auditLogger", auditLoggerMock);
```

### Lazy Dependencies
```csharp
public class CacheService
{
    private readonly Lazy<IDatabase> _database;

    public CacheService(Lazy<IDatabase> database)
    {
        _database = database;
    }
}

var mockTail = new MockTail<CacheService>();
var service = mockTail.Build();

// Get the mock for the lazy dependency
var dbMock = mockTail.GetLazy<IDatabase>();
dbMock.Connect().Returns(true);
```

### Func Dependencies
```csharp
public class JobProcessor
{
    private readonly Func<IJobRunner> _runnerFactory;

    public JobProcessor(Func<IJobRunner> runnerFactory)
    {
        _runnerFactory = runnerFactory;
    }
}

var mockTail = new MockTail<JobProcessor>();
var processor = mockTail.Build();

var runnerMock = mockTail.GetFunc<IJobRunner>();
runnerMock.Execute().Returns(true);
```


---

## 🏗️ Architecture

### Project Structure
```
MockTail/
├── MockTail/
│   ├── MockTail.cs              # Main entry point
│   ├── MockTailInstantiator.cs  # Instance creation logic
│   ├── Dependencies/
│   │   ├── DependencyContainer.cs   # Dependency management
│   │   ├── Dependency.cs            # Base dependency class
│   │   ├── MockDependency.cs        # Mock dependencies
│   │   ├── LazyMockDependency.cs    # Lazy<T> support
│   │   ├── FuncMockDependency.cs    # Func<T> support
│   │   └── RealDependency.cs        # Real instances
│   ├── Constructors/
│   │   └── ConstructorSelector.cs   # Constructor selection logic
│   ├── Exceptions/
│   │   ├── NotMockableException.cs
│   │   ├── DependencyNotAvailableException.cs
│   │   └── DuplicatedDependencyException.cs
│   └── Extensions/
│       └── TypeExtensions.cs        # Type utility methods
└── MockTail.csproj
```


### Core Components

#### 1. **MockTail\<T\>**
The main facade that provides the fluent API.

**Key Methods:**
- `Build()` - Creates instance with auto-mocked dependencies
- `BuildMock()` - Creates a mock of T with auto-mocked dependencies
- `Get<TService>()` - Retrieves a dependency mock
- `Set<TService>(service)` - Manually configures a dependency

#### 2. **DependencyContainer\<T\>**
The brain of MockTail - manages all dependencies for type T.

**Responsibilities:**
- Scans all constructors
- Creates appropriate mock/real instances
- Validates dependencies and types
- Handles generic wrappers (Lazy, Func)

#### 3. **ConstructorSelector**
Implements smart constructor selection strategy.

**Selection Criteria:**
1. Must contain all manually configured dependencies
2. Prefers constructor with most parameters
3. Falls back gracefully if no match found

#### 4. **Dependency Hierarchy**
```
Dependency (abstract)
├── MockDependency - NSubstitute mocks
│   ├── LazyMockDependency - Lazy<T> wrapper
│   └── FuncMockDependency - Func<T> wrapper
└── RealDependency - Actual instances/defaults
```

---

## 🎓 How It Works

### Step-by-Step Process

**1. Type Analysis**
```csharp
var mockTail = new MockTail<UserService>();
// Discovers all constructors of UserService
```

**2. Dependency Discovery**
```csharp
// Scans each constructor parameter
// Identifies: IUserRepository, IEmailService, ILogger, etc.
```

**3. Mock Creation**
```csharp
// For each dependency:
// - If mockable (interface/abstract) → creates NSubstitute mock
// - If Lazy<T> → wraps in Lazy
// - If Func<T> → creates factory function
// - If concrete → uses default value
```

**4. Constructor Selection**
```csharp
var service = mockTail
    .Set(customLogger)  // Manual configuration
    .Build();
// Selects constructor with customLogger parameter
// Fills remaining parameters with auto-mocks
```

**5. Instance Creation**
```csharp
// Invokes selected constructor with prepared dependencies
return new UserService(mockRepo, mockEmail, customLogger);
```

 
---

## 🔍 Advanced Features

### Mockability Rules

MockTail determines if a type is mockable:

| Type | Mockable | Example |
|------|----------|---------|
| Interface | ✅ Yes | `IUserService` |
| Abstract class | ✅ Yes | `abstract class BaseService` |
| Non-sealed class | ✅ Yes | `public class UserService` |
| Sealed class | ❌ No | `public sealed class StringHelper` |
| Primitive types | ❌ No | `int`, `string`, `DateTime` |
| Value types | ❌ No | `struct Point` |

### Expression Trees for Generic Wrappers

MockTail uses expression trees to create Lazy and Func wrappers:

```csharp
// Creating a Func<IService>
var delegateType = typeof(Func<>).MakeGenericType(serviceType);
var body = Expression.Block(Expression.Constant(mockInstance, serviceType));
var lambda = Expression.Lambda(delegateType, body);
return lambda.Compile(); // Returns Func<IService>
```


### Reflection-Based Discovery

Uses reflection extensively for:
- Constructor discovery and analysis
- Parameter type inspection
- Generic type manipulation
- Dynamic instantiation

---

## ⚠️ Exception Handling

MockTail provides clear, actionable error messages:

### NotMockableException
```csharp
// Thrown when trying to mock unsealed concrete classes
throw new NotMockableException(typeof(StringHelper));
// "The type 'StringHelper' is not mockable."
```

### DependencyNotAvailableException
```csharp
// No dependency of that type exists
throw new DependencyNotAvailableException(typeof(IService));
// "There is no dependency with the type 'IService'."

// Named dependency not found
throw new DependencyNotAvailableException(typeof(ILogger), "auditLogger");
// "There is no dependency with the type 'ILogger' and name 'auditLogger'."
```

### DuplicatedDependencyException
```csharp
// Multiple dependencies of same type without name specification
throw new DuplicatedDependencyException(typeof(ILogger));
// "There is more than one dependency with the type 'ILogger',
//  you need to specify the dependency type along with the name."
```


---

## 💡 Design Patterns

MockTail leverages several design patterns:

- **Builder Pattern** - Fluent API for configuration
- **Strategy Pattern** - Different dependency creation strategies
- **Template Method** - Abstract `Dependency.RealValue` property
- **Facade Pattern** - `MockTail<T>` simplifies complex subsystem
- **Factory Pattern** - Dynamic mock and instance creation

---

## 📊 Benefits

### ✅ Advantages

- **Less Boilerplate**: 50-70% reduction in test setup code
- **Improved Readability**: Tests focus on behavior, not setup
- **Refactoring-Friendly**: Add dependencies without updating all tests
- **Type-Safe**: Compile-time checking prevents runtime errors
- **Consistent**: Standardized approach across test suite

### ⚠️ Considerations

- **Reflection Overhead**: Slight performance cost (acceptable for tests)
- **Learning Curve**: Team needs to understand the tool
- **Not for Integration Tests**: Use real dependencies for integration tests
- **Mockable Types Only**: Works with interfaces, abstract classes, non-sealed classes

---

## 🎯 Use Cases

### Perfect For:
✅ Unit testing services with multiple dependencies  
✅ Testing classes with complex constructors  
✅ Projects following DI patterns  
✅ Rapid test creation and prototyping  
✅ Teams wanting consistent test setup  

### Not Ideal For:
❌ Integration tests (use real dependencies)  
❌ Testing static methods  
❌ Performance-critical test scenarios  
❌ Testing sealed concrete classes  

---

## 🛠️ Best Practices

### 1. **Use with Unit Tests Only**

```csharp
// ✅ Good - Unit test
[Test]
public void ProcessOrder_ValidOrder_ReturnsSuccess()
{
    var mockTail = new MockTail<OrderService>();
    var service = mockTail.Build();
    // ...
}

// ❌ Bad - Integration test
[Test]
public void ProcessOrder_Integration_SavesToDatabase()
{
    var mockTail = new MockTail<OrderService>(); // Don't mock in integration tests
    // Use real database connection instead
}
```


### 2. **Configure Before Building**
```csharp
// ✅ Good
var mockTail = new MockTail<UserService>()
    .Set(customLogger)
    .Set("cache", customCache);
var service = mockTail.Build();

// ❌ Bad
var service = mockTail.Build();
mockTail.Set(customLogger); // Too late!
```

### 3. **Use Named Dependencies for Clarity**

```csharp
// ✅ Good - Clear intent
var mockTail = new MockTail<PaymentProcessor>()
    .Set("primaryGateway", stripeMock)
    .Set("fallbackGateway", paypalMock);

// ❌ Unclear - Which logger is which?
mockTail.Set(logger1).Set(logger2); // Can't differentiate
```

### 4. **Keep Tests Simple**
```csharp
// ✅ Good - One concern per test
[Test]
public void GetUser_UserExists_ReturnsUser()
{
    var mockTail = new MockTail<UserService>();
    var mockRepo = mockTail.Get<IUserRepository>();
    mockRepo.GetById(1).Returns(new User { Id = 1 });

    var result = mockTail.Build().GetUser(1);

    Assert.That(result.Id, Is.EqualTo(1));
}
```


---

## 🔧 Technical Highlights

### Modern C# Features

MockTail leverages C# 12.0 features:

```csharp
// Collection expressions
dependencies = [];

// Primary constructors
public class NotMockableException(Type type) : Exception
{
    public override string Message => $"The type '{type.Name}' is not mockable.";
}

// Target-typed new expressions
return new RealDependency(service!, typeFromHandle, manuallyConfigured: true);
```

### Generic Type Constraints
```csharp
public class MockTail<T> where T : class
{
    public TService Get<TService>() where TService : class
    {
        // Type-safe dependency retrieval
    }
}
```


---

## 📄 License

MockTail is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

This is an open-source project that you can freely use, modify, and distribute in your projects (commercial or personal) under the terms of the MIT License.

### What You Can Do:
✅ Use in commercial projects  
✅ Modify and create derivative works  
✅ Distribute (with attribution)  
✅ Use for private purposes  

### What You Must Do:
📋 Include a copy of the license  
📋 Include copyright notice  

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

### Development Setup
```bash
git clone https://github.com/rodrigocerquinha/MockTail.git
cd MockTail
dotnet restore
dotnet build
```


---

## 🙏 Acknowledgments

- Built with [NSubstitute](https://nsubstitute.github.io/) - A friendly substitute for .NET mocking
- Inspired by AutoFixture's AutoMocking capabilities

---

## 📞 Contact & Support

- **GitHub Issues**: [Report bugs or request features](https://github.com/rodrigocerquinha/MockTail/issues)
- **Author**: [@rodrigocerquinha](https://github.com/rodrigocerquinha)

---

<div align="center">

**MockTail** - Making test setup a breeze! 🍹

⭐ Star this repo if you find it useful!

</div>
