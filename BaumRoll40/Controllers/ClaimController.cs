using BaumRoll40.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace BaumRoll40.Controllers
{
    [Authorize]
    public class ClaimController : Controller
    {
        private BaumRollEntities db = new BaumRollEntities();
        //private ILog logger = LogManager.GetLogger(Assembly.GetExecutingAssembly().FullName);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // GET: Claim/SendClaim
        public ActionResult SendClaim(string userid)
        {
            var model = new ClaimViewModel();

            if(!string.IsNullOrEmpty(userid))
            {
                model.UserId = int.Parse(userid);
                ViewBag.ResultMsg = "送信しました。管理者の気が向くのをごゆるりとお待ちください。";
            }
            else
            {
                model.UserId = int.Parse(User.Identity.Name);
            }

            model.UserName = db.Users.Where(u => u.UserId == model.UserId).Select(u => u.UserName).FirstOrDefault();

            return View(model);
        }

        // POST: Claim/SendClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendClaim([Bind(Include = "UserId,UserName,Content")]ClaimViewModel model)
        {
            if (db.Users.Find(model.UserId) == null || model.UserId != int.Parse(User.Identity.Name))
            {
                //エラー
                logger.Error("POST: Claim/SendClaim UserIdの取得に失敗");
                ViewBag.ErrorMsg = "ユーザー情報の取得に失敗しました。それでも人間ですか？";
                return View(model);
            }

            //Claimテーブル作成済み。登録してメール送信
            var claim = new Claim {
                UserId = int.Parse(User.Identity.Name),
                Content = model.Content,
                SendTime = DateTime.Now
            };

            db.Claim.Add(claim);

            try
            {
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                //エラー
                logger.Error("Claim/SendClaim POST(DB登録)で例外： " + ex + " 内部例外： " + ex.InnerException);
                ViewBag.ErrorMsg = "お問い合わせ情報の登録に失敗しました。あきらめてください。";
                return View(model);
            }

            try
            {
                var mailText = model.UserName + "さんから\n\n" + model.Content + "\n";
                SendMail(mailText);
            }
            catch (Exception ex)
            {
                //エラー
                logger.Error("Claim/SendClaim POST(メール送信)で例外： " + ex);
                ViewBag.ErrorMsg = "メールの送信に失敗しました。気持ちは伝わっているかもしれません。";
                return View(model);
            }

            return RedirectToAction("SendClaim", new { userid = model.UserId.ToString() });
        }

        private void SendMail(string text)
        {
            string fromAddress = ConfigurationManager.AppSettings["fromAddress"];
            string toAddress = ConfigurationManager.AppSettings["toAddress"];

            // MimeMessageを作り、宛先やタイトルなどを設定する
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress(fromAddress));
            message.To.Add(new MimeKit.MailboxAddress(toAddress));
            // message.Cc.Add(……省略……);
            // message.Bcc.Add(……省略……);
            message.Subject = "*** BaumRoll　お問い合わせ ***";

            // 本文を作る
            var textPart = new MimeKit.TextPart(MimeKit.Text.TextFormat.Plain);
            textPart.Text = string.Format(text);

            // MimeMessageを完成させる
            message.Body = textPart;

            // SMTPサーバに接続してメールを送信する
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                var port = int.Parse(ConfigurationManager.AppSettings["port"]);
                var host = ConfigurationManager.AppSettings["host"];
                var userName = ConfigurationManager.AppSettings["userName"];
                var password = ConfigurationManager.AppSettings["password"];

                //client.Connect(host, port);
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(host, port, MailKit.Security.SecureSocketOptions.Auto);

                client.Authenticate(userName, password);

                client.Send(message);

                client.Disconnect(true);
            }

        }
    }
}