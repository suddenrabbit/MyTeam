using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{

    public class TrackTaskCreateEdit
    {
        public TrackTask TrackTask { get; set; }

        [Display(Name = "项目干系人")]
        public string[] TaskPersons { get; set; }

        public string OldTaskPersons { get; set; } // 用以编辑的时候比较新旧值

    }
}