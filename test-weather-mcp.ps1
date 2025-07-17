#!/usr/bin/env pwsh

# 测试 Weather MCP 服务器
Write-Host "Testing Weather MCP Server..."

# 启动 MCP 服务器进程
$mcpProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project WeatherMcpServer/WeatherMcpServer.csproj" -NoNewWindow -PassThru -RedirectStandardInput -RedirectStandardOutput -RedirectStandardError

# 等待服务器启动
Start-Sleep -Seconds 3

# 发送初始化请求
$initRequest = @{
    jsonrpc = "2.0"
    id = "1"
    method = "initialize"
    params = @{
        protocolVersion = "2024-11-05"
        capabilities = @{}
        clientInfo = @{
            name = "test-client"
            version = "1.0.0"
        }
    }
} | ConvertTo-Json -Depth 10

Write-Host "Sending initialize request:"
Write-Host $initRequest

$mcpProcess.StandardInput.WriteLine($initRequest)

# 等待响应
Start-Sleep -Seconds 1

# 发送工具列表请求
$toolsRequest = @{
    jsonrpc = "2.0"
    id = "2"
    method = "tools/list"
    params = @{}
} | ConvertTo-Json -Depth 10

Write-Host "Sending tools/list request:"
Write-Host $toolsRequest

$mcpProcess.StandardInput.WriteLine($toolsRequest)

# 等待响应
Start-Sleep -Seconds 1

# 发送工具调用请求
$toolCallRequest = @{
    jsonrpc = "2.0"
    id = "3"
    method = "tools/call"
    params = @{
        name = "get_weather_by_city_name"
        arguments = @{
            cityName = "杭州"
        }
    }
} | ConvertTo-Json -Depth 10

Write-Host "Sending tools/call request:"
Write-Host $toolCallRequest

$mcpProcess.StandardInput.WriteLine($toolCallRequest)

# 等待响应
Start-Sleep -Seconds 1

# 关闭服务器
$mcpProcess.StandardInput.Close()
$mcpProcess.WaitForExit(5000)
if (!$mcpProcess.HasExited) {
    $mcpProcess.Kill()
}

Write-Host "Test completed."
