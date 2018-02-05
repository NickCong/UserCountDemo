using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace UserCount.Controllers
{

    public class DemoController : Controller
    {
        public ActionResult Home()
        {
            GetInitInfo();
            return View();
        }

        private void GetInitInfo()
        {
            AWSDynamoDBHelper helper = new AWSDynamoDBHelper();
            string email = Session["email"] == null ? string.Empty : Session["email"].ToString();
            string enpassword = Session["password"] == null ? string.Empty : Session["password"].ToString();
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.IsAuthentication = false;
            }
            else
            {
                UserDetail result = helper.CheckUser(email, enpassword);
                ViewBag.IsAuthentication = result.result;
                if (ViewBag.IsAuthentication)
                {
                    ViewBag.email = result.email;
                    ViewBag.password = result.password;
                    ViewBag.personalurl = result.personalurl;
                    ViewBag.phonenumber = result.phonenumber;
                }
            }
        }

        [HttpPost]
        public JsonResult Login(string email, string password)
        {
            AWSDynamoDBHelper helper = new AWSDynamoDBHelper();
            string enpassword = Encrypt(password);
            UserDetail result = helper.CheckUser(email, enpassword);
            if (result.result)
            {
                Session["email"] = email;
                Session["password"] = enpassword;
                Session.Timeout = 30;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetUser(string email, string password)
        {
            AWSDynamoDBHelper helper = new AWSDynamoDBHelper();
            UserDetail result = helper.CheckUser(email, password);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Register(string email, string password)
        {
            AWSDynamoDBHelper helper = new AWSDynamoDBHelper();
            UserDetail detail = new UserDetail();
            string enpassword = Encrypt(password);
            if (helper.CheckUserExist(email, enpassword))
            {
                detail.result = false;
                return Json(detail, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string personalID = Guid.NewGuid().ToString();
                string baseurl = ConfigurationManager.AppSettings["DomainURL"];
                string sourceID = RouteData.Values["id"] != null ? RouteData.Values["id"].ToString() : ConfigurationManager.AppSettings["DomainSourceID"];
                detail.result = true;
                detail.email = email;
                detail.password = enpassword;
                detail.personalurl = baseurl + "/" + personalID + "/Home";
                helper.CreateUser(email, enpassword, sourceID, personalID);
                PostTOAPIInfo(email, sourceID, personalID);
                if (detail.result)
                {
                    Session["email"] = email;
                    Session["password"] = enpassword;
                    Session.Timeout = 30;
                }
            }
            return Json(detail, JsonRequestBehavior.AllowGet);
        }

        private void PostTOAPIInfo(string email, string sourceID, string personalID)
        {
            using (var wb = new WebClient())
            {
                var data = new NameValueCollection();
                data["email"] = email;
                data["sourceID"] = sourceID;
                data["personalID"] = personalID;
                var response = wb.UploadValues(ConfigurationManager.AppSettings["APIURL"] + "/RegiserUserInfo", "POST", data);
                string responseInString = Encoding.UTF8.GetString(response);
            }
   //         HttpClient client = new HttpClient();
   //         var values = new Dictionary<string, string>
   //         { {"email", email},
   //{ "sourceID", sourceID },
   //{ "personalID", personalID }
   //         };

   //         var content = new FormUrlEncodedContent(values);

   //         var response =  client.PostAsync(ConfigurationManager.AppSettings["APIURL"]+ "/RegiserUserInfo", content);

   //         var responseString =  response.Result.Content.ReadAsStringAsync();
        }

        [HttpGet]
        public JsonResult GetAllUserEmail()
        {
            AWSDynamoDBHelper dbHelper = new AWSDynamoDBHelper();
            List<string> emails = dbHelper.GetAllUserEmail();
            return Json(emails, JsonRequestBehavior.AllowGet);
        }

        /// <summary>  
        /// C# DES解密方法  
        /// </summary>  
        /// <param name="encryptedValue">待解密的字符串</param>  
        /// <returns>解密后的字符串</returns>  
        private string Decrypt(string encryptedValue)
        {
            var PrivateKey = ConfigurationManager.AppSettings["PrivateKey"];
            var iv = ConfigurationManager.AppSettings["PrivateIV"];
            using (DESCryptoServiceProvider sa =
                new DESCryptoServiceProvider
                { Key = Encoding.UTF8.GetBytes(PrivateKey), IV = Encoding.UTF8.GetBytes(iv) })
            {
                using (ICryptoTransform ct = sa.CreateDecryptor())
                {
                    byte[] byt = Convert.FromBase64String(encryptedValue);

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
                        {
                            cs.Write(byt, 0, byt.Length);
                            cs.FlushFinalBlock();
                        }
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        /// <summary>  
        /// C# DES加密方法  
        /// </summary>  
        /// <param name="encryptedValue">要加密的字符串</param>  
        /// <returns>加密后的字符串</returns>  
        private string Encrypt(string originalValue)
        {
            var PrivateKey = ConfigurationManager.AppSettings["PrivateKey"];
            var iv = ConfigurationManager.AppSettings["PrivateIV"];
            using (DESCryptoServiceProvider sa
                = new DESCryptoServiceProvider { Key = Encoding.UTF8.GetBytes(PrivateKey), IV = Encoding.UTF8.GetBytes(iv) })
            {
                using (ICryptoTransform ct = sa.CreateEncryptor())
                {
                    byte[] by = Encoding.UTF8.GetBytes(originalValue);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, ct,
                                                         CryptoStreamMode.Write))
                        {
                            cs.Write(by, 0, by.Length);
                            cs.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public ActionResult About()
        {
            GetInitInfo();
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Report()
        {
            GetInitInfo();
            if (ViewBag.IsAuthentication)
            {
                if (ViewBag.email == ConfigurationManager.AppSettings["AdminUser"])
                {
                    ViewBag.role = "admin";
                }
            }
            return View();
        }
    }
}