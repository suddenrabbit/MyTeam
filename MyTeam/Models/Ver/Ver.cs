using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Controllers;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class Ver
    {
        // main

        [Key]
        public int VerID { get; set; }

        [Required]
        [Display(Name = "系统名称")]
        public int SysId { get; set; }

        [Required]
        [Display(Name = "版本年度")]
        public string VerYear { get; set; }

        [Required]
        [Display(Name = "版本发布频率（月）")]
        public int ReleaseFreq { get; set; }

        [Required]
        [Display(Name = "发布时间（计划）")]
        [DataType(DataType.Date)]
        public DateTime? PublishTime { get; set; }

        [Required]
        [Display(Name = "发布版本号")]
        public string VerNo { get; set; }

        [Required]
        [Display(Name = "版本计划制定时间")]
        [DataType(DataType.Date)]
        public DateTime? DraftTime { get; set; }

        [Required]
        [Display(Name = "版本计划制定人")]
        public int DraftPersonID { get; set; }

        [Required]
        [Display(Name = "版本类别")]
        public string VerType { get; set; }

        [Required]
        [Display(Name = "备注")]
        public string Remark { get; set; }

        [NotMapped]
        public string SysName
        {
            get
            {
                var r = (from a in Constants.SysList
                         where a.SysID == this.SysId
                         select a.SysName).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.SysName = value; }
        }

        [NotMapped]
        public string DraftPersonName
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.DraftPersonID
                         select a.NamePhone).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.DraftPersonName = value; }
        }

    }
}