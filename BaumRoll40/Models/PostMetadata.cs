using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    [MetadataType(typeof(PostMetadata))]
    public partial class Post
    {
        public string UserName { get; set; }

        public int PageNo { get; set; }
    }

    public class PostMetadata
    {
    }
}