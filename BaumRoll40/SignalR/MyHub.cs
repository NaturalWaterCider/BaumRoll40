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
        //private ILog logger = LogManager.GetLogger(Assembly.GetExecutingAssembly().FullName);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const string kenji = "kenji";
        private const string caramel = "caramel";
        private const string choco = "choco";
        private const int maxNum = 10000;

        public void SendMessage(string name, string message, int? picId)
        {
            if (picId != null && picId < 1)
            {
                picId = null;
            }

            int userId = 0;
            var postId = db.Post.Max(p => p.PostId) + 1;

            //userIdを取得
            if(int.TryParse(HttpContext.Current.User.Identity.Name, out userId))
            {
                //なりすまし対策
                var postUserId = db.Users.Where(u => u.UserName == name).Select(u => u.UserId).FirstOrDefault();
                if(userId != postUserId)
                {
                    name = db.Users.Where(u => u.UserId == userId).Select(u => u.UserName).FirstOrDefault();
                }

                var iconSrc = "/Content/Icon/" + userId + ".png";
                if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath(iconSrc)))
                {
                    iconSrc = "/Content/Icon/noicon-user.png";
                }

                Post post = MakePost(message, postId, userId, picId);
                db.Post.Add(post);

                try
                {
                    db.SaveChanges();

                    Clients.All.broadcastMessage(iconSrc, name, message, post.PostTime.ToString("yyyy/MM/dd HH:mm:ss"), picId);
                }
                catch (Exception ex)
                {
                    logger.Error("MyHub/SendMessage 投稿の登録に失敗： " + ex + "\n内部例外： " + ex.InnerException);
                    Clients.All.broadcastMessage(iconSrc, name, message + "\n[ERROR]登録エラー。再読込するとこの投稿は消えるかもしれません。再度投稿してみてください。", post.PostTime.ToString("yyyy/MM/dd HH:mm:ss"));
                }


                //AIとの会話があった場合
                if (!string.IsNullOrEmpty(message) && message.IndexOf("@") == 0)
                {
                    var AImessage = "";
                    var AIname = "";
                    var AIuserId = 0;
                    var AIconSrc = "";

                    try
                    {
                        //賢治
                        if (message.IndexOf("@" + kenji + " ") == 0)
                        {
                            AIuserId = 917;
                            AIname = "賢治";
                            AIconSrc = "/Content/Icon/" + AIuserId + ".png";
                            AImessage = TalkWithAI(message, kenji);
                        }
                        //キャラ子
                        else if (message.IndexOf("@" + caramel + " ") == 0)
                        {
                            AIuserId = 919;
                            AIname = "キャラメル";
                            AIconSrc = "/Content/Icon/" + AIuserId + ".png";
                            AImessage = TalkWithAI(message, caramel);
                        }
                        //しょこら
                        else if (message.IndexOf("@" + choco + " ") == 0)
                        {
                            AIuserId = 1024;
                            AIname = "しょこら";
                            AIconSrc = "/Content/Icon/" + AIuserId + ".png";
                            AImessage = TalkWithAI(message, choco);
                        }
                        else
                        {
                            return;
                        }

                    }
                    catch(Exception ex)
                    {
                        logger.Error("MyHub/SendMessage/TalkWithAI ： " + ex + "\n内部例外： " + ex.InnerException);
                        AImessage = "[ERROR] APIとの通信中にエラーが発生しました。";
                    }

                    //postIdは直前に指定しないと同時更新エラー吐きがち
                    var AIpostId = db.Post.Max(p => p.PostId) + 1;
                    Post AIpost = MakePost(AImessage, AIpostId, AIuserId, null);
                    db.Post.Add(AIpost);

                    try
                    {
                        db.SaveChanges();

                        Clients.All.broadcastMessage(AIconSrc, AIname, AImessage, AIpost.PostTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    }
                    catch (Exception ex)
                    {
                        logger.Error("MyHub/SendMessage 投稿の登録に失敗： " + ex + "\n内部例外： " + ex.InnerException);
                        Clients.All.broadcastMessage(AIconSrc, AIname, AImessage + "\n[ERROR] 登録エラー。再読込するとこの投稿は消えるかもしれません。再度投稿してみてください。", AIpost.PostTime.ToString("yyyy/MM/dd HH:mm:ss"));
                    }


                }

                //OneSignalで通知送れないかなー
                Notificator notificator = new Notificator();
                notificator.PushNotification(userId.ToString());
            }

        }

        private Post MakePost(string message, int postId, int userid, int? picId)
        {
            var post = new Post
            {
                PostId = postId,
                UserId = userid,
                Content = message,
                PictureId = picId,
                PostTime = DateTime.Now
            };

            if (db.Post.Count() > maxNum)
            {
                var minPost = db.Post.Find(db.Post.Min(p => p.PostId));
                db.Post.Remove(minPost);
            }

            return post;
        }

        /// <summary>
        /// すぺしゃるさんくすつもりくんandわたなべくん
        /// AIとお話できる
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string TalkWithAI(string message, string AIname)
        {
            string postData = message.Replace("@" + AIname + " ", "");
            string url = (AIname == choco) ? "http://172.23.8.76/chat" : "http://54.238.95.64/chatbot/" + AIname;

            //POST送信する文字列を作成
            //バイト型配列に変換
            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(postData);

            WebProxy proxyObject = new WebProxy("http://proxy.nssys.co.jp:8080/", true);
            
            //WebRequestの作成
            System.Net.WebRequest req =
                System.Net.WebRequest.Create(url);
            req.Proxy = proxyObject;
            req.Method = "POST";
            req.ContentType = "text/plain";

            //データをPOST送信するためのStreamを取得
            System.IO.Stream reqStream = req.GetRequestStream();
            //送信するデータを書き込む
            reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            reqStream.Close();

            //サーバーからの応答を受信するためのWebResponseを取得
            System.Net.WebResponse res = req.GetResponse();
            //応答データを受信するためのStreamを取得
            System.IO.Stream resStream = res.GetResponseStream();
            //受信
            System.IO.StreamReader sr = new System.IO.StreamReader(resStream);
            var reply = sr.ReadToEnd();
            //閉じる
            sr.Close();

            return reply;
        }
    }
}