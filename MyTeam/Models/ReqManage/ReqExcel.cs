using System;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class ReqExcel
    {
        
        [Display(Name = "系统名称")]
        public string SysName { get; set; }
        
        [Display(Name = "受理日期")]
        public DateTime? AcptDate { get; set; }

        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Display(Name = "需求编号")]
        public string ReqDetailNo { get; set; }

        [Display(Name = "下发版本号")]
        public string Version { get; set; }

        [Display(Name = "关联业需编号")]
        public string BusiReqNo { get; set; }

        
        [Display(Name = "需求申请事由")]
        public string ReqReason { get; set; }

        [Display(Name = "需求或问题概述")]
        public string ReqDesc { get; set; }

        
        [Display(Name = "需求发起单位")]
        public string ReqFromDept { get; set; }

        
        [Display(Name = "需求发起人/联系电话")]
        public string ReqFromPerson { get; set; }

        
        [Display(Name = "需求受理人/联系电话")]
        public string ReqAcptPerson { get; set; } 

        
        [Display(Name = "研发联系人/联系电话")]
        public string ReqDevPerson { get; set; }

        
        [Display(Name = "业务测试人/联系电话")]
        public string ReqBusiTestPerson { get; set; }

        [Display(Name = "需求类型")]
        public string ReqType { get; set; }

        [Display(Name = "研发受理日期")]
        public DateTime? DevAcptDate { get; set; }

        [Display(Name = "研发完成评估日期")]
        public DateTime? DevEvalDate { get; set; }

        [Display(Name = "研发评估工作量")]
        public int? DevWorkload { get; set; }

        [Display(Name = "需求状态")]
        public string ReqStat { get; set; }

        [Display(Name = "出池日期")]
        public DateTime? OutDate { get; set; }

        [Display(Name = "计划下发日期")]
        public DateTime? PlanRlsDate { get; set; }

        [Display(Name = "实际下发日期")]
        public DateTime? RlsDate { get; set; }

        [Display(Name = "下发通知编号")]
        public string RlsNo { get; set; }

        [Display(Name = "是否有关联系统")]
        public string IsSysAsso { get; set; }

        [Display(Name = "关联系统名称")]
        public string AssoSysName { get; set; }

        [Display(Name = "关联系统需求编号")]
        public string AssoReqNo { get; set; }

        [Display(Name = "关联系统下发要求")]
        public string AssoRlsDesc { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }  
    }
}