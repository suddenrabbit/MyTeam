using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyTeam.Utils;

namespace MyTeam.Models
{
    public class RetailSystem
    {
        [Key]
        public int SysID { get; set; }

        [Required]
        [Display(Name = "系统编号")]
        public string SysNO { get; set; }

        [Required]
        [Display(Name = "系统名称")]
        public string SysName { get; set; }

        [Required]
        [Display(Name = "系统简称")]
        public string SysShortName { get; set; }

        [Required]
        [Display(Name = "主办部门")]
        public string HostDept { get; set; }

        [Display(Name = "二级部门")]
        public string SecondDept { get; set; }

        [Required]
        [Display(Name = "业务联系人")]
        public string BusiPerson { get; set; }

        [Required]
        [Display(Name = "研发中心")]
        public string DevCenter { get; set; }

        [Required]
        [Display(Name = "研发联系人")]
        public string DevPerson { get; set; }

        [Required]
        [Display(Name = "维护需求受理人")]
        public int ReqPersonID { get; set; }

        [NotMapped]
        public string ReqPersonName
        {
            get
            {
                var s = (from a in Constants.UserList
                         where a.UID == this.ReqPersonID
                         select a.Realname).FirstOrDefault();
                return s == null ? "未知" : s.ToString();
            }
            set
            {
                this.ReqPersonName = value;
            }
        } //用于显示UID对应的名字
    }
}