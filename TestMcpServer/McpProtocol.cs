using System.Text.Json;

namespace TestMcpServer;

public class McpRequest
{
    public string Jsonrpc { get; set; } = "2.0";
    public string? Id { get; set; }
    public string Method { get; set; } = "";
    public JsonElement? Params { get; set; }
}

public class McpResponse
{
    public string Jsonrpc { get; set; } = "2.0";
    public string? Id { get; set; }
    public object? Result { get; set; }
    public McpError? Error { get; set; }
}

public class McpError
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }
}
