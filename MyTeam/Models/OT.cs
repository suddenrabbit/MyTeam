using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{
    // 外协人员考勤（加班）情况
    public class OT
    {
        [Key]
        public int OTID { get; set; }

        [Required]
        [Display(Name = "加班日期")]
        [DataType(DataType.Date)]
        public DateTime OTDate { get; set; }

        [Required]
        [Display(Name = "加班人员")]
        public int PersonID { get; set; }

        [Display(Name = "加班小时数")]
        public double OTHours { get; set; }

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

        [NotMapped]
        public string OTDateString
        {
            get
            {
                return OTDate.ToString("yyyy/MM");
            }
            set
            {
                OTDateString = value;
            }
        }
    }
}