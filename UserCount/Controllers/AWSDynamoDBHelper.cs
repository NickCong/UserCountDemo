using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace UserCount.Controllers
{
    public class AWSDynamoDBHelper
    {
        string TABLEName = ConfigurationManager.AppSettings["TABLEName"];
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
        private void CreateTable(string keyName)
        {
            // Build a 'CreateTableRequest' for the new table
            CreateTableRequest createRequest = new CreateTableRequest
            {
                TableName = TABLEName,
                AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "Email",
                    AttributeType = "N"
                }
            },
                KeySchema = new List<KeySchemaElement>()
            {
                new KeySchemaElement
                {
                    AttributeName = "Email",
                    KeyType = "HASH"
                }
            },
            };

            // Provisioned-throughput settings are required even though
            // the local test version of DynamoDB ignores them
            createRequest.ProvisionedThroughput = new ProvisionedThroughput(1, 1);

            // Using the DynamoDB client, make a synchronous CreateTable request
            CreateTableResponse createResponse;
            try
            {
                createResponse = client.CreateTable(createRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n Error: failed to create the new table; " + ex.Message);
                return;
            }

            // Report the status of the new table...
            Console.WriteLine("\n\n Created the \"Movies\" table successfully!\n    Status of the new table: '{0}'", createResponse.TableDescription.TableStatus);
        }
        public void CreateUser(string email, string password, string sourceID, string personalID)
        {
            try
            {
                var request = new UpdateItemRequest
                {
                    TableName = TABLEName,
                    Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = email } } },
                    ExpressionAttributeNames = new Dictionary<string, string> { { "#password", "Password" }, { "#sourceID", "SourceID" }, { "#personalID", "PersonalID" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        { ":newPassword", new AttributeValue { S = password } },
                        { ":newSourceID", new AttributeValue { S = sourceID } },
                        { ":newPersonalID", new AttributeValue { S = personalID } }
                    },
                    UpdateExpression = "SET #password = :newPassword, #sourceID = :newSourceID, #personalID = :newPersonalID"
                };
                client.UpdateItem(request);
                if (sourceID.Equals(ConfigurationManager.AppSettings["DomainSourceID"],StringComparison.OrdinalIgnoreCase))
                {
                    UpdateDomainSourceUserReference(personalID);
                }
                else
                {
                    UpdateSourceUserReference(sourceID, personalID);
                }
            }
            catch (Exception e)
            {
                //
            }
        }
        public void UpdateSourceUserReference(string sourceID, string referenceID)
        {
            try
            {
                ScanFilter filter = new ScanFilter();
                filter.AddCondition("PersonalID", ScanOperator.Equal, new DynamoDBEntry[] { sourceID });
                ScanOperationConfig config = new ScanOperationConfig
                {
                    AttributesToGet = new List<string> { "Email", "SourceReference" },
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
                        List<string> sourceReference = doc.ContainsKey("SourceReference") ? doc["SourceReference"].AsListOfString() : new List<string>();
                        if (!string.IsNullOrEmpty(email) && !sourceReference.Contains(referenceID))
                        {
                            sourceReference.Add(referenceID);
                            var sourceRequest = new UpdateItemRequest
                            {
                                TableName = TABLEName,
                                Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = email } } },
                                ExpressionAttributeNames = new Dictionary<string, string> { { "#sourceReference", "SourceReference" } },
                                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newSourceReference", new AttributeValue { SS = sourceReference } } },
                                UpdateExpression = "SET #sourceReference = :newSourceReference"
                            };
                            client.UpdateItem(sourceRequest);
                        }
                    }
                } while (!search.IsDone);
            }
            catch (Exception e)
            {

            }
        }
        public void UpdateDomainSourceUserReference(string referenceID)
        {
            try
            {
                var domain = usertable.GetItem(ConfigurationManager.AppSettings["DomainSourceID"]);
                List<string> sourceReference = domain.ContainsKey("SourceReference") ? domain["SourceReference"].AsListOfString() : new List<string>();
                if (!sourceReference.Contains(referenceID))
                {
                    sourceReference.Add(referenceID);
                    var updateRequest = new UpdateItemRequest
                    {
                        TableName = TABLEName,
                        Key = new Dictionary<string, AttributeValue> { { "Email", new AttributeValue { S = ConfigurationManager.AppSettings["DomainSourceID"] } } },
                        ExpressionAttributeNames = new Dictionary<string, string> { { "#sourceReference", "SourceReference" } },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":newSourceReference", new AttributeValue { SS = sourceReference } } },
                        UpdateExpression = "SET #sourceReference = :newSourceReference"
                    };
                    client.UpdateItem(updateRequest);
                }
            }
            catch (Exception e)
            {

            }
        }
        public bool CheckUser(string email)
        {
            bool IsExist = false;
            try
            {              
                var user = usertable.GetItem(email);
                if (user != null)
                {
                    IsExist = true;
                }
                return IsExist;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public UserDetail CheckUser(string email,string password)
        {
            UserDetail detail = new UserDetail();
            detail.result = false;
            try
            {
                var user = usertable.GetItem(email);
                if (user != null)
                {
                    if (user["Password"].AsString().Equals(password))
                    {
                        detail.result = true;
                        detail.email = email;
                        detail.personalurl = user.ContainsKey("PersonalID") ? ConfigurationManager.AppSettings["DomainURL"] + "/" + user["PersonalID"].AsString()+"/home" : string.Empty;
                        detail.phonenumber = user.ContainsKey("PhoneNumber") ? user["PhoneNumber"].AsString() : string.Empty;
                        detail.picture = user.ContainsKey("Picture") ? user["Picture"].AsString() : string.Empty;
                    }                    
                }
                return detail;
            }
            catch (Exception e)
            {
                return detail;
            }
        }
        public bool CheckUserExist(string email, string password)
        {
            UserDetail detail = new UserDetail();
            detail.result = false;
            try
            {
                var user = usertable.GetItem(email);
                if (user != null)
                {
                    if (user["Password"].AsString().Equals(password))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}