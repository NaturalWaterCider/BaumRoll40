using BaumRoll40.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaumRoll40.Controllers
{
    [Authorize]
    public class InfoController : Controller
    {
        private BaumRollEntities db = new BaumRollEntities();

        // GET: Info
        public ActionResult Index()
        {
            var userid = int.Parse(User.Identity.Name);
            ViewBag.UserName = db.Users.Where(u => u.UserId == userid).Select(u => u.UserName).FirstOrDefault();
            return View();
        }
    }
}