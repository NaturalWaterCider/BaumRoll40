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

        public bool IsFav { get; set; }

        public int? FavNum { get; set; }

        public Post()
        {

        }

        //作成用
        public Post(string message, int postId, int userid, int? picId)
        {
            PostId = postId;
            UserId = userid;
            Content = message;
            PictureId = picId;
            PostTime = DateTime.Now;
        }

        //検索用
        public Post(int postId, int userid, string username, string message, DateTime posttime, int? picId, bool isFav, int? favNum)
        {
            PostId = postId;
            UserId = userid;
            UserName = username;
            Content = message;
            PictureId = picId;
            PostTime = posttime;
            IsFav = isFav;
            FavNum = favNum;
        }
    }

    public class PostMetadata
    {
    }
}