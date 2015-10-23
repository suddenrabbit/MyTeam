using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class OutPoolResultExcel
    {
        [Display(Name = "受理月份")]
        public string AcptMonth { get; set; }

        [Display(Name = "系统名称")]
        public string SysName { get; set; }

        [Display(Name = "下发版本号")]
        public string Version { get; set; }

        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Display(Name = "需求编号")]
        public string ReqDetailNo { get; set; }

        [Display(Name = "需求申请事由")]
        public string ReqReason { get; set; }

        [Display(Name = "需求或问题概述")]
        public string ReqDesc { get; set; }

        [Display(Name = "研发评估工作量（人天）")]
        public int? DevWorkload { get; set; }

        [Display(Name = "研发联系人/联系电话")]
        public string ReqDevPerson { get; set; }

        [Display(Name = "业务测试人/联系电话")]
        public string ReqBusiTestPerson { get; set; }

        [Display(Name = "需求类型")]
        public string ReqType { get; set; }

        [Display(Name = "计划下发日期")]
        [DataType(DataType.Date)]
        public DateTime? PlanRlsDate { get; set; }

        [Display(Name = "实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? RlsDate { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }        
    }
}