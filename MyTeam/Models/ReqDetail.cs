using MyTeam.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    public class ReqDetail
    {
        // 维护需求

        [Key]
        public int ReqDetailID { get; set; }
        
        [Display(Name = "维护需求编号")]
        [StringLength(32, ErrorMessage = "维护需求编号不能超过32位")]
        public string ReqDetailNo { get; set; } // 唯一索引

        [ForeignKey("ReqMain")]
        public int ReqMainID { get; set; }
        public virtual ReqMain ReqMain { get; set; } // 跟ReqMain形成外键关系

        [Display(Name = "下发版本号")]
        [StringLength(8, ErrorMessage = "下发版本号不能超过8位")]
        public string Version { get; set; }

        [Display(Name = "需求概述")]
        public string ReqDesc { get; set; }

        [Display(Name = "需求类型")]        
        public int ReqType { get; set; }

        [Display(Name = "研发评估工作量")]
        public double? DevWorkload { get; set; }

        [Display(Name = "需求状态")]
        public int ReqStat { get; set; }

        [Display(Name = "出池日期")]
        [DataType(DataType.Date)]
        public DateTime? OutDate { get; set; }

        [Display(Name = "下发通知")]
        public int ReqReleaseID { get; set; }

        [Display(Name = "副下发通知")]
        public int SecondReqReleaseID { get; set; }

        [Display(Name = "是否有关联系统")]
        [System.ComponentModel.DefaultValue(false)]
        public bool IsSysAsso { get; set; }

        [Display(Name = "关联系统名称")]
        public string AssoSysName { get; set; }

        [Display(Name = "关联系统需求编号")]
        public string AssoReqNo { get; set; }

        [Display(Name = "关联系统下发要求")]
        public string AssoReleaseDesc { get; set; }

        [Display(Name = "备注")]
        public string Remark { get; set; }

        // 增加创建日期和最近一次更新日期，便于跟踪
        [Display(Name = "创建日期")]
        [DataType(DataType.DateTime)]
        public DateTime CreateTime { get; set; }

        [Display(Name = "最后一次更新日期")]
        [DataType(DataType.DateTime)]
        public DateTime UpdateTime { get; set; }


        [NotMapped]
        public string OldReqDetailNo { get; set; }

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
        public string ReqTypeName 
        {
            get
            {
                return Enum.GetName(typeof(ReqTypeEnums), ReqType);
            }
            set
            {
                this.ReqTypeName = value;
            }
        }

        [NotMapped]
        public string ReqStatName
        {
            get
            {
                return Enum.GetName(typeof(ReqStatEnums), ReqStat);
            }
            set
            {
                this.ReqStatName = value;
            }
        }

        [NotMapped]
        public string ReqReleaseNo { get; set; } //下发通知编号

        // TODO:将下发通知中的计划下发日期、实际下发日期也同时存储在ReqDetail中。当更新下发通知中的计划下发日期、实际下发日期，同时更新需求中的信息。
        /*[Display(Name = "计划下发日期")]
        [DataType(DataType.Date)]
        public DateTime? PlanRlsDate { get; set; }

        [Display(Name = "实际下发日期")]
        [DataType(DataType.Date)]
        public DateTime? RlsDate { get; set; }*/

    }
}