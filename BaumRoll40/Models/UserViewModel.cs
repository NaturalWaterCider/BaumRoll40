using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class UserViewModel
    {
        [DisplayName("ユーザーID")]
        public int UserId { get; set; }

        [DisplayName("ユーザー名")]
        public string UserName { get; set; }

        //今はいいやという感じ(ハッシュ化考えるとめんどくさい)
        //[DisplayName("旧パスワード")]
        //public string OldPassWord { get; set; }

        [DisplayName("新パスワード")]
        public string NewPassword1 { get; set; }

        //さばさいど検証になってしまう？
        //[Compare("NewPassword1", ErrorMessage = "パスワードが一致しません。")]
        [DisplayName("新パスワード(確認用)")]
        public string NewPassword2 { get; set; }

    }
}