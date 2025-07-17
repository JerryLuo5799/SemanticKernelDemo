namespace TestMcpServer;

public class WeatherService
{
    private readonly List<CityModel> _cityList;

    public WeatherService()
    {
        _cityList = new List<CityModel>()
        {
            new CityModel() { Code = "hangzhou", Name = "杭州" },
            new CityModel() { Code = "beijing", Name = "北京" },
            new CityModel() { Code = "shanghai", Name = "上海" }
        };
    }

    public async Task<CityModel> GetCityAsync(string cityName)
    {
        var city = _cityList.Where(a => a.Name == cityName).FirstOrDefault();
        return await Task.FromResult(city ?? new CityModel());
    }

    public async Task<WeatherModel> GetWeatherOfCityByCityCodeAsync(string cityCode)
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
