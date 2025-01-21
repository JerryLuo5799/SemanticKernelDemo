using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelDemo.Plugin;
using Microsoft.SemanticKernel.Plugins.Core;

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
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

foreach (var plugin in kernel.Plugins)
{
    Console.WriteLine("plugin: " + plugin.Name);
    foreach (var function in plugin)
    {
        Console.WriteLine("  - prompt function: " + function.Name);
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


// Initiate a back-and-forth chat
string? userInput;
do
{
    // Collect user input
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("User > ");

    userInput = Console.ReadLine();
    // Add user input
    history.AddUserMessage(userInput);
    
    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(history, executionSettings, kernel);

    // Print the results
    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("Azure OpenAI > " + result);
    Console.ForegroundColor = ConsoleColor.Red;

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);

