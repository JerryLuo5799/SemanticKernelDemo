using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// Create a kernel builder
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

// Add the local plugins first
kernelBuilder.Plugins.AddFromPromptDirectory("Plugin");
kernelBuilder.Plugins.AddFromType<TimePlugin>("Time");

// Build MCP Client and Use MCP tools as SK plugins
await using IMcpClient mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "TestMcpServer",
    Command = "dotnet",
    Arguments = ["run", "--project", "../TestMcpServer/TestMcpServer.csproj"]
}));

// Get MCP tools from the server
var mcpTools = await mcpClient.ListToolsAsync();

Console.WriteLine($"Found {mcpTools.Count} MCP tools:");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"  - {tool.Name}: {tool.Description}");
}
// Add MCP tools as SK functions using completely dynamic approach (following official blog)
var mcpFunctions = new List<KernelFunction>();
foreach (var tool in mcpTools)
{
    var function = KernelFunctionFactory.CreateFromMethod(
        async (KernelArguments arguments) =>
        {
            try
            {
                // Convert KernelArguments to Dictionary for MCP call
                var mcpArguments = new Dictionary<string, object?>();
                foreach (var arg in arguments)
                {
                    mcpArguments[arg.Key] = arg.Value;
                }
                
                var result = await mcpClient.CallToolAsync(tool.Name, mcpArguments);
                
                // Dynamically extract result content using reflection
                if (result != null)
                {
                    var resultType = result.GetType();
                    var contentProperty = resultType.GetProperty("Content");
                    if (contentProperty != null)
                    {
                        var content = contentProperty.GetValue(result);
                        
                        if (content is IEnumerable<object> contentList)
                        {
                            var contentBlocks = contentList.ToList();
                            if (contentBlocks.Count > 0)
                            {
                                var firstBlock = contentBlocks[0];
                                var blockType = firstBlock.GetType();
                                var textProperty = blockType.GetProperty("Text");
                                if (textProperty != null)
                                {
                                    var textValue = textProperty.GetValue(firstBlock);
                                    return textValue?.ToString() ?? "No text content";
                                }
                                return firstBlock.ToString() ?? "No content";
                            }
                        }
                        return content?.ToString() ?? "No content";
                    }
                    return result.ToString() ?? "No result";
                }
                
                return "No result from MCP tool";
            }
            catch (Exception ex)
            {
                return $"Error calling MCP tool: {ex.Message}";
            }
        },
        functionName: tool.Name,
        description: tool.Description ?? $"MCP tool: {tool.Name}"
    );
    
    mcpFunctions.Add(function);
}

kernelBuilder.Plugins.AddFromFunctions("McpTools", mcpFunctions);

// Add Azure OpenAI chat completion
kernelBuilder.AddAzureOpenAIChatCompletion(Config.DEPLOYMENT_NAME, Config.ENDPOINT, Config.API_KEY);

// Build the kernel
Kernel kernel = kernelBuilder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

foreach (var plugin in kernel.Plugins)
{
    Console.WriteLine($"Plugin: {plugin.Name} ({plugin.Count()} functions)");
}

// Enable automatic function calling
PromptExecutionSettings executionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
};

// Create a history store the conversation
var history = new ChatHistory($"""
        你是一个AI助手。你总是会调用插件来回答用户的问题, 如果用户的请求可以通过插件来满足, 那就只调用插件。你不会更改插件的输出格式。
        你不会输出markdown格式。
        如果遇到"今天", "昨天"这样的日期描述，请根据今天的实际日期, 将其转换为具体日期
        """);

string? userInput;
do
{
    // Collect user input
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("User > ");

    userInput = Console.ReadLine();
    // Add user input
    history.AddUserMessage(userInput ?? string.Empty);

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(history, executionSettings, kernel);

    // Print the results
    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("Azure OpenAI > " + result);
    Console.ForegroundColor = ConsoleColor.Red;

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
