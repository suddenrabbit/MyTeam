using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    // 入池
    public class InPoolReq
    { 
        public ReqMain ReqMain { get; set; }

        public List<ReqDetail> ReqDetails { get; set; }
    }
}