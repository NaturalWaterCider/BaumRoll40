using BaumRoll40.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace BaumRoll40.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        //private ILog logger = LogManager.GetLogger(Assembly.GetExecutingAssembly().FullName);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        readonly CustomMembershipProvider membershipProvider = new CustomMembershipProvider();
        private BaumRollEntities db = new BaumRollEntities();

        // ロガーの取得
        // (補足)log4net.config の appender@name に
        // 引数で渡した名前の appender は存在しないので root が取得される
        // logger.Debug("デバッグ：処理の中間情報");
        // logger.Info("情報：操作履歴");
        // logger.Warn("注意：現在は正常に動作が続けられるがデータ不整合の可能性がある操作。");
        // logger.Error("エラー：復旧可能な障害。継続できるが処理はロールバックされる。");
        // logger.Fatal("障害：アプリケーションが強制終了する障害");

        // GET: Login
        public ActionResult Index()
        {
            //ひつまぶし
            var strArray = new string[] { string.Empty, string.Empty, string.Empty };
            var joined = string.Join(",", strArray);
            System.Diagnostics.Debug.WriteLine(joined);

            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "UserId,Password")] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (this.membershipProvider.ValidateUser(model.UserId, model.PassWord))
                {
                    string passhash = membershipProvider.GeneratePasswordHash(model.UserId, model.PassWord);

                    var id = int.Parse(model.UserId);
                    var user = db.Users
                        .Where(u => u.UserId.Equals(id) && u.Password.Equals(passhash))
                        .FirstOrDefault();

                    if (user != null)
                    {
                        FormsAuthentication.SetAuthCookie(user.UserId.ToString(), false);
                        logger.Info("ログイン成功：ID " + model.UserId + "/ IP " + GetClientIP());
                        return RedirectToAction("Index", "Home", new { id = 1 });
                    }
                }
            }

            // ログ出力
            logger.Info("ログイン失敗：ID " + model.UserId + "/ IP " + GetClientIP());
            ViewBag.ErrorMsg = "ログインに失敗しました。";
            return View(model);
        }

        // GET: Login/SignOut
        public ActionResult SignOut()
        {
            logger.Info("ログアウト：ID " + User.Identity.Name);
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }


        /// <summary>
        /// クライアントのIPアドレス取得
        /// </summary>
        /// <returns></returns>
        private string GetClientIP()
        {
            var clientIp = "";
            var xForwardedFor = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(xForwardedFor) == false)
            {
                clientIp = xForwardedFor.Split(',').GetValue(0).ToString().Trim();
            }
            else
            {
                clientIp = Request.UserHostAddress;
            }

            if (clientIp != "::1"/*localhost*/)
            {
                clientIp = clientIp.Split(':').GetValue(0).ToString().Trim();
            }

            //var ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            //var clientIp = System.Net.Dns.GetHostEntry(ipAddress).HostName;

            return clientIp;
        }
    }
}