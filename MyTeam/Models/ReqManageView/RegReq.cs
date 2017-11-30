using System;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class RegReq
    {
        [Required]
        [Display(Name = "系统名称")]
        public int SysID { get; set; }

        [Required]
        [Display(Name = "受理日期")]
        [DataType(DataType.Date)]
        public DateTime AcptDate { get; set; }

        [Required]
        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Required]
        [Display(Name = "需求申请事由")]
        public string ReqReason { get; set; }

        [Required]
        [Display(Name = "需求发起单位")]
        public string ReqFromDept { get; set; }

        [Required]
        [Display(Name = "需求发起人/联系电话")]
        public string ReqFromPerson { get; set; }

        [Required]
        [Display(Name = "需求受理人/联系电话")]
        public int ReqAcptPerson { get; set; }

        [Required]
        [Display(Name = "业务测试人/联系电话")]
        public string ReqBusiTestPerson { get; set; }         

        [Required]
        [Range(1,50, ErrorMessage="需求数量必须是1-50之间的数字")]
        [Display(Name = "需求数量")]
        public int ReqAmt { get; set; }

        public string[] ReqDescs { get; set; } 
    }

    /*public class DetailRegReq
    {
        [Display(Name = "需求或问题概述")]
        public string ReqDesc { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }
    }*/
}