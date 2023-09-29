using BaumRoll40.SignalR;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;

namespace BaumRoll40.Models
{
    public class ManagePostModel
    {
        private BaumRollEntities db = new BaumRollEntities();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const string kenji = "kenji";
        private const string caramel = "caramel";
        private const string choco = "choco";
        private const int maxNum = 10000;

        /// <summary>
        /// 受け取った投稿をDB登録＆画面へ反映する
        /// ここへのルートは現状 View→MyHub or API(SendMsg)　のどちらか
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <param name="picId"></param>
        public void ReceiveMessage(int userId, string name, string postmessage, int? picId)
        {
            var message = CheckMessageLength(postmessage);

            var iconSrc = "/Content/Icon/" + userId + ".png";
            if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath(iconSrc)))
            {
                iconSrc = "/Content/Icon/noicon-user.png";
            }

            var postId = db.Post.Max(p => p.PostId) + 1;
            Post post = new Post(message, postId, userId, picId);
            RemoveOverPost();
            db.Post.Add(post);

            //SignalR側に通知
            var context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();

            try
            {
                db.SaveChanges();

                context.Clients.All.broadcastMessage(iconSrc, name, message, post.PostTime.ToString("yyyy/MM/dd HH:mm:ss"), picId, postId);
            }
            catch (Exception ex)
            {
                logger.Error("投稿の登録に失敗： " + ex + "\\n内部例外： " + ex.InnerException);
                context.Clients.All.broadcastMessage(iconSrc, name, message + "\\n[ERROR]登録エラー。再読込するとこの投稿は消えるかもしれません。再度投稿してみてください。", post.PostTime.ToString("yyyy/MM/dd HH:mm:ss"));
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

                    AImessage = CheckMessageLength(AImessage);

                }
                catch (Exception ex)
                {
                    logger.Error("APIとの通信中にエラー ： " + ex + "\\n内部例外： " + ex.InnerException);
                    AImessage = "[ERROR] お昼寝中…( ˘ω˘)ｽﾔｧ";
                }

                //postIdは直前に指定しないと同時更新エラー吐きがち
                var AIpostId = db.Post.Max(p => p.PostId) + 1;
                Post AIpost = new Post(AImessage, AIpostId, AIuserId, null);
                RemoveOverPost();
                db.Post.Add(AIpost);

                try
                {
                    db.SaveChanges();

                    context.Clients.All.broadcastMessage(AIconSrc, AIname, AImessage, AIpost.PostTime.ToString("yyyy/MM/dd HH:mm:ss"));
                }
                catch (Exception ex)
                {
                    logger.Error("投稿の登録に失敗： " + ex + "\\n内部例外： " + ex.InnerException);
                    context.Clients.All.broadcastMessage(AIconSrc, AIname, AImessage + "\\n[ERROR] 登録エラー。再読込するとこの投稿は消えるかもしれません。再度投稿してみてください。", AIpost.PostTime.ToString("yyyy/MM/dd HH:mm:ss"));
                }
            }

            //OneSignalで通知送れないかなー
            Notificator notificator = new Notificator();
            notificator.PushNotification(userId.ToString());
        }

        /// <summary>
        /// 文字数チェック
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string CheckMessageLength(string message)
        {
            var newmessage = message.Length > 200 ? "[懺悔] 私は200文字を超えて投稿しようとしました。" : message;
            return newmessage;
        }


        /// <summary>
        /// 所持上限超えたPost(およびそれに紐づくPictureとFav)をDBから削除する
        /// </summary>
        private void RemoveOverPost()
        {
            if (db.Post.Count() >= maxNum)
            {
                var minPost = db.Post.Find(db.Post.Min(p => p.PostId));
                //トランザクションいるかなぁ
                if (minPost.PictureId != null)
                {
                    db.Picture.Remove(db.Picture.Find(minPost.PictureId));
                }
                var delfavs = db.Fav.Where(x => x.PostId == minPost.PostId);
                db.Fav.RemoveRange(delfavs);
                db.Post.Remove(minPost);
            }
        }

        /// <summary>
        /// すぺしゃるさんくすつもりくんandわたなべくん
        /// AIとお話できる
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string TalkWithAI(string message, string AIname)
        {
            string postData = JsonConvert.SerializeObject(message.Replace("@" + AIname + " ", ""));
            string url = (AIname == choco) ? ConfigurationManager.AppSettings["wAddress"] : ConfigurationManager.AppSettings["tAddress"] + AIname;

            //POST送信する文字列を作成
            //バイト型配列に変換
            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(postData);

            WebProxy proxyObject = new WebProxy("http://proxy.nssys.co.jp:8080/", true);

            //WebRequestの作成
            System.Net.WebRequest req =
                System.Net.WebRequest.Create(url);
            req.Proxy = proxyObject;
            req.Method = "POST";
            req.ContentType = "application/json";

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