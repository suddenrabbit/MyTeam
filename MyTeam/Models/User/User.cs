using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyTeam.Models
{

    public class User
    {
        [Key]
        public int UID { get; set; }

        [Required]
        [Display(Name = "用户名")]
        [StringLength(20, ErrorMessage = "用户名最长不能超过20位")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        [StringLength(32, MinimumLength=6, ErrorMessage="密码长度最少6位、最长32位")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "真实姓名")]
        [StringLength(10, ErrorMessage = "真实姓名最长不能超过10位")]
        public string Realname { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "电话号码")]
        [StringLength(6, ErrorMessage = "电话号码不能超过6位")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "管理员", Description = "管理员可以修改系统参数和管理用户")]
        public bool IsAdmin { get; set; }

        [Display(Name = "用户类别")]
        public int UserType { get; set; } //0-系统用户 1-行员 2-外协

        [NotMapped]
        public string NamePhone { 
            get
            {
                return this.Realname + "/" + this.Phone;
            }
            set
            {
                string[] s = value.Split('/');
                this.Realname = s[0];
                this.Phone = s[1];
            }
        }

        [NotMapped]
        public string UserTypeName { 
            get 
            {
                if (this.UserType == 0)
                    return "系统用户";
                else if (this.UserType == 1)
                    return "行员";
                else if (this.UserType == 2)
                    return "外协";
                else
                    return "未知";
            } 
            set 
            { 
                this.UserTypeName = value;
            } 
        }
        //0-系统用户 1-行员 2-外协
    }

    
}