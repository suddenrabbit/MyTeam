using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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

        public IPagedList<OutPoolResult> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&SysId=").Append(this.SysId)
                .Append("&Version=").Append(this.Version)
                .Append("&MaintainYear=").Append(this.MaintainYear)
                .Append("&isQuery=True").ToString();
        }
    }
}