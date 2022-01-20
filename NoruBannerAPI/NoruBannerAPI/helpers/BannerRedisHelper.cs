using NoruBannerAPI.Dtos;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace NoruBannerAPI.helpers
{
    public class BannerRedisHelper
    {
        private readonly IConnectionMultiplexer _redis;

        public BannerRedisHelper(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task SetHashEntry(RedisKey origin, RedisValue key, List<BannerActionData> watchs)
        {
            var redisDb = _redis.GetDatabase();

            HashEntry[] bannerHashValue =
            {
                new HashEntry(key, JsonSerializer.Serialize(watchs))
            };
            await redisDb.HashSetAsync(origin, bannerHashValue);
        }

        public async Task<List<BannerActionData>> GetWatchs(RedisKey origin, RedisValue bannerId)
        {
            var redisDb = _redis.GetDatabase();
            var jsonWatchs = await redisDb.HashGetAsync(origin, bannerId);

            return JsonSerializer.Deserialize<List<BannerActionData>>(jsonWatchs);
        }

        public async Task<List<BannerActionData>> UpdateWatch(RedisKey origin, RedisValue bannerId, BannerActionData newData)
        {
            List<BannerActionData> bannerWatchs = await this.GetWatchs(origin, bannerId);

            if (bannerWatchs.Exists(w => w.ClientIp == newData.ClientIp))
            {
                int index = bannerWatchs.FindIndex(w => w.ClientIp == newData.ClientIp);
                bannerWatchs[index].Watchs++;
                bannerWatchs[index].LastWatchDate = DateTime.Now;
            }
            else
                bannerWatchs.Add(newData);

            return bannerWatchs;
        }
    }
}
