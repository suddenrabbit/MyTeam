using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;
using PagedList;
using System.Text;

namespace MyTeam.Models
{
    public class ProjMeeting
    {

        [Key]
        public int MeetingID { get; set; }

        [Required]
        [Display(Name = "项目名称")]
        public int ProjID { get; set; }

        [Required]
        [Display(Name = "会议议题")]
        public String MeetingTopic { get; set; }

        [Required]
        [Display(Name = "会议类型")]
        public String MeetingType { get; set; }


        [Display(Name = "参加部门/主办部门")]
        public String HostDept { get; set; }


        [Display(Name = "会议组织者/需求联系人")]
        public String HostPerson { get; set; }


        [Display(Name = "评审专家")]
        public String ReviewExpert { get; set; }


        [Display(Name = "参会人员")]
        public String Participants { get; set; }

        [Required]
        [Display(Name = "会议日期")]
        [DataType(DataType.Date)]
        public DateTime MeetingDate { get; set; }


        [Display(Name = "会议通知编号")]
        public String NoticeNo { get; set; }


        [Display(Name = "评审结论")]
        public String ReviewConclusion { get; set; }


        [Display(Name = "会议结论")]
        public String MeetingConclusion { get; set; }


        [Display(Name = "当前状态")]
        public String Stat { get; set; }


        [Display(Name = "备注")]
        public String Remark { get; set; }

        [NotMapped]
        public string ProjName
        {
            get
            {
                // ProjID转name
                using (MyTeamContext dbContext = new MyTeamContext())
                {
                    var p = dbContext.Projs.ToList().Find(a => a.ProjID == this.ProjID);
                    return p == null ? "未知" : p.ProjName;
                }
            }

            set { this.ProjName = value; }
        }

        public IPagedList<ProjMeeting> ResultList { get; set; }

        public string ToQueryString()
        {
            return new StringBuilder("&ProjID=").Append(this.ProjID)
                .Append("&MeetingTopic=").Append(this.MeetingTopic)
                .Append("&MeetingType=").Append(this.MeetingType)
                .Append("&HostDept=").Append(this.HostDept)
                .Append("&HostPerson=").Append(this.HostPerson)
                .Append("&ReviewExpert=").Append(this.ReviewExpert)
                .Append("&Participants=").Append(this.Participants)
                .Append("&MeetingTopic=").Append(this.MeetingDate)
                .Append("&MeetingType=").Append(this.NoticeNo)
                .Append("&HostDept=").Append(this.ReviewConclusion)
                .Append("&HostPerson=").Append(this.MeetingConclusion)
                .Append("&ReviewExpert=").Append(this.Stat)
                .Append("&Participants=").Append(this.Remark)
                .Append("&isQuery=True").ToString();
        }
    }
}