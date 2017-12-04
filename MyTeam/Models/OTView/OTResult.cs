using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    // 外协人员加班情况
    public class OTResult
    {
       
        [Display(Name = "加班日期")]
        public string OTDate { get; set; }

        [Required]
        [Display(Name = "加班人员")]
        public string PersonName { get; set; }

        [Display(Name = "加班小时数")]
        public double OTHours { get; set; }     

       
    }
}