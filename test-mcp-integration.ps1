#!/usr/bin/env pwsh

# 测试 TestMcpServer 功能
Write-Host "测试 TestMcpServer..." -ForegroundColor Green

# 启动 TestMcpServer
$mcpProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "TestMcpServer/TestMcpServer.csproj" -WorkingDirectory "." -PassThru -WindowStyle Hidden

# 等待服务器启动
Start-Sleep -Seconds 3

Write-Host "TestMcpServer 进程 ID: $($mcpProcess.Id)" -ForegroundColor Yellow

# 运行 SemanticKernelDemo
Write-Host "启动 SemanticKernelDemo..." -ForegroundColor Green

try {
    $testInput = "杭州的天气如何?"
    Write-Host "测试输入: $testInput" -ForegroundColor Cyan
    
    # 这里可以添加自动化测试逻辑
    Write-Host "请手动运行 SemanticKernelDemo 并测试天气查询功能" -ForegroundColor Yellow
    
} catch {
    Write-Host "测试过程中出现错误: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    # 清理进程
    if ($mcpProcess -and !$mcpProcess.HasExited) {
        Write-Host "停止 TestMcpServer..." -ForegroundColor Yellow
        Stop-Process -Id $mcpProcess.Id -Force
    }
}

Write-Host "测试完成" -ForegroundColor Green
