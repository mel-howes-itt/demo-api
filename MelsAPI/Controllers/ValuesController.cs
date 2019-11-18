using MelsAPI.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace MelsAPI.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<Product> Get()
        {
            // read from Azure SQL Database
            string connectionString = 
                ConfigurationManager.ConnectionStrings["SourceSQLConnection"].ConnectionString;
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = 
                    new SqlCommand("SELECT TOP 20 [ProductID] ,[ProductNumber] FROM [SalesLT].[Product]"
                    , connection);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        ProductID = int.Parse(reader["ProductID"].ToString()),
                        ProductNumber = reader["ProductNumber"].ToString()
                    });
                }
            }

            // upload the json to blob store

            string storageConnectionString = 
                ConfigurationManager.AppSettings["DestinationStorageConnection"].ToString();

            CloudStorageAccount storageAccount;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // The "products" container should pre-exist
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("products");

                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference("products.txt");

                string json = JsonConvert.SerializeObject(products);

                cloudBlockBlob.UploadText(json);
            }            

            return products;
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
            //string connectionString = "Server=tcp:validationsrv01.database.windows.net,1433;Initial Catalog=validationdb01;Persist Security Info=False;User ID=rezaadmin;Password=Kluwer2006!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
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