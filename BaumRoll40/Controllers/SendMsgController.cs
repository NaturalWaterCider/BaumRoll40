using BaumRoll40.Models;
using BaumRoll40.SignalR;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;

namespace BaumRoll40.Controllers
{
    public class SendMsgController : ApiController
    {
        readonly CustomMembershipProvider membershipProvider = new CustomMembershipProvider();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private BaumRollEntities db = new BaumRollEntities();

        // GET: api/SendMsg
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SendMsg/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/SendMsg
        /// <summary>
        /// 
        /// </summary>
        /// <returns>応答メッセージ</returns>
        /// <param name="value">id, password, message</param>
        public string Post([FromBody]JToken value)
        {
            //入力チェックの嵐
            if(value == null)
            {
                return "ばーか";
            }

            var strUserId = value["id"]?.ToString();
            int userId = 0;
            if(string.IsNullOrEmpty(strUserId) || !int.TryParse(strUserId, out userId))
            {
                return "ばーか";
            }

            var pass = value["password"] != null ? value["password"].ToString(): "";
            if(!Regex.IsMatch(pass, "[0-9a-zA-Z]{0,20}"))
            {
                return "ばーか";
            }

            if (value["message"] == null || string.IsNullOrEmpty(value["message"].ToString()))
            {
                return "ばーか";
            }
            var message = value["message"].ToString();

            //認証する
            if (!this.membershipProvider.ValidateUser(userId.ToString(), pass))
            {
                return "ばーか";
            }

            var username = db.Users.Where(u => u.UserId == userId).Select(u => u.UserName).FirstOrDefault();

            //DB登録＆Viewへ反映
            var managePostModel = new ManagePostModel();
            managePostModel.ReceiveMessage(userId, username, message, null);

            return "わーい！";
        }

        // PUT: api/SendMsg/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SendMsg/5
        public void Delete(int id)
        {
        }
    }
}
