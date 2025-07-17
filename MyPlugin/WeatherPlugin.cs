using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace MyPlugin;

public class WeatherPlugin
{
    private readonly List<CityModel> _cityList;

    public WeatherPlugin()
    {
        _cityList = new List<CityModel>()
        {
            new CityModel() { Code = "hangzhou", Name = "杭州" },
            new CityModel() { Code = "beijing", Name = "北京" },
            new CityModel() { Code = "shanghai", Name = "上海" }
        };
    }

    [KernelFunction("get_city")]
    [Description("Gets a city by cityName")]
    public async Task<CityModel> GetCityAsync([Description("The name of the city")] string cityName)
    {
        var city = _cityList.Where(a => a.Name == cityName).FirstOrDefault();
        return await Task.FromResult(city ?? new CityModel());
    }

    [KernelFunction("get_weather_of_city")]
    [Description("Get the current weather in a given city.")]
    public static async Task<WeatherModel> GetWeatherOfCityAsync([Description("The city to get weather for")] CityModel city)
    {
        var weather = city.Code switch
        {
            "hangzhou" => new WeatherModel { Temperature = 5, Condition = "下雨" },
            "beijing" => new WeatherModel { Temperature = 10, Condition = "多云" },
            "shanghai" => new WeatherModel { Temperature = 15, Condition = "晴" },
            _ => new WeatherModel { Temperature = 0, Condition = "未知" }
        };
        return await Task.FromResult(weather);
    }

    [KernelFunction("get_weather_of_city_by_city_code")]
    [Description("Get the current weather in a given city by city code.")]
    public async Task<WeatherModel> GetWeatherOfCityByCityCodeAsync([Description("The city code")] string cityCode)
    {
        var weather = cityCode switch
        {
            "hangzhou" => new WeatherModel { Temperature = 5, Condition = "下雨" },
            "beijing" => new WeatherModel { Temperature = 10, Condition = "多云" },
            "shanghai" => new WeatherModel { Temperature = 15, Condition = "晴" },
            _ => new WeatherModel { Temperature = 0, Condition = "未知" }
        };
        return await Task.FromResult(weather);
    }

    [KernelFunction("get_weather_by_city_name")]
    [Description("Get the current weather by city name directly.")]
    public async Task<string> GetWeatherByCityNameAsync([Description("The name of the city")] string cityName)
    {
        var city = await GetCityAsync(cityName);
        if (!string.IsNullOrEmpty(city.Code))
        {
            var weather = await GetWeatherOfCityByCityCodeAsync(city.Code);
            return weather.ToString();
        }
        return "城市未找到";
    }
}

public class CityModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class WeatherModel
{
    public int Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;

    public override string ToString() => $"当前的天气情况为: (温度: {this.Temperature} ℃, 天气: {this.Condition})";
}
