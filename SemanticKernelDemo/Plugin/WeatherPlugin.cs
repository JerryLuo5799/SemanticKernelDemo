using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelDemo.Plugin
{
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
        [return: Description("A city")]
        public async Task<CityModel> GetCityAsync(string cityName)
        {
            var city = _cityList.Where(a => a.Name == cityName).FirstOrDefault();
            return city ?? new CityModel();
        }

        [KernelFunction("get_weather_of_city")]
        [Description("Get the current weather in a given city.")]
        [return: Description("The current weather of given city")]
        public async Task<WeatherModel> GetWeatherOfCityAsync(CityModel city)
        {
            return city.Code switch
            {
                "hangzhou" => new WeatherModel { Temperature = 5, Condition = "下雨" },
                "beijing" => new WeatherModel { Temperature = 10, Condition = "多云" },
                "shanghai" => new WeatherModel { Temperature = 15, Condition = "晴" },
                _ => new WeatherModel { Temperature = 0, Condition = "未知" }
            };
        }
    }

    public class CityModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class WeatherModel
    {
        public int Temperature { get; set; }
        public string Condition { get; set; }

        public string ToString() => $"当前的天气情况为: (温度: {this.Temperature} ℃, 天气: {this.Condition})";
    }
}


