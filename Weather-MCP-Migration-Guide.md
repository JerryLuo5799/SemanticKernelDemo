# Weather Plugin 转换为 MCP 服务指南

## 项目概述

本项目成功将原有的 `WeatherPlugin` 转换为基于 Model Context Protocol (MCP) 的独立服务（TestMcpServer）。这样做带来了以下优势：

### 转换前后对比

| 方面 | 原 WeatherPlugin | TestMcpServer（MCP服务） |
|------|------------------|-------------------------|
| 部署方式 | 集成在主程序中 | 独立进程运行 |
| 通信方式 | 直接方法调用 | JSON-RPC over stdio |
| 可重用性 | 仅限当前.NET项目 | 跨语言/跨项目支持 |
| 维护性 | 与主程序耦合 | 独立维护和测试 |
| 扩展性 | 需要重新编译主程序 | 独立扩展不影响主程序 |

## 项目结构

```
SemanticKernelDemo/
├── SemanticKernelDemo/                    # 主程序
│   ├── Program.cs                         # 集成MCP客户端
│   └── Plugin/
│       └── WeatherPlugin.cs               # 原始插件（保留）
├── TestMcpServer/                         # 重命名后的MCP服务器
│   ├── Program.cs                         # 服务器入口
│   ├── McpServer.cs                       # MCP协议实现
│   ├── McpProtocol.cs                     # MCP数据结构
│   ├── WeatherService.cs                 # 天气服务逻辑
│   ├── TestMcpServer.csproj              # 项目文件
│   └── README.md                          # 详细文档
├── Module.API/                           # API模块
├── start-test-mcp-server.bat             # 启动脚本
├── test-mcp.ps1                          # 测试脚本
└── SemanticKernelDemo.sln                # 解决方案文件（包含所有项目）
```

## 快速开始

### 1. 运行TestMcpServer MCP服务器

```bash
# 方法1: 直接运行
cd TestMcpServer
dotnet run

# 方法2: 使用启动脚本
start-test-mcp-server.bat

# 方法3: 通过解决方案运行
dotnet run --project TestMcpServer/TestMcpServer.csproj
```

### 2. 运行主程序（集成MCP客户端）

```bash
cd SemanticKernelDemo
dotnet run
```

### 3. 测试交互

主程序运行后，你可以测试以下功能：

```
User > 今天杭州的天气怎么样？
User > 北京现在的温度是多少？
User > 上海的天气如何？
```

## MCP服务提供的功能

TestMcpServer提供三个工具：

1. **get_city** - 根据城市名称获取城市信息
2. **get_weather_of_city** - 根据城市代码获取天气
3. **get_weather_by_city_name** - 直接根据城市名称获取天气

## 支持的城市

- 杭州 (hangzhou)
- 北京 (beijing) 
- 上海 (shanghai)

## 技术细节

### MCP协议通信

MCP服务器使用标准输入输出进行JSON-RPC通信：

```json
// 初始化请求
{
  "jsonrpc": "2.0",
  "id": "1", 
  "method": "initialize",
  "params": { "protocolVersion": "2024-11-05" }
}

// 工具调用请求
{
  "jsonrpc": "2.0",
  "id": "3",
  "method": "tools/call", 
  "params": {
    "name": "get_weather_by_city_name",
    "arguments": { "cityName": "杭州" }
  }
}
```

### 主程序集成

在主程序中通过以下配置集成TestMcpServer：

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

## 扩展和自定义

### 添加新城市

在 `WeatherService.cs` 中的 `_cityList` 添加新城市：

```csharp
_cityList.Add(new CityModel() { Code = "shenzhen", Name = "深圳" });
```

并在 `GetWeatherOfCityByCityCodeAsync` 方法中添加对应的天气数据。

### 添加新功能

1. 在 `WeatherService.cs` 中添加新的服务方法
2. 在 `McpServer.cs` 的 `HandleToolsList` 中注册新工具
3. 在 `HandleToolCallAsync` 中添加新工具的处理逻辑

### 集成真实天气API

替换 `WeatherService.cs` 中的模拟数据，调用真实的天气API服务。

## 测试

### 使用PowerShell测试脚本：

```powershell
./test-mcp.ps1
```

该脚本会测试TestMcpServer的初始化、工具列表和工具调用功能。

### 通过解决方案编译和运行：

```bash
# 编译整个解决方案
dotnet build

# 运行TestMcpServer
dotnet run --project TestMcpServer

# 运行主程序
dotnet run --project SemanticKernelDemo
```

## 注意事项

1. **端口冲突**: 确保没有其他程序占用标准输入输出
2. **路径设置**: 主程序中的MCP服务器路径需要正确
3. **依赖项**: 确保所有NuGet包都已正确安装
4. **进程管理**: MCP服务器作为子进程运行，主程序退出时会自动清理

## 后续改进方向

1. **真实天气数据**: 集成OpenWeatherMap或其他天气API
2. **缓存机制**: 添加天气数据缓存减少API调用
3. **更多城市**: 支持全球城市天气查询
4. **天气预报**: 添加未来几天的天气预报功能
5. **多语言支持**: 支持中英文等多语言天气描述
6. **错误处理**: 完善错误处理和重试机制

这个转换展示了如何将传统的插件架构升级为更加灵活和可扩展的MCP服务架构。
