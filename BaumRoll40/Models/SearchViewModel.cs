using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class SearchViewModel
    {
        public string SearchWord { get; set; }

        public List<Post> SearchedPostList { get; set; }
    }
}