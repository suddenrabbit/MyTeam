using MyTeam.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
        public int UserType { get; set; } 

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
                return Enum.GetName(typeof(UserTypeEnums), UserType);
            } 
            set 
            { 
                this.UserTypeName = value;
            } 
        }

        [Display(Name = "分管人")]
        public int BelongTo { get; set; }

        [NotMapped]
        public string BelongToName
        {
            get
            {
                if(this.UserType != (int)UserTypeEnums.外协 )
                {
                    return "暂不适用";
                }
                var r = (from a in Utils.Constants.UserList
                         where a.UID == this.BelongTo
                         select a.Realname).FirstOrDefault();

                return r == null ? "未知" : r.ToString();
            }
            set
            {
                this.UserTypeName = value;
            }
        }

    }

    
}