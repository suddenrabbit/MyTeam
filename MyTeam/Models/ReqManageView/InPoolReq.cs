using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    // 入池
    public class InPoolReq
    {
        
        [Display(Name = "更新受理日期")]
        [DataType(DataType.Date)]
        public DateTime? NewAcptDate { get; set; }

        [Display(Name = "更新申请编号")]
        [StringLength(32, ErrorMessage = "申请编号不能超过32位")]
        public string NewReqNo { get; set; }

        public ReqMain ReqMain { get; set; }

        public List<ReqDetail> ReqDetails { get; set; }
    }
}