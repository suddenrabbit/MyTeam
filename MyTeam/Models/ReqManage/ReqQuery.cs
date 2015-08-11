using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    /// <summary>
    /// 维护需求查询Model
    /// </summary>
    public class ReqQuery
    {
        
        [Display(Name = "系统名称")]
        public int SysId { get; set; }

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

        public List<Req> ResultList { get; set; }

    }
}
