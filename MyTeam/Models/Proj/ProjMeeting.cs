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
        [StringLength(128, ErrorMessage = "不能超过128位")]
        public String MeetingTopic { get; set; }

        [Required]
        [Display(Name = "会议类型")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
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
                    return p == null ? "其他" : p.ProjName;
                }
            }

            set { this.ProjName = value; }
        }
    }
}