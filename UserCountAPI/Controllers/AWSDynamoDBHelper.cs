using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace UserCountAPI.Controllers
{
    public class AWSDynamoDBHelper
    {
        string TABLEName = "Demo";
        string Acceskey = ConfigurationManager.AppSettings["AWSAccessKey"];
        string Secretkey = ConfigurationManager.AppSettings["AWSSecretKey"];
        AmazonDynamoDBClient client;
        Table usertable;
        public AWSDynamoDBHelper()
        {
            AmazonDynamoDBConfig ddbConfig = new AmazonDynamoDBConfig();
            ddbConfig.ServiceURL = "https://dynamodb.us-east-2.amazonaws.com";
            ddbConfig.AuthenticationRegion = "us-east-2";
            client = new AmazonDynamoDBClient(Acceskey, Secretkey, ddbConfig);
            usertable = Table.LoadTable(client, TABLEName);
        }
        public void UpdateReference(string sourceID, string referenceID)
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
                        List<string> reference = doc["Reference"] != null ? doc["Reference"].AsListOfString() : new List<string>();
                        List<string> duplicateReference = doc["DuplicateReference"] != null ? doc["DuplicateReference"].AsListOfString() : new List<string>();
                        if (!string.IsNullOrEmpty(email) && !reference.Contains(referenceID))
                        {
                            reference.Add(referenceID);                           
                        }
                        duplicateReference.Add(referenceID);
                        var sourceRequest = new UpdateItemRequest
                        {
                            TableName = TABLEName,
                            Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = email } } },
                            ExpressionAttributeNames = new Dictionary<string, string> { { "#reference", "Reference" }, { "#duplicateReference", "DuplicateReference" } },
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newReference", new AttributeValue { NS = reference } }, { ":newDuplicateReference", new AttributeValue { NS = duplicateReference } } },
                            UpdateExpression = "SET #reference = :newReference;#duplicateReference = :newDuplicateReference;"
                        };
                        client.UpdateItem(sourceRequest);
                    }
                } while (!search.IsDone);
            }
            catch (Exception e)
            {

            }
        }
        public void UpdateUpdateDomainReference(string referenceID)
        {
            try
            {
                var domain = usertable.GetItem("Domain");
                List<string> reference = domain["Reference"] != null ? domain["Reference"].AsListOfString() : new List<string>();
                List<string> duplicateReference = domain["DuplicateReference"] != null ? domain["DuplicateReference"].AsListOfString() : new List<string>();
                if (!string.IsNullOrEmpty(referenceID) && !reference.Contains(referenceID))
                {
                    reference.Add(referenceID);                   
                }
                duplicateReference.Add(referenceID);
                var updateRequest = new UpdateItemRequest
                {
                    TableName = TABLEName,
                    Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = "Domain" } } },
                    ExpressionAttributeNames = new Dictionary<string, string> { { "#reference", "Reference" }, { "#duplicateReference", "DuplicateReference" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newReference", new AttributeValue { NS = reference } },{ ":newDuplicateReference", new AttributeValue { NS = duplicateReference } } },
                    UpdateExpression = "SET #reference = :newReference;#duplicateReference = :newDuplicateReference;"
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