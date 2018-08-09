using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class WeekReportRisk
    {
        [Key]
        public int WRRiskID { get; set; }

        [Required]
        [Display(Name = "填报周期")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RptDate { get; set; }

        [Required]
        [Display(Name = "风险或待协调问题")]
        public string RiskDetail { get; set; }

        [Display(Name = "解决建议")] // 因周报模板调整，故中英文不完全匹配
        public string Solution { get; set; }

        [Required]
        [Display(Name = "填报人")]
        public int RptPersonID { get; set; }
    }
}