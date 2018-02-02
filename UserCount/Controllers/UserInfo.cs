using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserCount.Controllers
{
    public class UserInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string SourceID { get; set; }
        public string PersonalID { get; set; }
        public string UniqueBook { get; set; }
        public string PhoneNumber { get; set; }
        public string Picture { get; set; }
        public List<string> Reference { get; set; }

        public List<BookInfo> DuplicateReference { get; set; }

        public List<string> SourceReference { get; set; }
        public List<BookInfo> DuplicateSourceReference { get; set; }
        public List<string> SourceRegister { get; set; }
    }

    public class BookInfo
    {
        public string email { get; set; }
        public string BStatus { get; set; }
        public string BTime { get; set; }
        public string PersonalID { get; set; }
    }

}