using MyTeam.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyTeam.Models
{
    /// <summary>
    /// 跟踪任务的干系人
    /// </summary>
    public class TrackTaskPerson
    {
        [Key]
        public int TrackTaskPersonID { get; set; }

        [ForeignKey("TrackTask")]
        public int TrackTaskID { get; set; }
        public virtual TrackTask TrackTask { get; set; }

        public int PersonID { get; set; }

        [NotMapped]
        public string PersonName
        {
            get
            {
                var n = (from a in Constants.UserList
                         where a.UID == PersonID select a.Realname).FirstOrDefault();

                return n == null ? "未知" : n.ToString();
            }
            set { PersonName = value; }
        }

    }
}