using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace MyTeam.Models
{
    /// <summary>
    /// 维护需求查询Model
    /// </summary>
    public class ReqQuery
    {
        
        [Display(Name = "系统名称")]
        public int SysId { get; set; }

        [Display(Name = "受理年度")]
        public string AcptYear { get; set; }

        [Display(Name = "受理月份")]
        public string AcptMonth { get; set; }

        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Display(Name = "需求编号")]
        public string ReqDetailNo { get; set; }

        [Display(Name = "下发版本号")]
        public string Version { get; set; }

        [Display(Name = "需求状态")]
        public string ReqStat { get; set; }

        [Display(Name = "需求受理人")]
        public int ReqAcptPerson { get; set; }

        [Display(Name = "不等于")]
        public bool NotEqual { get; set; }

        public IPagedList<Req> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&SysId=").Append(this.SysId)
                .Append("&AcptMonth=").Append(this.AcptMonth)
                .Append("&ReqNo=").Append(this.ReqNo)
                .Append("&ReqDetailNo=").Append(this.ReqDetailNo)
                .Append("&Version=").Append(this.Version)
                .Append("&ReqStat=").Append(this.ReqStat)
                .Append("&ReqAcptPerson=").Append(this.ReqAcptPerson)
                .Append("&NotEqual=").Append(this.NotEqual)
                .Append("&isQuery=True").ToString();
        }

    }
}
