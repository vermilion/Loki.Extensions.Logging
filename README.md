# Loki.Extensions.Logging [![downloads](https://img.shields.io/nuget/dt/Loki.Extensions.Logging?style=flat-square)](https://www.nuget.org/packages/Loki.Extensions.Logging) [![nuget](https://img.shields.io/nuget/v/Loki.Extensions.Logging.svg?style=flat-square)](https://www.nuget.org/packages/Loki.Extensions.Logging) [![license](https://img.shields.io/github/license/vermilion/Loki.Extensions.Logging.svg?style=flat-square)](https://github.com/vermilion/Loki.Extensions.Logging/blob/master/LICENSE.md)

[Loki](https://grafana.com/docs/loki/latest/) .NET Standard 2.0 provider for [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging).

## Usage

Start by installing the [NuGet package](https://www.nuget.org/packages/Loki.Extensions.Logging).

```sh
dotnet add package Loki.Extensions.Logging
```

### ASP.NET Core

Use the `LoggingBuilder.AddLoki()` extension method from the `Loki.Extensions.Logging` namespace when configuring your host.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .UseStartup<Startup>()
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    loggingBuilder.AddLoki(options =>
                    {
                        options.IncludeScopes = false;
                        options.ApplicationName = context.HostingEnvironment.ApplicationName;
                    });
                });
            });
}
```

Logger options are taken from the "Loki" provider section in `appsettings.json` in the same way as other providers. These are customised further in the code above.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "Loki": {
      "Host": "localhost",
      "Port": 3100,                  // Not required if using default 3100.
      "LogSource": "My.App.Name",    // Not required if set in code as above.
      "AdditionalFields": {          // Optional fields added to all logs.
        "foo": "bar"
      },
      "LogLevel": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  }
}
```

For a full list of options, see [`LokiLoggerOptions`](src/Loki.Extensions.Logging/Options/LokiLoggerOptions.cs). See the [samples](/samples) directory full examples. For more information on providers and logging in general, see the aspnetcore [logging documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging).

### Auto Reloading Config

Settings can be changed at runtime and will be applied without the need for restarting your app. In the case of invalid config (e.g. missing hostname) the change will be ignored.

#### Global Fields

Global fields can be added to all logs by setting them in `LokiLoggerOptions.AdditionalFields`.

#### Scoped Fields

[Log scopes](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-7.0#logscopes) can also be used to attach fields to a group of related logs. Create a log scope with a [`ValueTuple<string, string>`](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/), `ValueTuple<string, int/double/decimal>` (or any other numeric value) or `Dictionary<string, object>` to do so. _Note that any other types passed to `BeginScope()` will be ignored, including `Dictionary<string, string>`._

```csharp
using (_logger.BeginScope(("correlation_id", correlationId)))
{
    // Field will be added to all logs within this scope (using any ILogger<T> instance).
}

using (_logger.BeginScope(new Dictionary<string, object>
{
    ["order_id"] = orderId,
    ["customer_id"] = customerId
}))
{
    // Fields will be added to all logs within this scope (using any ILogger<T> instance).
}
```

#### Additional Fields Factory

It is possible to derive additional fields from log data with a factory function. This is useful for adding more information than what is provided by default e.g. the Microsoft log level or exception type.

```csharp
options.AdditionalFieldsFactory = (logLevel, eventId, exception) =>
    new Dictionary<string, object>
    {
        ["log_level"] = logLevel.ToString(),
        ["exception_type"] = exception?.GetType().ToString()
    };
```

## Contributing

Pull requests welcome!