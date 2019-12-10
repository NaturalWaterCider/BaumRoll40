using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class LoginViewModel
    {
        [DisplayName("ユーザーID")]
        [RegularExpression(@"[0-9]+", ErrorMessage = "半角数字のみ入力できます")]
        public string UserId { get; set; }

        [DisplayName("パスワード")]
        public string PassWord { get; set; }
    }
}