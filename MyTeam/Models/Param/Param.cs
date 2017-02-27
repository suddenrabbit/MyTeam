using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class Param
    {
        [Key]
        public int ParamID { get; set; }

        [Required]
        [Display(Name = "参数类别")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string ParamType { get; set; }

        [Required]
        [Display(Name = "参数值")]
        [StringLength(4, ErrorMessage = "不能超过32位")]
        public string ParamValue { get; set; }

        [Required]
        [Display(Name = "中文名称")]
        [StringLength(16, ErrorMessage = "不能超过32位")]
        public string ParamName { get; set; }

        [Required]
        [Display(Name = "参数说明")]
        [StringLength(32, ErrorMessage = "不能超过16位")]
        public string ParamDesc { get; set; }       
    }
}