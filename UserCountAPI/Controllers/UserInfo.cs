using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UserCountAPI.Controllers
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

    public interface BookInfoInterface
    {
        string BStatus { get; set; }
        string BTime { get; set; }
        string Email { get; set; }
        string PersonalID { get; set; }
    }
    public class BookInfo: BookInfoInterface
    {
        public string BStatus { get; set; }
        public string BTime { get; set; }
        public string Email { get; set; }
        public string PersonalID { get; set; }
    }
    public class UpdateBookInfo: BookInfoInterface
    {
        public string BStatus { get; set; }
        public string BTime { get; set; }
        public string Email { get; set; }
        public string PersonalID { get; set; }

        public string TTime { get; set; }
        public string TStatus { get; set; }
    }

    public class ShowBookInfo : BookInfoInterface
    {
        public string BStatus { get; set; }
        public string BTime { get; set; }
        public string Email { get; set; }
        public string PersonalID { get; set; }

        public string ReferenceEmail { get; set; }
        public string ReferencePersonalID { get; set; }
    }

    public class ALLReferenceInfo
    {
        public int AllUser { get; set; }
        public int NoBookUser { get; set; }
        public int AllRefSuccess { get; set; }
        public int AllRefFail { get; set; }
        public int AllDomainRefSuccess { get; set; }
        public int AllDomainRefFail { get; set; }

        public ReferenceInfo ReferenceInfo { get; set; }
    }
    public class ReferenceInfo
    {
        public List<ShowBookInfo> UserReferenceFail { get; set; }
        public List<ShowBookInfo> UserSourceReferenceFail { get; set; }
        public List<ShowBookInfo> UserReferenceSuccess { get; set; }
        public List<ShowBookInfo> UserSourceReferenceSuccess { get; set; }
    }
}