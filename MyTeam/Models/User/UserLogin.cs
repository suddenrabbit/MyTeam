using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class UserLogin
    {
        [Required]
        [Display(Name = "用户名")]
        [StringLength(20, ErrorMessage = "用户名最长不能超过20位")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; }
    }
}