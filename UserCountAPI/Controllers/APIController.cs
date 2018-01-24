using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UserCountAPI.Controllers
{
    public class APIController : Controller
    {
        // GET: API
        public void CountUserInfo(string sourceurl, string useremail)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            string sourceID = string.Empty;
            string baseurl = ConfigurationManager.AppSettings["DomainURL"];
            if (sourceurl.Length > baseurl.Length)
            {
                if (sourceurl.Substring(ConfigurationManager.AppSettings["DomainURL"].Length).IndexOf("/") > 0)
                {
                    string temp = sourceurl.Substring(ConfigurationManager.AppSettings["DomainURL"].Length);
                    sourceID = temp.Substring(temp.IndexOf("/"));
                }
                else
                {
                    sourceID = sourceurl.Substring(ConfigurationManager.AppSettings["DomainURL"].Length);
                }
            }
            Document user = dbHelper.GetUser(useremail);
            if (user != null)
            {
                if (!user["PersonalID"].AsString().Equals(sourceID))
                {
                    if (string.IsNullOrEmpty(sourceID))
                    {
                        dbHelper.UpdateUpdateDomainReference(user["PersonalID"].AsString());
                    }
                    else {
                        dbHelper.UpdateReference(sourceID, user["PersonalID"].AsString());
                    }
                }
            }
        }
    }
}