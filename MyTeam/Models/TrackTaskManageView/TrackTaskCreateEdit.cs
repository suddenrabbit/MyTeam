using PagedList;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTeam.Models
{

    public class TrackTaskCreateEdit
    {
        public TrackTask TrackTask { get; set; }

        [Display(Name = "项目干系人")]
        public string[] TaskPersons { get; set; }
       
    }
}