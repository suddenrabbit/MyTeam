using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    // 统计
    public class AttendanceSumUp
    {      
        [Display(Name = "人员")]
        public int PersonID { get; set; }

        [Display(Name = "年度总计请假天数")]
        public double LeaveDays { get; set; }

        [Display(Name = "年度总计加班小时数")]
        public double OTHours { get; set; }

        [Display(Name = "加班折抵天数")]
        public double OTAsDays { get; set; }

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