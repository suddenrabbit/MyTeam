﻿using System.Collections.Generic;

namespace MyTeam.Models
{
    public class FullReq
    {
        public ReqMain reqMain { get; set; }
        public ReqDetail reqDetail { get; set; }
        public ReqRelease reqRelease { get; set; }
    }
}