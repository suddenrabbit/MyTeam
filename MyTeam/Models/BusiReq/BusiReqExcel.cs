using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyTeam.Models
{
    public class BusiReqExcel
    {
        [Display(Name = "项目名称")]
        public string ProjName { get; set; }

        [Display(Name = "业需编号")]
        public string BusiReqNo { get; set; }

        [Display(Name = "业需名称")]
        public string BusiReqName { get; set; }

        [Display(Name = "功能描述")]
        public string Desc { get; set; }

        [Display(Name = "创建日期")]
        public DateTime? CreateDate { get; set; }

        [Display(Name = "需求来源及状态")]
        public string Stat { get; set; }

        [Display(Name = "受理日期")]
        [DataType(DataType.Date)]
        public DateTime? AcptDate { get; set; }

        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Display(Name = "需求编号")]
        public string ReqDetailNo { get; set; }

        [Display(Name = "下发版本号")]
        public string Version { get; set; }

        [Display(Name = "需求申请事由")]
        public string ReqReason { get; set; }

        [Display(Name = "需求或问题概述")]
        public string ReqDesc { get; set; }

        [Display(Name = "需求发起人/联系电话")]
        public string ReqFromPerson { get; set; }

        [Display(Name = "需求类型")]
        public string ReqType { get; set; }

        [Display(Name = "实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? RlsDate { get; set; }
    }
}