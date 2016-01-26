using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class TargetMissionResult
    {
        // 得分
        public List<TargetMissionPoint> pointLs { get; set; }

        // 主办的
        public List<TargetMissionQuery> HostLs { get; set; }

        // 协办的
        public List<TargetMissionQuery> SideLs { get; set; }
    }

    public class TargetMissionPoint
    {
        public int PID { get; set; }

        public int Point { get; set; }

        public string Name
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.PID
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.Name = value; }
        }
    }

    public class TargetMissionQuery
    {
        [Key]
        public int TMID { get; set; }

        [Display(Name = "指标名称")]
        public int TID { get; set; }

        [Display(Name = "主办人员")]
        public int PersonID { get; set; }

        [Display(Name = "协办人员")]
        public string SidePerson { get; set; }

        [Display(Name = "任务内容")]
        public string Mission { get; set; }

        [Display(Name = "完成情况")]
        public string Stat { get; set; }

        public string TargetName { get; set; }

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

        public int TargetYear { get; set; }
    }
}