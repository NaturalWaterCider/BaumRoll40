using BaumRoll40.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BaumRoll40.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private BaumRollEntities db = new BaumRollEntities();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private const int PAGE_1NUM = 100;

        // GET: Home
        public ActionResult Index(string id)
        {
            int iid;
            if(!int.TryParse(id, out iid))
            {
                iid = 0;
            }

            var model = new PostViewModel();

            var userid = int.Parse(User.Identity.Name);
            model.UserName = db.Users.Where(u => u.UserId == userid).Select(u => u.UserName).FirstOrDefault();
            model.PostList = GetAllPostList();

            var quoti = model.PostList.Count / PAGE_1NUM;
            var remainder = model.PostList.Count % PAGE_1NUM;
            int fromNo = 0;
            int endNo = 0;
            model.AllPageNo = quoti + 1;

            //PostListの取得範囲決定
            if(iid < 1 || model.AllPageNo < iid)       //範囲外の数値
            {
                endNo = PAGE_1NUM;
                model.PageNo = 1;
                ViewBag.ErrorMsg = string.Format("範囲外のページが指定されたため、1ページ目を表示しています。");
                logger.Info("Home/Index 範囲外のページ指定 by" + model.UserName + " ぱらめた:" +id);
            }
            else if(iid == model.AllPageNo)            //最後のページ
            {
                fromNo = (iid - 1) * PAGE_1NUM ;
                endNo = fromNo + remainder;
                model.PageNo = iid;
            }
            else if(iid <= quoti)                //1～最後の1つ前のページ
            {
                fromNo = (iid - 1) * PAGE_1NUM;
                endNo = fromNo + PAGE_1NUM;
                model.PageNo = iid;
            }

            model.PostList = model.PostList.GetRange(fromNo, endNo - fromNo);

            return View(model);
        }


        [HttpGet]
        public ActionResult Search(string searchWord)
        {
            var model = new SearchViewModel();
            model.SearchWord = GetNotEscapeHTMLStr(searchWord);

            try
            {
                var allPostList = GetAllPostList();
                allPostList = allPostList.Where(p => !string.IsNullOrEmpty(p.Content)).ToList();
                var allpageNo = allPostList.Count() / PAGE_1NUM + 1;

                //検索
                //こんなこまごまやらんでも
                //var curComp = CultureInfo.CurrentCulture.CompareInfo;
                //var cmpOption = CompareOptions.IgnoreCase | // 大文字と小文字を無視
                //    CompareOptions.IgnoreKanaType | // ひらがなとカタカナを無視
                //    CompareOptions.IgnoreWidth | // 半角と全角を無視
                //    CompareOptions.IgnoreSymbols; // 空白文字・句読点・その他の記号を無視
                //var searchedList = from p in allPostList where 0 <= curComp.IndexOf(p.Content, model.SearchWord, cmpOption) select p;
                //model.SearchedPostList = searchedList.ToList();
                model.SearchedPostList = allPostList.Where(x => x.Content.Contains(model.SearchWord)).ToList();
                model.SearchedPostList.ForEach(s => { s.PageNo = GetPageNo(s.PostId, allpageNo); });

            }
            catch (Exception ex)
            {
                logger.Info("Home/Searchにてエラー : " + ex);
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }


            return PartialView(model);

        }

        //git 変更テスト
        //↑このこめんとなに

        /// <summary>
        /// 投稿を全件取得
        /// </summary>
        /// <returns></returns>
        private List<Post> GetAllPostList()
        {
            var query = from p in db.Post
                        join u in db.Users
                        on p.UserId equals u.UserId
                        select new
                        {
                            p.PostId,
                            p.UserId,
                            u.UserName,
                            p.Content,
                            p.PostTime,
                            p.PictureId
                        };

            var list = new List<Post>();
            query.ToList().ForEach(item =>
            {
                Post post = new Post(item.PostId,item.UserId, item.UserName, item.Content, item.PostTime, item.PictureId);
                list.Add(post);
            });

            return list.OrderByDescending(p => p.PostTime).ToList();
        }

        /// <summary>
        /// xss対策用にエスケープした文字を元に戻す
        /// とてもねむい
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetNotEscapeHTMLStr(string str)
        {
            str = str.Replace('＆', '&');
            str = str.Replace('＜', '<');
            str = str.Replace('＞', '>');
            str = str.Replace('”', '"');
            str = str.Replace('’', '\'');
            return str;

        }

        /// <summary>
        /// 指定されたPostのPageNoを取得する
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public int GetPageNo(int postId, int allpageNo)
        {
            var pageNo = db.Post.OrderByDescending(p => p.PostId).Select(p => p.PostId.ToString()).ToList().FindIndex(p => p.Equals(postId.ToString())) / PAGE_1NUM + 1;

            return pageNo > allpageNo ? allpageNo : pageNo;
        }

        /// <summary>
        /// 画像拡大表示用モーダルをだすよ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShadowBox(string id)
        {
            var errorMsg = "";

            int picid = 0;
            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out picid))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            Picture picture = db.Picture.Find(picid);
            if(picture == null)
            {
                //そんなことあるんだろうか
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            ViewBag.ErrorMsg = errorMsg;
            return PartialView(picture);
        }

        public ActionResult SearchFile(string path)
        {
            if (System.IO.File.Exists(Server.MapPath(path)))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
    }
}