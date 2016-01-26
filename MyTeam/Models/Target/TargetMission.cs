using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    /// <summary>
    /// 考核量化目标的对应任务
    /// </summary>
    public class TargetMission
    {

        [Key]
        public int TMID { get; set; }

        [Required]
        [Display(Name = "指标名称")]
        public int TID { get; set; }

        [Required]
        [Display(Name = "主办人员")]
        public int PersonID { get; set; }

        [Display(Name = "协办人员")]
        public string SidePerson { get; set; }

        [Required]
        [Display(Name = "任务内容")]
        public string Mission { get; set; }
       
        [Display(Name = "完成情况")]
        public string Stat { get; set; }

        [NotMapped]
        public string TargetName {get; set;}

        [NotMapped]
        public string PersonName
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.PersonID
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.PersonName = value; }
        }
    }
}