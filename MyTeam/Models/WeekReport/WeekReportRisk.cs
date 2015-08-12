using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class WeekReportRisk
    {
        [Key]
        public int WRRiskID { get; set; }

        [Required]
        [Display(Name = "周报日期")]
        [DataType(DataType.Date)]
        public string RptDate { get; set; }

        [Required]
        [Display(Name = "风险或待协调问题")]
        public string RiskDetail { get; set; }

        [Display(Name = "解决情况")]
        public string Solution { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }
    }
}