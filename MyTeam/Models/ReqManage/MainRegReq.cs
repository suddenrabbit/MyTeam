using System;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class MainRegReq
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

        /*
         * 
        [Required]
        [Display(Name = "研发联系人/联系电话")]
        public string ReqDevPerson { get; set; }
         
        [Display(Name = "研发受理日期")]
        [DataType(DataType.Date)]
        public DateTime? DevAcptDate { get; set; }

        [Display(Name = "研发完成评估日期")]
        [DataType(DataType.Date)]
        public DateTime? DevEvalDate { get; set; }
         * */

        [Required]
        [Range(1,10, ErrorMessage="需求数量必须是1-10之间的数字")]
        [Display(Name = "需求数量")]
        public int ReqAmt { get; set; }
    }
}