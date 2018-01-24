using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    // 外协人员考勤（加班）情况
    public class MyFile
    {
        [Key]
        public int FileID { get; set; }

        [Required]
        [Display(Name = "文件名")]
        [StringLength(100, ErrorMessage ="文件名最多只能100个字符")]
        public string FileName { get; set; }

        [Required]
        [Display(Name = "文件类型")]
        [StringLength(20, ErrorMessage = "文件名最多只能20个字符")]
        public string FileType { get; set; }

        [Display(Name = "文件大小")]
        public long FileSize { get; set; } // 保存字节数

        [Required]
        [Display(Name = "内部文件名")]
        public string FileHashName { get; set; } // 对文件hash之后得到的内部文件名，避免上传的文件名不规范

        [Display(Name = "上传人")]
        public int PersonID { get; set; }

        [Display(Name = "下载次数")]
        public int DownloadTimes { get; set; }

        [Display(Name = "创建日期")]
        public DateTime CreateTime { get; set; }

        [Display(Name = "更新日期")]
        public DateTime UpdateTime { get; set; }

        [NotMapped]
        public string PersonName
        {
            get
            {
                return Utils.MyTools.GetUserName(PersonID);
            }
            set
            {
                this.PersonName = value;
            }
        } //用于显示UID对应的名字        
    }
}