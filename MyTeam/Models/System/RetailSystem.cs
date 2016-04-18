using System.Linq;
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
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string SysNO { get; set; }

        [Required]
        [Display(Name = "系统名称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string SysName { get; set; }

        [Required]
        [Display(Name = "系统简称")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string SysShortName { get; set; }

        [Required]
        [Display(Name = "主办部门")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string HostDept { get; set; }

        [Display(Name = "二级部门")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string SecondDept { get; set; }

        [Required]
        [Display(Name = "业务联系人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string BusiPerson { get; set; }

        [Required]
        [Display(Name = "研发中心")]
        [StringLength(8)]
        public string DevCenter { get; set; }

        [Required]
        [Display(Name = "研发联系人")]
        [StringLength(16, ErrorMessage = "不能超过16位")]
        public string DevPerson { get; set; }

        [Required]
        [Display(Name = "需求受理人")]
        public int ReqPersonID { get; set; }

        [Display(Name = "需求登记人")]
        public int ReqEditPersonID { get; set; }

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

        [NotMapped]
        public string OldSysName { get; set; }

        [NotMapped]
        public string ReqEditPersonName
        {
            get
            {
                var s = (from a in Constants.UserList
                         where a.UID == this.ReqEditPersonID
                         select a.Realname).FirstOrDefault();
                return s == null ? "未知" : s.ToString();
            }
            set
            {
                this.ReqEditPersonName = value;
            }
        } //用于显示UID对应的名字
    }
}