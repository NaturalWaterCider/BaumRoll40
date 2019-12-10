using BaumRoll40.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaumRoll40.Controllers
{
    public class ApplePieController : ApiController
    {
        private BaumRollEntities db = new BaumRollEntities();

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public string Post([FromBody]string date)
        {
            var res = "";
            try
            {
                DateTime dateTime = (!string.IsNullOrEmpty(date)) ? DateTime.Parse(date) : DateTime.Now;

                db.Post.Where(p => p.PostTime.CompareTo(dateTime) >= 0).ToList().ForEach(p => {
                    if (string.IsNullOrEmpty(res))
                    {
                        res += p.Content;
                    }
                    else
                    {
                        res += ", " + p.Content;
                    }
                });

            }
            catch (Exception e)
            {
                res = "ばーか";
                return res;
            }
            return res;

        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}