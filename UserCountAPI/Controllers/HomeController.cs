using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UserCountAPI.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public JsonResult GetReferenceAllInfo(string useremail)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            ALLReferenceInfo info = dbHelper.UserReferenceInfo(useremail);          
            return Json(info, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void RegiserUserInfo(string email, string personalID, string sourceID)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            dbHelper.RegiserUserInfo(email, personalID, sourceID);
        }

        [HttpGet]
        public JsonResult GetUserReference(string useremail, bool filterAdmin = true)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            ReferenceInfo info = dbHelper.GetUserReference(useremail, filterAdmin);
            return Json(info, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllUserReference(bool filterAdmin = true)
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            ReferenceInfo info = dbHelper.GetAllUserReference(filterAdmin);
            return Json(info, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public void Book(string sourceID, string useremail, string status)
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
                        dbHelper.UpdateDomainBookCount(status, true);
                        dbHelper.UpdateUpdateDomainReference(useremail, user["PersonalID"].AsString(), status);
                    }
                    else
                    {
                        dbHelper.UpdateDomainBookCount(status, false);
                        dbHelper.UpdateReference(useremail, sourceID, user["PersonalID"].AsString(), status);                       
                    }
                    dbHelper.UpdateSourceReference(user, sourceID, useremail, status);                  
                }
            }
        }

    }
}
