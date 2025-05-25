using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDemo.Plugin;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using McpDotNet.Client;
using McpDotNet.Configuration;
using McpDotNet.Protocol.Transport;
using Microsoft.Extensions.AI;
using McpDotNet.Protocol.Types; // Add this using directive

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(Config.DEPLOYMENT_NAME, Config.ENDPOINT, Config.API_KEY);

// Add the plugin to the kernel
builder.Plugins.AddFromType<WeatherPlugin>("Weather");
builder.Plugins.AddFromPromptDirectory("Plugin");
builder.Plugins.AddFromType<TimePlugin>("Time");

// Build the kernel
Kernel kernel = builder.Build();

//await kernel.ImportPluginFromOpenApiAsync("Schedule", new Uri("https://localhost:7293/swagger/v1/swagger.json"), new OpenApiFunctionExecutionParameters() { EnablePayloadNamespacing = true });

McpClientOptions options = new()
{
    ClientInfo = new() { Name = "TestClient", Version = "1.0.0" }
};

McpServerConfig config = new()
{
    Id = "everything",
    Name = "Everything",
    TransportType = TransportTypes.StdIo,
    TransportOptions = new()
    {
        ["command"] = "npx",
        ["arguments"] = "-y @modelcontextprotocol/server-everything",
    }
};


var client = await McpClientFactory.CreateAsync(config, options);

var mcpTools = new List<Tool>();
await foreach (var tool in client.ListToolsAsync())
{
    //Console.WriteLine($"{tool.Name} ({tool.Description})");
    mcpTools.Add(tool);
}

// 创建 MCP 工具的 Semantic Kernel 函数
var mcpFunctions = new List<KernelFunction>();

foreach (var tool in mcpTools)
{
    // 捕获当前工具的名称，避免闭包问题
    var currentToolName = tool.Name;
    
    // 为每个 MCP 工具创建一个函数
    var kernelFunction = KernelFunctionFactory.CreateFromMethod(
        async (string input) =>
        {
            try
            {
                // 调用 MCP 工具 - 修改参数格式
                var parameters = new Dictionary<string, object>
                {
                    ["input"] = input
                };
                var result = await client.CallToolAsync(currentToolName, parameters);
                return result?.ToString() ?? "No result";
            }
            catch (Exception ex)
            {
                return $"Error calling {currentToolName}: {ex.Message}";
            }
        },
        functionName: tool.Name,
        description: tool.Description ?? $"MCP tool: {tool.Name}"
    );
    
    mcpFunctions.Add(kernelFunction);
}

// 将函数添加到内核作为插件
kernel.Plugins.AddFromFunctions("GithubMcpTools", mcpFunctions);

//IList<AIFunction> tools = await client.GetAIFunctionsAsync();


//kernel.Plugins.AddFromFunctions("GitHub", tools.Select(aiFunction => aiFunction.AsKernelFunction()));


var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

foreach (var plugin in kernel.Plugins)
{
    Console.WriteLine("plugin: " + plugin.Name);
    foreach (var function in plugin)
    {
        Console.WriteLine($"  - function: {function.Name}, Description: {function.Description}");
    }
}

// 2. Enable automatic function calling
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
    history.AddUserMessage(userInput ?? string.Empty); // Fix for possible null reference

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(history, executionSettings, kernel);

    // Print the results
    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("Azure OpenAI > " + result);
    Console.ForegroundColor = ConsoleColor.Red;

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);
