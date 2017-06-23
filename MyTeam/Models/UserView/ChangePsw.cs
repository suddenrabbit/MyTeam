﻿using System.ComponentModel.DataAnnotations;

namespace MyTeam.Models
{
    public class ChangePsw
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        [StringLength(32, MinimumLength = 6, ErrorMessage = "密码长度最少6位、最长32位")]
        public string NewPsw { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [System.Web.Mvc.Compare("NewPsw", ErrorMessage = "密码必须一致")]
        public string NewPswRep { get; set; }
    }
}