using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace BaumRoll40.Models
{
    /*
     * ただのメモ
     *
     * MyHub.cs内でPushNotification()を呼んでいるので、通知はローカル/サーバー関係なく送信できる
     * 受信はローカルではできないはず？　理由は知らない
     *    
     */
    public class Notificator
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void PushNotification(string userId)
        {
            //WebProxy proxyObject = new WebProxy("http://proxy.nssys.co.jp:8080/", true);
            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
            //request.Proxy = proxyObject;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic OGRiNjAwN2QtMTcyMi00ZDFlLThlZDQtY2JmMThkODI0MmY1");

            var serializer = new JavaScriptSerializer();
            var obj = new
            {
                app_id = "916de59c-4af9-4c5e-b1ac-8f31f2f57d7c",
                headings = new { en = " ", ja = "　"},
                contents = new { en = "English language content is required…", ja = "　" },
                included_segments = new string[] { "All" },
                filters = new object[] { new { field = "tag", key = "customId", relation = "!=", value = userId } }
            };
            var param = serializer.Serialize(obj);
            byte[] byteArray = Encoding.UTF8.GetBytes(param);

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }
                //logger.Info("request送信");

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
                //logger.Info("response受信");

            }
            catch (WebException ex)
            {
                logger.Error("Notificator.csにてエラー発生: " + ex.Message + ex.Status);

                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
        }
    }
}