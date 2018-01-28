using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace UserCountAPI.Controllers
{
    public class CountUserInfoController : ApiController
    {
        // GET api/values
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}
        public Dictionary<string, string> Get()
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            Document user = dbHelper.GetUser(ConfigurationManager.AppSettings["DomainSourceID"]);
            Dictionary<string, string> report = new Dictionary<string, string>();
            if (user != null)
            {
                if (user.ContainsKey("Reference"))
                {
                    report.Add("NoRerence", user["Reference"].AsListOfString().Count.ToString());                  
                }
                else
                {
                    report.Add("NoRerence", "0");
                }
                if (user.ContainsKey("OtherReference"))
                {
                    report.Add("Reference", user["OtherReference"].AsListOfString().Count.ToString());
                }
                else
                {
                    report.Add("Reference", "0");
                }
            }
            else
            {
                report.Add("Reference", "0");
                report.Add("NoRerence", "0");
            }
            return report;
        }

        // GET api/values/5
        public void Get(string sourceID, string useremail)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            string baseurl = ConfigurationManager.AppSettings["DomainURL"];
            //string sourceID = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : ConfigurationManager.AppSettings["DomainSourceID"];
            Document user = dbHelper.GetUser(useremail);
            if (user != null)
            {
                if (!user["PersonalID"].AsString().Equals(sourceID))
                {
                    if (string.IsNullOrEmpty(sourceID) || sourceID.Equals(ConfigurationManager.AppSettings["DomainSourceID"], StringComparison.OrdinalIgnoreCase))
                    {
                        dbHelper.UpdateUpdateDomainReference(user["PersonalID"].AsString());
                    }
                    else
                    {
                        dbHelper.UpdateReference(sourceID, user["PersonalID"].AsString());
                    }
                }
            }
        }

        // POST api/       
        public void Post(string sourceID, string useremail)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            string baseurl = ConfigurationManager.AppSettings["DomainURL"];
            //string sourceID = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : ConfigurationManager.AppSettings["DomainSourceID"];
            Document user = dbHelper.GetUser(useremail);
            if (user != null)
            {
                if (!user["PersonalID"].AsString().Equals(sourceID))
                {
                    if (string.IsNullOrEmpty(sourceID) || sourceID.Equals(ConfigurationManager.AppSettings["DomainSourceID"]))
                    {
                        dbHelper.UpdateUpdateDomainReference(user["PersonalID"].AsString());
                    }
                    else
                    {
                        dbHelper.UpdateReference(sourceID, user["PersonalID"].AsString());
                    }
                }
            }
        }
        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

    }
}