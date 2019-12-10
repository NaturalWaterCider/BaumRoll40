using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class ClaimViewModel
    {
        public int UserId { get; set; }

        [DisplayName("ユーザー名")]
        public string UserName { get; set; }

        [DisplayName("要望・お問い合わせ・苦情等何でもござれ")]
        public string Content { get; set; }

    }
}