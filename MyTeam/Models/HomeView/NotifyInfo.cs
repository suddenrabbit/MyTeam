
using System.Collections.Generic;

namespace MyTeam.Models
{
    public class NotifyInfo
    {
        public List<ReqMain> InPoolDelay { get; set; }

        public List<HomeProjDelay> ProjDelay { get; set; }
    }
}