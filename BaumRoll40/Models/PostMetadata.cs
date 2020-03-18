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

        public Post()
        {

        }

        public Post(string message, int postId, int userid, int? picId)
        {
            PostId = postId;
            UserId = userid;
            Content = message;
            PictureId = picId;
            PostTime = DateTime.Now;
        }

        public Post(int postId, int userid, string username, string message, DateTime posttime, int? picId)
        {
            PostId = postId;
            UserId = userid;
            UserName = username;
            Content = message;
            PictureId = picId;
            PostTime = posttime;
        }
    }

    public class PostMetadata
    {
    }
}