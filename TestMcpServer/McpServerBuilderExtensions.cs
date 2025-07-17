using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Server;

namespace TestMcpServer;

public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, Kernel kernel)
    {
        foreach (var plugin in kernel.Plugins)
        {
            foreach (var function in plugin)
            {
                // Use the kernel function directly as recommended
                builder.Services.AddSingleton(services => McpServerTool.Create(function));
            }
        }

        return builder;
    }
}
