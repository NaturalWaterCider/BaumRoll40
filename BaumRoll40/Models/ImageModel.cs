using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace BaumRoll40.Models
{
    public class ImageModel
    {
        private BaumRollEntities db = new BaumRollEntities();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public int Id;

        public ImageModel()
        {
        }

        public ImageModel(int id)
        {
            Id = id;
        }

        public byte[] Show()
        {
            //var dt = new DataTable();
            //var conn = new SqlConnection(ConnectionString);
            //var cmd = new SqlCommand("select [Images].[Bin] from [Images] where [Images].[id]=@Id;", conn);
            //cmd.Parameters.AddWithValue("@Id", Id);
            //var adp = new SqlDataAdapter(cmd);
            //adp.Fill(dt);
            //return dt.Rows[0][0] as byte[];

            //EntityFrameworkすげぇなと思う瞬間である
            return db.Picture.Find(Id).Picture1;


        }

        public int Create()
        {
            //var conn = new SqlConnection(ConnectionString);
            //var cmd = new SqlCommand("insert into [Picture]([Picture].[Picture]) values(@Bin); select @@identity;", conn);
            //cmd.Parameters.AddWithValue("@Bin", File("lumonde"));
            //conn.Open();
            //long.TryParse(cmd.ExecuteScalar().ToString(), out Id);
            //conn.Close();
            //return Id;
            if (!HttpContext.Current.Request.ContentType.Contains("image/") && HttpContext.Current.Request.ContentType != "")
            {
                logger.Error("ImageModel Create 画像登録失敗 image以外の指定 by" + HttpContext.Current.User.Identity.Name);
                return -1;
            }

            try
            {
                var pic = new Picture
                {
                    Picture1 = File("lumonde")
                };

                if(pic.Picture1.Length == 0)
                {
                    return -1;
                }

                db.Picture.Add(pic);
                db.SaveChanges();

                return pic.PictureId;
            }
            catch (Exception ex)
            {
                logger.Error("ImageModel Create 画像登録失敗： " + ex);
                return -1;
            }
        }

        public static byte[] File(string key)
        {

            var str = HttpContext.Current.Request.InputStream;
            var strLen = Convert.ToInt32(str.Length);
            byte[] strArr = new byte[strLen];

            // Read stream into byte array.
            str.Read(strArr, 0, strLen);

            str.Dispose();

            return strArr;
        }
    }
}