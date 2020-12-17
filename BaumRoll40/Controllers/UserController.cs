using BaumRoll40.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BaumRoll40.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private BaumRollEntities db = new BaumRollEntities();
        //private ILog logger = LogManager.GetLogger(Assembly.GetExecutingAssembly().FullName);
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        readonly CustomMembershipProvider membershipProvider = new CustomMembershipProvider();

        // GET: User
        public ActionResult Index(string userid)
        {
            var model = new UserViewModel();

            if (!string.IsNullOrEmpty(userid))
            {
                model.UserId = int.Parse(userid);
                ViewBag.ResultMsg = "設定を変更しました。";
            }
            else
            {
                model.UserId = int.Parse(User.Identity.Name);
            }

            model.UserName = db.Users.Where(u => u.UserId == model.UserId).Select(u => u.UserName).FirstOrDefault();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index([Bind(Include ="UserId,UserName,NewPassword1,NewPassword2")]UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.NewPassword1) || string.IsNullOrEmpty(model.NewPassword2))
            {
                //error
                ViewBag.ErrorMsg = string.Format("パスワードを入力してください。");
                return View(model);
            }

            if (model.NewPassword1 != model.NewPassword2)
            {
                //error
                ViewBag.ErrorMsg = string.Format("パスワードが一致しません。");
                return View(model);
            }

            // パスワードをハッシュ化
            string hash = membershipProvider.GeneratePasswordHash(model.UserId.ToString(), model.NewPassword1);

            Users user = db.Users.Find(model.UserId);
            if(user == null)
            {
                //error
                ViewBag.ErrorMsg = string.Format("ユーザーが見つかりません。どういうことなの");
                return View(model);
            }

            user.Password = hash;

            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            logger.Info("パスワード変更：ID " + model.UserId);

            return RedirectToAction("Index", new { userid = user.UserId });
        }

        public ActionResult ChangeIcon()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangeIcon(string id)
        {
            try
            {
                HttpPostedFileBase iconFile = Request.Files["lubera"];

                if (iconFile != null && iconFile.ContentType != "" && iconFile.ContentType.Contains("image/"))
                {
                    //ファイルアップロード
                    var upIconFilePath = Server.MapPath($"~/Content/Icon/{User.Identity.Name}.png");
                    iconFile.SaveAs(upIconFilePath);
                    HttpResponse.RemoveOutputCacheItem("/Home/Index/");
                    ViewBag.ResultMsg = string.Format("アイコン画像が登録できた！よ！");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChangeIcon 画像登録失敗 by" + User.Identity.Name + "：" + ex);
                ViewBag.ErrorMsg = string.Format("アイコン画像が登録できなかった！よ！");
            }

            return View();
        }

        public ActionResult ChangeMode()
        {
            var id = int.Parse(User.Identity.Name);
            var user = db.Users.Find(id);

            if(user == null)
            {
                logger.Error("ChangeMode 失敗 by" + User.Identity.Name + "：ゆーざーが見つかりません。迷子です。");
            }

            user.DarkFlag = !user.DarkFlag;
            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Error("ChangeMode 失敗 by" + User.Identity.Name + "：" + ex);
            }

            return RedirectToAction("Index", "Home", new { id = 1 });
        }

        /// <summary>
        /// くりえいとあかうんと
        /// 当面は自分用　気が向いたら権限とかいれて
        /// 
        /// ↑の理由につきエラーチェック甘め
        /// </summary>
        /// <returns></returns>
        public ActionResult Crenata8Red()
        {
            if(User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            var viewModel = new UserViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Crenata8Red(UserViewModel viewModel)
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ POST：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            if (viewModel.NewPassword1 != viewModel.NewPassword2)
            {
                //error
                ViewBag.ErrorMsg = string.Format("パスワードが一致しません。");
                return View(viewModel);
            }

            // パスワードをハッシュ化
            string hash = membershipProvider.GeneratePasswordHash(viewModel.UserId.ToString(), viewModel.NewPassword1);

            Users user = new Users
            {
                UserId = viewModel.UserId,
                UserName = viewModel.UserName,
                Password = hash,
                DarkFlag = false
            };

            db.Users.Add(user);
            //同時更新エラーは起きないはず
            db.SaveChanges();

            return RedirectToAction("Crenata8Red");
        }


        /// <summary>
        /// ajax前提　ユーザー名取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetUserName()
        {
            var id = int.Parse(User.Identity.Name);
            var userName = db.Users.Where(u => u.UserId == id).Select(u => u.UserName).FirstOrDefault();

            return Content(userName);
        }

    }
}