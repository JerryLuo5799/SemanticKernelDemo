using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;
using MyPlugin;

namespace TestMcpServer;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a kernel builder and add plugins from MyPlugin project
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Plugins.AddFromType<WeatherPlugin>();
        
        // Build the kernel
        Kernel kernel = kernelBuilder.Build();

        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        
        // Configure logging to stderr (required for MCP)
        builder.Logging.AddConsole(consoleLogOptions =>
        {
            consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Configure MCP Server and add all functions from the kernel plugins as MCP tools
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            // Add all functions from the kernel plugins to the MCP server as tools
            .WithTools(kernel);

        await builder.Build().RunAsync();
    }
}
