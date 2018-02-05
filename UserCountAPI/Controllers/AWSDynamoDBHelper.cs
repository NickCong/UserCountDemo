using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace UserCountAPI.Controllers
{
    public class AWSDynamoDBHelper
    {
        string TABLEName = ConfigurationManager.AppSettings["TABLEName"];
        string Acceskey = ConfigurationManager.AppSettings["AWSAccessKey"];
        string Secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
        string AuthenticationRegion = ConfigurationManager.AppSettings["AuthenticationRegion"];
        AmazonDynamoDBClient client;
        Table usertable;
        public AWSDynamoDBHelper()
        {
            AmazonDynamoDBConfig ddbConfig = new AmazonDynamoDBConfig();
            ddbConfig.ServiceURL = "https://dynamodb.us-east-2.amazonaws.com";
            ddbConfig.AuthenticationRegion = AuthenticationRegion;
            client = new AmazonDynamoDBClient(Acceskey, Secretkey, ddbConfig);
            usertable = Table.LoadTable(client, TABLEName);
        }
        public void UpdateSourceReference(Document user, string sourceID, string useremail, string status)
        {
            if (sourceID.Equals(ConfigurationManager.AppSettings["DomainSourceID"], StringComparison.OrdinalIgnoreCase))
            {
                List<string> sourceReference = user.ContainsKey("SourceReference") ? user["SourceReference"].AsListOfString() : new List<string>();
                string duplicateReference = user.ContainsKey("DuplicateSourceReference") ? user["DuplicateSourceReference"].AsString() : string.Empty;
                if (!string.IsNullOrEmpty(sourceID) && !sourceReference.Contains(sourceID))
                {
                    sourceReference.Add(sourceID);
                }
                List<BookInfo> books = new List<BookInfo>();
                books = JsonConvert.DeserializeObject<List<BookInfo>>(duplicateReference);
                if (books == null)
                {
                    books = new List<BookInfo>();
                }
                BookInfo newInfo = new BookInfo()
                {
                    PersonalID = ConfigurationManager.AppSettings["DomainSourceID"],
                    BStatus = status,
                    BTime = DateTime.Now.ToString(),
                    Email = ConfigurationManager.AppSettings["DomainSourceID"],
                };
                books.Add(newInfo);
                string json = JsonConvert.SerializeObject(books);
                var sourceRequest = new UpdateItemRequest
                {
                    TableName = TABLEName,
                    Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = useremail } } },
                    ExpressionAttributeNames = new Dictionary<string, string> { { "#reference", "SourceReference" }, { "#duplicateSourceReference", "DuplicateSourceReference" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newReference", new AttributeValue { SS = sourceReference } }, { ":newDuplicateDourceReference", new AttributeValue { S = json } } },
                    UpdateExpression = "SET #reference = :newReference,#duplicateSourceReference = :newDuplicateDourceReference"
                };
                client.UpdateItem(sourceRequest);
            }
            else
            {
                try
                {
                    ScanFilter filter = new ScanFilter();
                    filter.AddCondition("PersonalID", ScanOperator.Equal, new DynamoDBEntry[] { sourceID });
                    ScanOperationConfig config = new ScanOperationConfig
                    {
                        AttributesToGet = new List<string> { "Email" },
                        Filter = filter
                    };
                    Search search = usertable.Scan(filter);
                    List<Document> docList = new List<Document>();
                    do
                    {
                        try
                        {
                            docList = search.GetNextSet();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\n Error: Search.GetNextStep failed because: " + ex.Message);
                            break;
                        }
                        foreach (var doc in docList)
                        {
                            string email = doc["Email"] != null ? doc["Email"].AsString() : string.Empty;
                            List<string> sourceReference = user.ContainsKey("SourceReference") ? user["SourceReference"].AsListOfString() : new List<string>();
                            string duplicateReference = user.ContainsKey("DuplicateSourceReference") ? user["DuplicateSourceReference"].AsString() : string.Empty;
                            if (!string.IsNullOrEmpty(sourceID)&&!sourceReference.Contains(sourceID))
                            {
                                sourceReference.Add(sourceID);
                            }
                            List<BookInfo> books = new List<BookInfo>();
                            books = JsonConvert.DeserializeObject<List<BookInfo>>(duplicateReference);
                            if (books == null)
                            {
                                books = new List<BookInfo>();
                            }
                            BookInfo newInfo = new BookInfo()
                            {
                                PersonalID = sourceID,
                                BStatus = status,
                                BTime = DateTime.Now.ToString(),
                                Email = email,
                            };
                            books.Add(newInfo);
                            string json = JsonConvert.SerializeObject(books);
                            var sourceRequest = new UpdateItemRequest
                            {
                                TableName = TABLEName,
                                Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = useremail } } },
                                ExpressionAttributeNames = new Dictionary<string, string> { { "#reference", "SourceReference" }, { "#duplicateSourceReference", "DuplicateSourceReference" } },
                                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newReference", new AttributeValue { SS = sourceReference } }, { ":newDuplicateDourceReference", new AttributeValue { S = json } } },
                                UpdateExpression = "SET #reference = :newReference,#duplicateSourceReference = :newDuplicateDourceReference"
                            };
                            client.UpdateItem(sourceRequest);
                        }
                    } while (!search.IsDone);
                }
                catch (Exception e)
                {

                }
            }
        }

        public void UpdateDomainBookCount(string status, bool isdomain) {
            string key = string.Empty;
            int value = 0;
            Document domain = GetUser(ConfigurationManager.AppSettings["DomainSourceID"]);
            if (isdomain)
            {
                if (status.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    key = "DomainRefSuccessCount";
                    value = domain.ContainsKey("DomainRefSuccessCount") ? domain["DomainRefSuccessCount"].AsInt() : 0;
                    value++;
                }
                else
                {
                    key = "DomainRefFailCount";
                    value = domain.ContainsKey("DomainRefFailCount") ? domain["DomainRefFailCount"].AsInt() : 0;
                    value++;
                }
            }
            else
            {
                if (status.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    key = "ReferenceSuccessCount";
                    value = domain.ContainsKey("ReferenceSuccessCount") ? domain["ReferenceSuccessCount"].AsInt() : 0;
                    value++;
                }
                else
                {
                    key = "ReferenceFailCount";
                    value = domain.ContainsKey("ReferenceFailCount") ? domain["ReferenceFailCount"].AsInt() : 0;
                    value++;
                }
            }
            var updateRequest = new UpdateItemRequest
            {
                TableName = TABLEName,
                Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = ConfigurationManager.AppSettings["DomainSourceID"] } } },
                ExpressionAttributeNames = new Dictionary<string, string> {
                        { "#temp", key },
                    },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newValue", new AttributeValue { S = value.ToString() } } },
                UpdateExpression = "SET #temp = :newValue"
            };
            client.UpdateItem(updateRequest);
        }
        public ALLReferenceInfo UserReferenceInfo(string email)
        {
            ALLReferenceInfo referenceInfo = new ALLReferenceInfo();
            try
            {       //query        
                Document user = GetUser(ConfigurationManager.AppSettings["DomainSourceID"]);
                referenceInfo.AllUser = user["AllRegister"].AsListOfString().Count;
                referenceInfo.AllDomainRefSuccess = user.ContainsKey("DomainRefSuccessCount") ? user["DomainRefSuccessCount"].AsInt() : 0;
                referenceInfo.AllDomainRefFail = user.ContainsKey("DomainRefFailCount") ? user["DomainRefFailCount"].AsInt() : 0;
                referenceInfo.AllRefSuccess = user.ContainsKey("ReferenceSuccessCount") ? user["ReferenceSuccessCount"].AsInt() : 0;
                referenceInfo.AllRefFail = user.ContainsKey("ReferenceFailCount") ? user["ReferenceFailCount"].AsInt() : 0;
                referenceInfo.NoBookUser = GetAllNoBookUser();
            }
            catch (Exception e)
            {

            }
            return referenceInfo;
        }
        public ReferenceInfo GetUserReference(string email,bool filterAdmin=true)
        {
            ReferenceInfo referenceInfo = new ReferenceInfo();
            referenceInfo.UserReferenceFail = new List<ShowBookInfo>();
            referenceInfo.UserReferenceSuccess = new List<ShowBookInfo>();
            referenceInfo.UserSourceReferenceFail = new List<ShowBookInfo>();
            referenceInfo.UserSourceReferenceSuccess = new List<ShowBookInfo>();
            List<ShowBookInfo> sourcebooks = new List<ShowBookInfo>();
            List<ShowBookInfo> books = new List<ShowBookInfo>();
            try
            {
                Document user = GetUser(email);
                string duplicateSourceReference = user.ContainsKey("DuplicateSourceReference") ? user["DuplicateSourceReference"].AsString() : string.Empty;
                sourcebooks = JsonConvert.DeserializeObject<List<ShowBookInfo>>(duplicateSourceReference);
                if (sourcebooks == null)
                {
                    sourcebooks = new List<ShowBookInfo>();
                }
                foreach (ShowBookInfo doc in sourcebooks)
                {
                    if (filterAdmin && !doc.Email.Equals(ConfigurationManager.AppSettings["DomainSourceID"], StringComparison.OrdinalIgnoreCase))
                    {
                        doc.PersonalUrl = string.Format(ConfigurationManager.AppSettings["DomainURL"], doc.PersonalID);
                        if (doc.BStatus.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            referenceInfo.UserSourceReferenceSuccess.Add(doc);
                        }
                        else
                        {
                            referenceInfo.UserSourceReferenceFail.Add(doc);
                        }
                    }
                }
                string duplicateReference = user.ContainsKey("DuplicateReference") ? user["DuplicateReference"].AsString() : string.Empty;
                books = JsonConvert.DeserializeObject<List<ShowBookInfo>>(duplicateReference);
                if (books == null)
                {
                    books = new List<ShowBookInfo>();
                }
                foreach (ShowBookInfo doc in books)
                {
                    doc.PersonalUrl = string.Format(ConfigurationManager.AppSettings["DomainURL"], doc.PersonalID);
                    if (doc.BStatus.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        referenceInfo.UserReferenceSuccess.Add(doc);
                    }
                    else
                    {
                        referenceInfo.UserReferenceFail.Add(doc);
                    }
                }
            }
            catch (Exception e)
            {

            }
            return referenceInfo;
        }

        public void RegiserUserInfo(string email, string personalID, string sourceID)
        {
            try
            {
                var request = new UpdateItemRequest
                {
                    TableName = TABLEName,
                    Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = email } } },
                    ExpressionAttributeNames = new Dictionary<string, string> { { "#sourceID", "SourceID" }, { "#personalID", "PersonalID" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        { ":newSourceID", new AttributeValue { S = sourceID } },
                        { ":newPersonalID", new AttributeValue { S = personalID } }
                    },
                    UpdateExpression = "SET #sourceID = :newSourceID, #personalID = :newPersonalID"
                };
                client.UpdateItem(request);
            }
            catch (Exception e)
            { }
        }
        public ReferenceInfo GetAllUserReference(bool filterAdmin=true)
        {
            ReferenceInfo referenceInfo = new ReferenceInfo();
            referenceInfo.UserReferenceFail = new List<ShowBookInfo>();
            referenceInfo.UserReferenceSuccess = new List<ShowBookInfo>();
            referenceInfo.UserSourceReferenceFail = new List<ShowBookInfo>();
            referenceInfo.UserSourceReferenceSuccess = new List<ShowBookInfo>();
            List<ShowBookInfo> sourcebooks = new List<ShowBookInfo>();
            List<ShowBookInfo> books = new List<ShowBookInfo>();
            try
            {
                try
                {
                    ScanFilter filter = new ScanFilter();
                    filter.AddCondition("Email", ScanOperator.NotEqual, ConfigurationManager.AppSettings["DomainSourceID"]);
                    ScanOperationConfig config = new ScanOperationConfig
                    {
                        AttributesToGet = new List<string> { "Email", "PersonalID", "DuplicateSourceReference" },
                        Filter = filter
                    };
                    Search search = usertable.Scan(filter);
                    List<Document> docList = new List<Document>();
                    do
                    {
                        try
                        {
                            docList = search.GetNextSet();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\n Error: Search.GetNextStep failed because: " + ex.Message);
                            break;
                        }
                        foreach (var user in docList)
                        {
                            string duplicateSourceReference = user.ContainsKey("DuplicateSourceReference") ? user["DuplicateSourceReference"].AsString() : string.Empty;
                            sourcebooks = JsonConvert.DeserializeObject<List<ShowBookInfo>>(duplicateSourceReference);
                            if (sourcebooks == null)
                            {
                                sourcebooks = new List<ShowBookInfo>();
                            }
                            foreach (ShowBookInfo doc in sourcebooks)
                            {

                                if (filterAdmin && !doc.Email.Equals(ConfigurationManager.AppSettings["DomainSourceID"], StringComparison.OrdinalIgnoreCase))
                                {
                                    doc.ReferenceEmail = user["Email"].AsString();
                                    doc.ReferencePersonalID = user["PersonalID"].AsString();
                                    if (doc.BStatus.Equals("Y", StringComparison.OrdinalIgnoreCase))
                                    {
                                        referenceInfo.UserSourceReferenceSuccess.Add(doc);
                                    }
                                    else
                                    {
                                        doc.ReferenceEmail = user["Email"].AsString();
                                        doc.ReferencePersonalID = user["PersonalID"].AsString();
                                        referenceInfo.UserSourceReferenceFail.Add(doc);
                                    }
                                }
                            }
                            /*string duplicateReference = user.ContainsKey("DuplicateReference") ? user["DuplicateReference"].AsString() : string.Empty;
                            books = JsonConvert.DeserializeObject<List<BookInfo>>(duplicateReference);
                            if (books == null)
                            {
                                books = new List<BookInfo>();
                            }
                            foreach (BookInfo doc in books)
                            {
                                if (doc.BStatus.Equals("Y", StringComparison.OrdinalIgnoreCase))
                                {                                  
                                    referenceInfo.UserReferenceSuccess.Add(doc);
                                }
                                else
                                {
                                    referenceInfo.UserReferenceFail.Add(doc);
                                }
                            }*/
                        }
                    } while (!search.IsDone);
                }
                catch (Exception e)
                {

                }
                
            }
            catch (Exception e)
            {

            }
            return referenceInfo;
        }
        public int GetAllNoBookUser()
        {
            int usercount = 0;
            try
            {
                ScanFilter filter = new ScanFilter();
                filter.AddCondition("SourceReference", ScanOperator.IsNull);
                filter.AddCondition("Email", ScanOperator.NotEqual, ConfigurationManager.AppSettings["DomainSourceID"]);
                ScanOperationConfig config = new ScanOperationConfig
                {
                    AttributesToGet = new List<string> { "Email" },
                    Filter = filter
                };
                Search search = usertable.Scan(filter);
                List<Document> docList = new List<Document>();
                do
                {
                    try
                    {
                        docList = search.GetNextSet();
                        usercount +=docList.Count;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\n Error: Search.GetNextStep failed because: " + ex.Message);
                        break;
                    }
                } while (!search.IsDone);
            }
            catch (Exception e)
            {

            }
            return usercount;
        }
        public void UpdateAllBookUser(string referenceID)
        {
            try
            {
                var domain = usertable.GetItem(ConfigurationManager.AppSettings["DomainSourceID"]);
                List<string> allRegister = domain.ContainsKey("AllRegister") ? domain["AllRegister"].AsListOfString() : new List<string>();
                if (!allRegister.Contains(referenceID))
                {
                    allRegister.Add(referenceID);
                    var updateRequest = new UpdateItemRequest
                    {
                        TableName = TABLEName,
                        Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = ConfigurationManager.AppSettings["DomainSourceID"] } } },
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#sourceReference", "AllRegister" } },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newSourceReference", new AttributeValue { SS = allRegister } } },
                        UpdateExpression = "SET #sourceReference = :newSourceReference"
                    };
                    client.UpdateItem(updateRequest);
                }
            }
            catch (Exception e)
            {

            }
        }
        public void UpdateReference(string currentEmail, string sourceID, string referenceID, string status)
        {
            try
            {
                ScanFilter filter = new ScanFilter();
                filter.AddCondition("PersonalID", ScanOperator.Equal, new DynamoDBEntry[] { sourceID });
                ScanOperationConfig config = new ScanOperationConfig
                {
                    AttributesToGet = new List<string> { "Email", "Reference", "DuplicateReference" },
                    Filter = filter
                };
                Search search = usertable.Scan(filter);
                List<Document> docList = new List<Document>();
                do
                {
                    try
                    {
                        docList = search.GetNextSet();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\n Error: Search.GetNextStep failed because: " + ex.Message);
                        break;
                    }
                    foreach (var doc in docList)
                    {
                        string email = doc["Email"] != null ? doc["Email"].AsString() : string.Empty;
                        List<string> reference = doc.ContainsKey("Reference") ? doc["Reference"].AsListOfString() : new List<string>();
                        string duplicateReference = doc.ContainsKey("DuplicateReference") ? doc["DuplicateReference"].AsString():string.Empty;
                        if (!string.IsNullOrEmpty(email) && !reference.Contains(referenceID))
                        {
                            reference.Add(referenceID);
                        }
                        List<BookInfo> books = new List<BookInfo>();
                        books = JsonConvert.DeserializeObject<List<BookInfo>>(duplicateReference);
                        if (books == null)
                        {
                            books = new List<BookInfo>();
                        }
                        BookInfo newInfo = new BookInfo()
                        {
                            PersonalID = referenceID,
                            BStatus = status,
                            BTime = DateTime.Now.ToString(),
                            Email = currentEmail
                        };
                        books.Add(newInfo);
                        string json = JsonConvert.SerializeObject(books);
                        var sourceRequest = new UpdateItemRequest
                        {
                            TableName = TABLEName,
                            Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = email } } },
                            ExpressionAttributeNames = new Dictionary<string, string> { { "#reference", "Reference" }, { "#duplicateReference", "DuplicateReference" } },
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newReference", new AttributeValue { SS = reference } }, { ":newDuplicateReference", new AttributeValue {  S = json } } },
                            UpdateExpression = "SET #reference = :newReference,#duplicateReference = :newDuplicateReference"
                        };
                        client.UpdateItem(sourceRequest);
                    }
                } while (!search.IsDone);
            }
            catch (Exception e)
            {

            }
        }
        public void UpdateUpdateDomainReference(string currentEmail, string referenceID, string status)
        {
            try
            {
                List<BookInfo> books = new List<BookInfo>();
                var domain = usertable.GetItem(ConfigurationManager.AppSettings["DomainSourceID"]);
                List<string> reference = domain.ContainsKey("Reference") ? domain["Reference"].AsListOfString() : new List<string>();
                string duplicateReference = domain.ContainsKey("DuplicateReference") ? domain["DuplicateReference"].AsString() : string.Empty;
                if (!string.IsNullOrEmpty(referenceID) && !reference.Contains(referenceID))
                {
                    reference.Add(referenceID);
                }
                books = JsonConvert.DeserializeObject<List<BookInfo>>(duplicateReference);
                if (books == null)
                {
                    books = new List<BookInfo>();
                }
                BookInfo newInfo = new BookInfo()
                {
                    PersonalID = referenceID,
                    BStatus = status,
                    BTime = DateTime.Now.ToString(),
                    Email = currentEmail
                };
                books.Add(newInfo);
                string json = JsonConvert.SerializeObject(books);
                // var item = Document.FromJson(json);
                
                 var updateRequest = new UpdateItemRequest
                {
                    TableName = TABLEName,
                    Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = ConfigurationManager.AppSettings["DomainSourceID"] } } },
                    ExpressionAttributeNames = new Dictionary<string, string> {
                        { "#reference", "Reference" },
                        { "#duplicateReference", "DuplicateReference" }
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newReference", new AttributeValue { SS = reference } }, { ":newDuplicateReference", new AttributeValue { S = json} } },
                    UpdateExpression = "SET #reference = :newReference,#duplicateReference = :newDuplicateReference"
                };
                client.UpdateItem(updateRequest);
            }
            catch (Exception e)
            {

            }
        }
        public Document GetUser(string email)
        {
            try
            {
                var user = usertable.GetItem(email);
                if (user != null)
                {
                    return user;
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }       
    }
}