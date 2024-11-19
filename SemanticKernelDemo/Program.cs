using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure;
using Microsoft.Extensions.DependencyInjection;
using Azure.Search.Documents.Models;
using SemanticKernelDemo;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// Create a kernel with Azure OpenAI chat completion
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(Config.DEPLOYMENT_NAME, Config.ENDPOINT, Config.API_KEY);

// Build the kernel
Kernel kernel = builder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Create a history store the conversation
var history = new ChatHistory("""
    You are an AI assistant that helps people find information,  you will provide all the detailed information.
    You like to speak Chinese, you don't output results in Markdown format.
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
    var result = await chatCompletionService.GetChatMessageContentAsync(history, null, kernel);

    // Print the results
    Console.ForegroundColor = ConsoleColor.White;

    Console.WriteLine("Azure OpenAI > " + result);
    Console.ForegroundColor = ConsoleColor.Red;

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);

