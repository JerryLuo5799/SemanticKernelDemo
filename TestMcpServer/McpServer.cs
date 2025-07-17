using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TestMcpServer;

public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly WeatherService _weatherService;

    public McpServer(ILogger<McpServer> logger)
    {
        _logger = logger;
        _weatherService = new WeatherService();
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Test MCP Server...");

        // MCP 协议使用标准输入输出进行通信
        using var stdin = Console.OpenStandardInput();
        using var stdout = Console.OpenStandardOutput();
        using var reader = new StreamReader(stdin);
        using var writer = new StreamWriter(stdout) { AutoFlush = true };

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line);
                if (request != null)
                {
                    var response = await HandleRequestAsync(request);
                    var responseJson = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request: {Line}", line);
                var errorResponse = new McpResponse
                {
                    Id = null,
                    Error = new McpError
                    {
                        Code = -1,
                        Message = ex.Message
                    }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse);
                await writer.WriteLineAsync(errorJson);
            }
        }
    }

    private async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        try
        {
            return request.Method switch
            {
                "initialize" => HandleInitialize(request),
                "tools/list" => HandleToolsList(request),
                "tools/call" => await HandleToolCallAsync(request),
                _ => new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError
                    {
                        Code = -32601,
                        Message = $"Method not found: {request.Method}"
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request method: {Method}", request.Method);
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -1,
                    Message = ex.Message
                }
            };
        }
    }

    private McpResponse HandleInitialize(McpRequest request)
    {
        return new McpResponse
        {
            Id = request.Id,
            Result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name = "test-mcp-server",
                    version = "1.0.0"
                }
            }
        };
    }

    private McpResponse HandleToolsList(McpRequest request)
    {
        var tools = new object[]
        {
            new
            {
                name = "get_city",
                description = "Gets a city by cityName",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        cityName = new
                        {
                            type = "string",
                            description = "The name of the city"
                        }
                    },
                    required = new[] { "cityName" }
                }
            },
            new
            {
                name = "get_weather_of_city",
                description = "Get the current weather in a given city",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        cityCode = new
                        {
                            type = "string",
                            description = "The code of the city"
                        }
                    },
                    required = new[] { "cityCode" }
                }
            },
            new
            {
                name = "get_weather_by_city_name",
                description = "Get the current weather by city name",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        cityName = new
                        {
                            type = "string",
                            description = "The name of the city"
                        }
                    },
                    required = new[] { "cityName" }
                }
            }
        };

        return new McpResponse
        {
            Id = request.Id,
            Result = new { tools }
        };
    }

    private async Task<McpResponse> HandleToolCallAsync(McpRequest request)
    {
        if (request.Params?.GetProperty("name").GetString() is not string toolName)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32602,
                    Message = "Invalid parameters: missing tool name"
                }
            };
        }

        var arguments = request.Params?.GetProperty("arguments");

        try
        {
            var result = toolName switch
            {
                "get_city" => await HandleGetCityAsync(arguments),
                "get_weather_of_city" => await HandleGetWeatherOfCityAsync(arguments),
                "get_weather_by_city_name" => await HandleGetWeatherByCityNameAsync(arguments),
                _ => throw new ArgumentException($"Unknown tool: {toolName}")
            };

            return new McpResponse
            {
                Id = request.Id,
                Result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = result
                        }
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -1,
                    Message = ex.Message
                }
            };
        }
    }

    private async Task<string> HandleGetCityAsync(JsonElement? arguments)
    {
        if (arguments?.TryGetProperty("cityName", out var cityNameElement) == true)
        {
            var cityName = cityNameElement.GetString();
            var city = await _weatherService.GetCityAsync(cityName ?? "");
            return JsonSerializer.Serialize(city);
        }
        throw new ArgumentException("Missing cityName parameter");
    }

    private async Task<string> HandleGetWeatherOfCityAsync(JsonElement? arguments)
    {
        if (arguments?.TryGetProperty("cityCode", out var cityCodeElement) == true)
        {
            var cityCode = cityCodeElement.GetString();
            var weather = await _weatherService.GetWeatherOfCityByCityCodeAsync(cityCode ?? "");
            return weather.ToString();
        }
        throw new ArgumentException("Missing cityCode parameter");
    }

    private async Task<string> HandleGetWeatherByCityNameAsync(JsonElement? arguments)
    {
        if (arguments?.TryGetProperty("cityName", out var cityNameElement) == true)
        {
            var cityName = cityNameElement.GetString();
            var city = await _weatherService.GetCityAsync(cityName ?? "");
            if (!string.IsNullOrEmpty(city.Code))
            {
                var weather = await _weatherService.GetWeatherOfCityByCityCodeAsync(city.Code);
                return weather.ToString();
            }
            return "城市未找到";
        }
        throw new ArgumentException("Missing cityName parameter");
    }
}
