using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyTeam.Models
{
    public class OutPoolQuery
    {
        [Display(Name = "系统名称")]
        public int SysId { get; set; }

        [Display(Name="版本号")]
        public string Version { get; set; }

        [Display(Name = "维护年度")]
        public string MaintainYear { get; set; }

        public List<OutPoolResult> ResultList { get; set; }
    }
}