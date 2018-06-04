using MyTeam.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    /// <summary>
    /// 跟踪任务
    /// </summary>
    public class TrackTask
    {
        [Key]
        public int TrackTaskID { get; set; }

        [Required]
        [Display(Name = "任务名称")]
        [StringLength(64, ErrorMessage = "任务名称不能超过64位")]
        public string TrackTaskName { get; set; }

        [Required]
        [Display(Name = "任务创建人")]
        public int CreatePersonID { get; set; }

        [Required]
        [Display(Name = "任务创建日期")]
        public DateTime CreateDate { get; set; }

        [Required]
        [Display(Name = "任务状态")]
        public int TrackTaskStat { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [NotMapped]
        public string CreatePersonName
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.CreatePersonID
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.CreatePersonName = value; }
        }
    }
}