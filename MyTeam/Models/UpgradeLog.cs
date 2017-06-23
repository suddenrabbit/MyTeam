using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyTeam.Models
{
    /// <summary>
    /// 版本更新日志
    /// </summary>
    public class UpgradeLog
    {
        [Key]
        public int LogID { get; internal set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; internal set; }

        [Required]
        [StringLength(8, ErrorMessage = "The SysVersion is too long!")]
        public string SysVersion { get; internal set; }

        [Required]
        [StringLength(500, ErrorMessage = "The Description is too long!")]
        public string Description { get; internal set; }
    }
}