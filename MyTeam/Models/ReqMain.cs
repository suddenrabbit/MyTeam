using MyTeam.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    public class ReqMain
    {
        // 需求主体部分

        [Key]
        public int ReqMainID { get; set; }

        [Required]
        [Display(Name = "系统名称")]
        public int SysID { get; set; }

        [Required]
        [Display(Name = "申请编号")]
        [StringLength(32, ErrorMessage = "申请编号不能超过32位")]
        public string ReqNo { get; set; } // 唯一索引

        [Display(Name = "受理日期")]
        [DataType(DataType.Date)]
        public DateTime? AcptDate { get; set; }

        [Required]
        [Display(Name = "需求申请事由")]
        public string ReqReason { get; set; }

        [Required]
        [Display(Name = "需求发起单位")]
        [StringLength(16)]
        public string ReqFromDept { get; set; }

        [Required]
        [Display(Name = "需求发起人/联系电话")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ReqFromPerson { get; set; }

        [Required]
        [Display(Name = "需求受理人/联系电话")]
        public int ReqAcptPerson { get; set; } //数据库存储UID，页面显示根据User表

        [Display(Name = "研发联系人/联系电话")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ReqDevPerson { get; set; }

        [Display(Name = "业务测试人/联系电话")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string ReqBusiTestPerson { get; set; }

        [Display(Name = "研发受理日期")]
        [DataType(DataType.Date)]
        public DateTime? DevAcptDate { get; set; }

        [Display(Name = "研发完成评估日期")]
        [DataType(DataType.Date)]
        public DateTime? DevEvalDate { get; set; }     
        
        public virtual ICollection<ReqDetail> ReqDetails { get; set; } //与ReqDetail形成外键关系

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
        public string ReqAcptPersonNamePhone
        {
            get
            {
                var r = (from a in Constants.UserList
                         where a.UID == this.ReqAcptPerson
                         select a.NamePhone).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set { this.ReqAcptPersonNamePhone = value; }
        }        

        [NotMapped]
        public string ShortReqReason //需求申请事由，只显示25个字
        {
            get
            {
                if (this.ReqReason.Length > 25)
                    return this.ReqReason.Substring(0, 24) + "...";
                else
                    return this.ReqReason;
            }
            set
            {
                this.ShortReqReason = value;
            }
        }
        
        [NotMapped]
        public string OldReqNo { get; set; }
    }
}