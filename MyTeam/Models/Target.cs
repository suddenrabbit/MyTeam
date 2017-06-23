using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class Target
    {
        [Key]
        public int TID { get; set; }

        [Required]
        [Display(Name = "指标名称")]
        [StringLength(32, ErrorMessage = "不能超过16位")]
        public string TargetName { get; set; }

        [Required]
        [Display(Name = "指标年度")]
        public int TargetYear { get; set; }    

        [Required]
        [Display(Name = "量化目标")]
        public string TargetDesc { get; set; }

        [Required]
        [Display(Name = "评分规则")]
        public string TargetRule { get; set; }

        [Required]
        [Display(Name = "分值")]
        [Range(0,100)]
        public int TargetPoint { get; set; }        
    }
}