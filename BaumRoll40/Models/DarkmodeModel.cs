using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class DarkmodeModel
    {
        private BaumRollEntities db = new BaumRollEntities();

        /// <summary>
        /// だーくもーどの民だったらtrueを返します
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsDarkmode(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var id = int.Parse(userId);

            //IDが変な値だったらfalseになる…はず…
            return db.Users.Where(u => u.UserId == id).Select(u => u.DarkFlag).FirstOrDefault();
        }
    }
}