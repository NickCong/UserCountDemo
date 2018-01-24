using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserCount.Controllers
{
    public class UserDetail
    {
        public bool result { get; set; }
        public string email { get; set; }
        public string personalurl { get; set; }
        public string phonenumber { get; set; }
        public string picture { get; set; }
        public string newpassword { get; set; }
        public string confirmpassword { get; set; }
        public string password { get; set; }
    }
}