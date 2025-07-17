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
                builder.Services.AddSingleton(services => McpServerTool.Create(function.AsAIFunction()));
            }
        }

        return builder;
    }
}
