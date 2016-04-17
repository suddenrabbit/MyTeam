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
    public class Req
    {
        // main

        [Key]
        public int RID { get; set; }

        [Required]
        [Display(Name = "系统名称")]
        public int SysId { get; set; }

        [Required]
        [Display(Name = "受理日期")]
        [DataType(DataType.Date)]
        public DateTime AcptDate { get; set; }

        [Required]
        [Display(Name = "申请编号")]
        [StringLength(32, ErrorMessage = "申请编号不能超过32位")]
        public string ReqNo { get; set; }

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

        // detail
        [Display(Name = "维护需求编号")]
        [StringLength(32, ErrorMessage = "维护需求编号不能超过32位")]
        public string ReqDetailNo { get; set; }

        [Display(Name = "下发版本号")]
        [StringLength(8, ErrorMessage = "下发版本号不能超过8位")]
        public string Version { get; set; }

        [Display(Name = "关联业需编号")]
        [StringLength(32, ErrorMessage = "关联业需编号不能超过32位")]
        public string BusiReqNo { get; set; }


        [Display(Name = "需求或问题概述")]
        public string ReqDesc { get; set; }


        [Display(Name = "需求类型")]
        [StringLength(8)]
        public string ReqType { get; set; }

        [Display(Name = "研发评估工作量")]
        public int? DevWorkload { get; set; }

        [Display(Name = "需求状态")]
        [StringLength(8)]
        public string ReqStat { get; set; }

        [Display(Name = "出池日期")]
        [DataType(DataType.Date)]
        public DateTime? OutDate { get; set; }

        [Display(Name = "计划下发日期")]
        [DataType(DataType.Date)]
        public DateTime? PlanRlsDate { get; set; }

        [Display(Name = "主实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? RlsDate { get; set; }

        [Display(Name = "主下发通知编号")]
        [StringLength(32, ErrorMessage = "不能超过32位")]
        public string RlsNo { get; set; }

        [Display(Name = "副下发通知编号")]
        public string SecondRlsNo { get; set; }

        [Display(Name = "副实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? SecondRlsDate { get; set; }

        [Display(Name = "是否有关联系统")]
        public bool IsSysAsso { get; set; }

        [Display(Name = "关联系统名称")]
        public string AssoSysName { get; set; }

        [Display(Name = "关联系统需求编号")]
        public string AssoReqNo { get; set; }

        [Display(Name = "关联系统下发要求")]
        public string AssoRlsDesc { get; set; }

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
        public string OldReqDetailNo { get; set; }

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
        public string ShortReqDesc //需求描述，只显示45个字
        {
            get
            {
                if (this.ReqDesc.Length > 45)
                    return this.ReqDesc.Substring(0, 44) + "...";
                else
                    return this.ReqDesc;
            }
            set
            {
                this.ShortReqDesc = value;
            }
        }

        [NotMapped]
        [Display(Name = "下发通知编号")]
        public string AnyRlsNo //下发通知编号，若存在副下发，两个都输出
        {
            get
            {
                if (!String.IsNullOrEmpty(this.SecondRlsNo))
                    return this.RlsNo + "（主）、" + this.SecondRlsNo + "（副）";
                else
                    return this.RlsNo;
            }
            set
            {
                this.AnyRlsNo = value;
            }
        }


        // 增加创建日期和最近一次更新日期，便于跟踪
        [Display(Name = "创建日期")]
        [StringLength(16)]
        public string CreateTime { get; set; }

        [Display(Name = "最后一次更新日期")]
        [StringLength(16)]
        public string UpdateTime { get; set; }
    }
}