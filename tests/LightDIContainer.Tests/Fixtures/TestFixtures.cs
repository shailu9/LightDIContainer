namespace LightDIContainer.Tests.Fixtures;


// interfaces
public interface ILogger
{
    void Log(string message);
    IReadOnlyList<string> Messages { get; }
}

public interface IRepository
{
    void Save(string data);
}
public interface IService
{
    void Execute();
}

public interface IOrderService
{
    void PlaceOrder(string item);
}

// concrete implementations
public class ConsoleLogger : ILogger
{
    private readonly List<string> _messages = new();
    public IReadOnlyList<string> Messages => _messages.AsReadOnly();
    public void Log(string message) => _messages.Add(message);
}

public class AnotherLogger : ILogger
{
    private readonly List<string> _messages = new();
    public IReadOnlyList<string> Messages => _messages.AsReadOnly();
    public void Log(string message) => _messages.Add($"[ALT] {message}");
}

public class UserRepository : IRepository
{
    private readonly ILogger _logger;
    public UserRepository(ILogger logger) { _logger = logger; }
    public void Save(string data) => _logger.Log($"Saved: {data}");
}

public class UserService : IService
{
    private readonly IRepository _repository;
    private readonly ILogger _logger;

    public UserService(IRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public void Execute()
    {
        _logger.Log("Executing UserService");
        _repository.Save("User data");
    }
}

public class OrderService : IOrderService
{
    private readonly ILogger _logger;

    public OrderService(ILogger logger)
    {
        _logger = logger;
    }

    public void PlaceOrder(string item)
    {
        _logger.Log($"Placing order: {item}");
    }
}

//// <summary>Concrete class with no interface — for self-registration tests.</summary>
public class StandaloneService
{
    private readonly ILogger _logger;
    public StandaloneService(ILogger logger) { _logger = logger; }
    public void DoWork() => _logger.Log("StandaloneService working");
}

// Special-case implementations for specific test scenarios
// Class with a parameterless constructor — tests zero-dep resolution
public class NoDepService
{
    public bool WasCreated { get; } = true;
}

// For circular dependency tests — A depends on B
public interface ICircularA { }
public interface ICircularB { }
public class CircularA : ICircularA { public CircularA(ICircularB b) { } }
public class CircularB : ICircularB { public CircularB(ICircularA a) { } }
