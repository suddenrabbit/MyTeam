using System;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    // 入池
    public class InPoolReq
    {
        [Required]
        [Display(Name = "申请编号")]
        public string ReqNo { get; set; }

        [Required]
        [Display(Name = "研发联系人/联系电话")]
        public string ReqDevPerson { get; set; }

        [Required]
        [Display(Name = "研发受理日期")]
        [DataType(DataType.Date)]
        public DateTime? DevAcptDate { get; set; }

        [Required]
        [Display(Name = "研发完成评估日期")]
        [DataType(DataType.Date)]
        public DateTime? DevEvalDate { get; set; }
       
    }
}