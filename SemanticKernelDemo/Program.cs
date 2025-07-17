using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Microsoft.Extensions.AI;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(Config.DEPLOYMENT_NAME, Config.ENDPOINT, Config.API_KEY);

// Add the plugin to the kernel
builder.Plugins.AddFromPromptDirectory("Plugin");
builder.Plugins.AddFromType<TimePlugin>("Time");

// Build the kernel
Kernel kernel = builder.Build();

//await kernel.ImportPluginFromOpenApiAsync("Schedule", new Uri("https://localhost:7293/swagger/v1/swagger.json"), new OpenApiFunctionExecutionParameters() { EnablePayloadNamespacing = true });

// 创建MCP客户端连接到TestMcpServer
var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "TestMcpServer",
    Command = "dotnet",
    Arguments = ["run", "--project", "../TestMcpServer/TestMcpServer.csproj"],
});

// var client = await McpClientFactory.CreateAsync(clientTransport);

// // 获取所有可用的工具
// var mcpTools = await client.ListToolsAsync();

// // 创建 MCP 工具的 Semantic Kernel 函数
// var mcpFunctions = new List<KernelFunction>();

// foreach (var tool in mcpTools)
// {
//     // 为每个 MCP 工具创建一个函数
//     var kernelFunction = KernelFunctionFactory.CreateFromMethod(
//         async (string cityName) =>
//         {
//             try
//             {
//                 // 根据工具名称调用相应的参数
//                 var parameters = new Dictionary<string, object?>();
                
//                 if (tool.Name == "GetCity")
//                 {
//                     parameters["cityName"] = cityName;
//                 }
//                 else if (tool.Name == "GetWeatherOfCity")
//                 {
//                     // 对于这个工具，cityName参数实际上是cityCode
//                     parameters["cityCode"] = cityName;
//                 }
//                 else if (tool.Name == "GetWeatherByCityName")
//                 {
//                     parameters["cityName"] = cityName;
//                 }
//                 else
//                 {
//                     parameters["input"] = cityName;
//                 }
                
//                 var result = await client.CallToolAsync(tool.Name, parameters);
                
//                 // 处理结果内容
//                 if (result?.Content != null && result.Content.Any())
//                 {
//                     var firstContent = result.Content.First();
//                     if (firstContent.Type == "text")
//                     {
//                         // 使用反射获取Text属性或直接转换
//                         var textProp = firstContent.GetType().GetProperty("Text");
//                         if (textProp != null)
//                         {
//                             return textProp.GetValue(firstContent)?.ToString() ?? "No text content";
//                         }
//                     }
//                     return firstContent.ToString() ?? "No result";
//                 }
                
//                 return "No result";
//             }
//             catch (Exception ex)
//             {
//                 return $"Error calling {tool.Name}: {ex.Message}";
//             }
//         },
//         functionName: tool.Name,
//         description: tool.Description ?? $"MCP tool: {tool.Name}"
//     );
    
//     mcpFunctions.Add(kernelFunction);
// }

// // 将函数添加到内核作为插件
// kernel.Plugins.AddFromFunctions("TestMcpTools", mcpFunctions);

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
