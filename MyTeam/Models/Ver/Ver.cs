using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int SysID { get; set; }

        [Required]
        [Display(Name = "版本年度")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public string VerYear { get; set; }

        [Required]
        [Display(Name = "版本发布频率（月）")]
        public int ReleaseFreq { get; set; }

        [Required]
        [Display(Name = "计划版本日期")]
        [DataType(DataType.Date)]
        public DateTime? PublishTime { get; set; }

        [Required]
        [Display(Name = "发布版本号")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public string VerNo { get; set; }

        
        [Display(Name = "实际版本日期")]
        [DataType(DataType.Date)]
        public DateTime? DraftTime { get; set; }

        
        [Display(Name = "版本计划制定人")]
        public int DraftPersonID { get; set; }

        
        [Display(Name = "版本类别")]
        [StringLength(8, ErrorMessage = "不能超过8位")]
        public string VerType { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        [NotMapped]
        public string SysName
        {
            get
            {
                var r = (from a in Constants.SysList
                         where a.SysID == this.SysID
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
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.DraftPersonName = value; }
        }

    }
}