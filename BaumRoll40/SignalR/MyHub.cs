using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BaumRoll40.Models;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BaumRoll40.SignalR
{
    [HubName("chatHub")]
    public class MyHub : Hub
    {
        private BaumRollEntities db = new BaumRollEntities();

        public void SendMessage(string name, string message, int? picId)
        {
            //picId偽装対策
            if (picId != null && picId < 1)
            {
                picId = null;
            }

            int userId = 0;
            //userIdを取得
            if(int.TryParse(HttpContext.Current.User.Identity.Name, out userId))
            {
                //なりすまし対策
                var postUserId = db.Users.Where(u => u.UserName == name).Select(u => u.UserId).FirstOrDefault();
                if(userId != postUserId)
                {
                    name = db.Users.Where(u => u.UserId == userId).Select(u => u.UserName).FirstOrDefault();
                }

                //DB登録＆Viewへ反映
                var managePostModel = new ManagePostModel();
                managePostModel.ReceiveMessage(userId, name, message, picId);
            }

        }

    }
}