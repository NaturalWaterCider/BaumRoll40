using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BaumRoll40.Models;

namespace BaumRoll40.Controllers
{
    public class BlogsController : Controller
    {
        private BaumRollEntities db = new BaumRollEntities();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // GET: Blogs
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(db.Blog.ToList());
        }

        // GET: Blogs/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blog.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // GET: Blogs/Create
        [Authorize]
        public ActionResult Create()
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            return View();
        }

        // POST: Blogs/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BlogId,Title,Content")] Blog blog)
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            if (ModelState.IsValid)
            {
                blog.CreateDate = DateTime.Now;
                db.Blog.Add(blog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blog);
        }

        // GET: Blogs/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blog.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BlogId,Title,Content")] Blog blog)
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }


            var blog1 = db.Blog.Find(blog.BlogId);
            blog1.Title = blog.Title;
            blog1.Content = blog.Content;
            blog1.Tag.Add(db.Tag.Find(1));

            if (ModelState.IsValid)
            {
                db.Entry(blog1).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blog);
        }

        // GET: Blogs/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Blog blog = db.Blog.Find(id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        // POST: Blogs/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (User.Identity.Name != "4280")
            {
                logger.Error("わるいこ GET：" + User.Identity.Name);
                return RedirectToAction("Index", "Home", new { id = 1 });
            }

            Blog blog = db.Blog.Find(id);
            db.Blog.Remove(blog);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
