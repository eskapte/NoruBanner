using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoruBannerAPI.Dtos
{
    public class BannerActionData
    {
        public string ClientIp { get; set; }
        public DateTime LastWatchDate { get; set; }
        public bool IsClicked { get; set; } = false;
        public int Watchs { get; set; } = 1;
    }
}
