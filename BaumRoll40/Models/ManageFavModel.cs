using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class ManageFavModel
    {
        private BaumRollEntities db = new BaumRollEntities();

        public bool Fav(int postId)
        {
            int userId = 0;
            if (int.TryParse(HttpContext.Current.User.Identity.Name, out userId) && db.Post.Find(postId) != null)
            {
                var fav = db.Fav.Find(postId, userId);

                if (fav == null)     //ふぁぼ
                {
                    var newFav = new Fav
                    {
                        PostId = postId,
                        UserId = userId
                    };

                    db.Fav.Add(newFav);
                }
                else    //ふぁぼ解除
                {
                    db.Fav.Remove(fav);
                }

                try
                {
                    db.SaveChanges();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

    }
}