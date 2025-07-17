# TestMcpServer - Semantic Kernel Plugin MCP Server

这是一个基于官方ModelContextProtocol SDK实现的MCP服务器，将MyPlugin项目中的Semantic Kernel插件动态转换为MCP工具。

## 项目结构

```
TestMcpServer/
├── Program.cs                      # 主程序入口，加载MyPlugin并启动MCP服务器
├── McpServerBuilderExtensions.cs   # 扩展方法，将Kernel插件转换为MCP工具
└── TestMcpServer.csproj            # 项目文件
```

## 技术实现

### 核心特性
- **动态插件加载**: 自动加载MyPlugin项目中的所有Semantic Kernel插件
- **官方SDK**: 使用ModelContextProtocol 0.3.0-preview.3官方包
- **扩展方法**: 实现WithTools扩展方法，将Kernel函数转换为MCP工具
- **标准通信**: 支持stdio传输协议

### 自动暴露的工具
服务器会自动将MyPlugin中WeatherPlugin的所有函数暴露为MCP工具：

1. **get_city** - 根据城市名称获取城市信息
2. **get_weather_of_city** - 根据城市对象获取天气信息  
3. **get_weather_of_city_by_city_code** - 根据城市代码获取天气信息
4. **get_weather_by_city_name** - 根据城市名称直接获取天气信息

## 支持的城市

目前支持以下城市：
- 杭州 (hangzhou)
- 北京 (beijing)
- 上海 (shanghai)

## 运行方式

### 1. 直接运行

```bash
cd TestMcpServer
dotnet run
```

### 2. 使用启动脚本

```bash
./start-test-mcp-server.bat
```

### 3. 作为MCP客户端集成

在主程序中配置MCP客户端：

```csharp
McpServerConfig testConfig = new()
{
    Id = "test",
    Name = "TestService",
    TransportType = TransportTypes.StdIo,
    TransportOptions = new()
    {
        ["command"] = "dotnet",
        ["arguments"] = "run --project ../TestMcpServer/TestMcpServer.csproj"
    }
};

var client = await McpClientFactory.CreateAsync(testConfig, options);
```

## 测试

使用提供的测试脚本来验证服务器功能：

```powershell
./test-mcp.ps1
```

## MCP协议通信示例

### 初始化请求
```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "method": "initialize",
  "params": {
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {
      "name": "test-client",
      "version": "1.0.0"
    }
  }
}
```

### 获取工具列表
```json
{
  "jsonrpc": "2.0",
  "id": "2",
  "method": "tools/list",
  "params": {}
}
```

### 调用工具
```json
{
  "jsonrpc": "2.0",
  "id": "3",
  "method": "tools/call",
  "params": {
    "name": "get_weather_by_city_name",
    "arguments": {
      "cityName": "杭州"
    }
  }
}
```

## 与原WeatherPlugin的对比

| 功能 | WeatherPlugin | Test MCP Server |
|------|---------------|-------------------|
| 部署方式 | 集成在主程序中 | 独立进程运行 |
| 通信方式 | 直接方法调用 | JSON-RPC over stdio |
| 可重用性 | 仅限.NET项目 | 跨语言支持 |
| 扩展性 | 需要重新编译 | 独立扩展和部署 |
| 维护性 | 与主程序耦合 | 独立维护 |

## 优势

1. **跨语言支持**: MCP协议是语言无关的，可以被任何支持MCP的客户端调用
2. **独立部署**: 作为独立进程运行，不影响主程序
3. **标准化接口**: 使用标准的MCP协议，便于集成
4. **可扩展性**: 易于添加新的天气功能或支持更多城市
5. **可测试性**: 可以独立测试服务器功能

## 未来扩展

- 添加更多城市支持
- 集成真实的天气API
- 添加天气预报功能
- 支持多语言天气描述
- 添加缓存机制提高性能
