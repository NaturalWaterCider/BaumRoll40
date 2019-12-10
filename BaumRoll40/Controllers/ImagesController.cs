using BaumRoll40.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaumRoll40.Controllers
{
    [Authorize]
    public class ImagesController : Controller
    {
        private BaumRollEntities db = new BaumRollEntities();

        [HttpGet]
        public ActionResult Show(int id)
        {
            var image = new ImageModel(id).Show();

            return new FileContentResult(image, "image/jpeg");
        }

        [HttpPost]
        public string Create()
        {
            var id = new ImageModel().Create();
            return id.ToString();
        }
    }
}