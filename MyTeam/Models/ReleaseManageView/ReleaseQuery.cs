using PagedList;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{
    public class ReleaseQuery
    {        
        [Display(Name = "下发通知编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ReleaseNo { get; set; } 

        [Display(Name = "计划下发日期起始")]
        public string PlanReleaseDateStart { get; set; }

        [Display(Name = "计划下发日期结束")]
        public string PlanReleaseDateEnd { get; set; }

        [Display(Name = "实际下发日期起始")]
        public string ReleaseDateStart { get; set; }

        [Display(Name = "实际下发日期起始")]
        public string ReleaseDateEnd { get; set; }

        public IPagedList<ReqRelease> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&isQuery=True&ReleaseNo=").Append(this.ReleaseNo)
                .Append("&PlanReleaseDateStart=").Append(this.PlanReleaseDateStart)
                .Append("&PlanReleaseDateEnd=").Append(this.PlanReleaseDateEnd)
                .Append("&ReleaseDateStart=").Append(this.ReleaseDateStart)
                .Append("&ReleaseDateEnd=").Append(this.ReleaseDateEnd)               
                .ToString();
        }
    }
}