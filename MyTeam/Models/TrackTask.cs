using MyTeam.Utils;
using System;
using System.Collections.Generic;
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

        public virtual ICollection<TrackTaskPerson> TrackTaskPersons { get; set; } // 任务与干系人，一对多

        [Required]
        [Display(Name = "任务创建日期")]
        [DataType(DataType.Date)]
        public DateTime CreateDate { get; set; }

        [Required]
        [Display(Name = "任务状态")]  // 1-进行中  2-已完成 3-已暂停
        public int TrackTaskStat { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [NotMapped]
        [Display(Name = "任务干系人")]
        public string TaskPersonNames
        {
            get
            {
                if(TrackTaskPersons == null)
                {
                    return "数据缺失";
                }

                List<string> names = new List<string>();
                foreach (var r in TrackTaskPersons)
                {
                    names.Add(r.PersonName);
                }

                return string.Join(", ", names.ToArray());
            }
            set { TaskPersonNames = value; }
        }

        [NotMapped]
        public string TrackTaskStatName
        {
            get
            {
                switch (TrackTaskStat)
                {
                    case 1: return "进行中";
                    case 2: return "已完成";
                    case 3: return "已暂停";
                    default: return "未知(" + TrackTaskStat + ")";
                }
            }
            set { this.TrackTaskStatName = value; }
        }
    }
}