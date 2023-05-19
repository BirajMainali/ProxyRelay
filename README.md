# ProxyRelay

ProxyRelay is a C# application that acts as a proxy server to relay HTTP requests from a source URL to a destination URL. It uses the `HttpListener` and `HttpClient` classes to handle incoming requests and forward them to the specified destination.

## Features

- Relays HTTP requests from a source URL to a destination URL.
- Supports forwarding requests with various HTTP methods.
- Handles request headers and content types.
- Decompresses gzip-encoded responses from the destination URL if applicable.
- Provides logging using Microsoft.Extensions.Logging.

## Prerequisites

- .NET Framework 4.7.2 or higher.
- Visual Studio or another compatible C# development environment.

## Installation

1. Clone the repository or download the source code files.
2. Open the solution file (`ProxyRelay.sln`) in Visual Studio.
3. Build the solution to compile the application.

## Usage

1. Instantiate a `ProxyService` object, passing an instance of `ILogger<ProxyService>` as a parameter.
2. Customize the `SourceUrl` and `DestinationUrl` constants in the `ProxyService` class according to your requirements.
3. Run the application.

```csharp
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<ProxyService>();
var proxyService = new ProxyService(logger);
await proxyService.StartAsync(CancellationToken.None);
```

## Configuration

The `ProxyService` class contains two constants that can be configured:

- `SourceUrl`: The URL that the proxy server listens on for incoming requests. Modify this value to specify the desired source URL.
- `DestinationUrl`: The URL to which the incoming requests will be forwarded. Modify this value to specify the desired destination URL.

Ensure that the provided URLs are valid and accessible.

## Contributing

Contributions to ProxyRelay are welcome! If you encounter any issues or have suggestions for improvements, please open an issue or submit a pull request on the [GitHub repository](https://github.com/BirajMainali/proxyrelay).

## License

This project is licensed under the [MIT License](LICENSE).
