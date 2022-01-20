using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NoruBannerAPI.Dtos;
using NoruBannerAPI.helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoruBannerAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class BannerController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public BannerController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpPost("watch")]
        public async Task<IActionResult> WatchBanner([FromBody] string bannerId)
        {
            var redisDb = _redis.GetDatabase();
            string origin = Request.Headers["Origin"];
            BannerRedisHelper redisHelper = new(_redis);
            List<BannerActionData> bannerWatchs;

            BannerActionData data = new()
            {
                ClientIp = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                LastWatchDate = DateTime.Now
            };

            if (await redisDb.HashExistsAsync(origin, bannerId))
                bannerWatchs = await redisHelper.UpdateWatch(origin, bannerId, data);
            else
                bannerWatchs = new List<BannerActionData>() { data };

            await redisHelper.SetHashEntry(origin, bannerId, bannerWatchs);

            var result = await redisDb.HashGetAsync(origin, bannerId);

            return Ok(JsonSerializer.Deserialize<List<BannerActionData>>(result));
        }

        [HttpPost("click")]
        public async Task<IActionResult> ClickBanner([FromBody] string bannerId)
        {
            var redisDb = _redis.GetDatabase();
            string origin = Request.Headers["Origin"];
            string clientIp = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            BannerRedisHelper redisHelper = new(_redis);

            List<BannerActionData> bannerWatchs = await redisHelper.GetWatchs(origin, bannerId);
            bannerWatchs.Find(w => w.ClientIp == clientIp).IsClicked = true;

            await redisHelper.SetHashEntry(origin, bannerId, bannerWatchs);

            var result = await redisDb.HashGetAsync(origin, bannerId);

            return Ok(JsonSerializer.Deserialize<List<BannerActionData>>(result));
        }
    }
}
