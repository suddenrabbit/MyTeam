using System;
using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    /// <summary>
    /// 版本更新日志
    /// </summary>
    public class UpgradeLog
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "发布日期")]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [Display(Name = "版本号")]
        [StringLength(8, ErrorMessage = "版本号不能超过8位")]
        public string SysVersion { get; set; }

        [Required]
        [Display(Name = "更新说明")]
        [StringLength(500, ErrorMessage = "更新说明不能超过500字")]
        public string Description { get; set; }
    }
}