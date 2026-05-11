# LightDIContainer

A lightweight dependency injection container for .NET 8 supporting constructor injection with Singleton and Transient lifetimes.

## Features

- Constructor injection (greedy — picks the constructor with the most resolvable parameters)
- Singleton and Transient lifetimes
- Circular dependency detection
- Fluent registration API

## Usage

```csharp
var container = new Container();

container
    .AddSingleton<ILogger, ConsoleLogger>()
    .AddTransient<IRepository, UserRepository>()
    .AddTransient<IService, UserService>();

var service = container.Resolve<IService>();
service.Execute();
```

## Registration

| Method | Lifetime |
|--------|----------|
| `AddSingleton<TService, TImpl>()` | One instance for the container's lifetime |
| `AddTransient<TService, TImpl>()` | New instance on every resolve |
| `AddSingleton<TImpl>()` | Self-registration, singleton |
| `AddTransient<TImpl>()` | Self-registration, transient |

## Running Tests

```bash
dotnet test
```
