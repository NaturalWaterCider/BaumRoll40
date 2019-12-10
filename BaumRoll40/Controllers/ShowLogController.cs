using BaumRoll40.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BaumRoll40.Controllers
{
    [Authorize]
    public class ShowLogController : Controller
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // GET: showLog
        public ActionResult Index()
        {
            ShowLogViewModel viewModel = new ShowLogViewModel();
            viewModel.StrLogDate = DateTime.Now.ToString("yyyy-MM-dd");
            viewModel.LogContent = GetLogContent(viewModel.StrLogDate);

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult index([Bind(Include = "StrLogDate")]ShowLogViewModel viewModel)
        {
            viewModel.LogContent = GetLogContent(viewModel.StrLogDate);

            return View(viewModel);
        }

        private string GetLogContent(string date)
        {
            DateTime validationDate = new DateTime();
            if(string.IsNullOrEmpty(date) || date.Length != 10 || !DateTime.TryParse(date, out validationDate))
            {
                logger.Error("ログ　指定日がおかしいですよ by" + User.Identity.Name + " 指定日:" + date);
                return "指定日がおかしいですよ";
            }

            string filePath = @"C:\inetpub\wwwroot\logs\";
            if(date.Substring(5, 2) != DateTime.Now.Month.ToString("00"))
            {
                filePath += date.Substring(0, 7) + @"\";
            }
            filePath += date + ".log";

            if (System.IO.File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("Shift_JIS"));

                string str = sr.ReadToEnd();

                sr.Close();

                return str;
            }
            else
            {
                return "ファイルがないらしいよ";
            }
        }
    }
}