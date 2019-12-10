using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class PostViewModel
    {
        public string UserName { get; set; }

        public int PageNo { get; set; }

        public int AllPageNo { get; set; }

        public List<Post> PostList { get; set; }

        public string SearchWord { get; set; }

        public List<Post> SearchedPostList { get; set; }
    }
}