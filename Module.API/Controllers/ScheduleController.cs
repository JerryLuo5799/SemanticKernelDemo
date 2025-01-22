using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Module.API.Controllers
{
    /// <summary>
    /// 用户日程相关接口
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        /// <summary>
        /// 获取用户今天的日程数量
        /// </summary>
        /// <returns>当前用户今天的日程数量</returns>
        [HttpGet]
        [Route("GetScheduleCount")]
        public string GetScheduleCount()
        {
            Random random = new Random();
            return $"你 {DateTime.Now.ToString("yyyy-MM-dd")} 有 {random.Next(10)} 个日程";
        }
    }
}
