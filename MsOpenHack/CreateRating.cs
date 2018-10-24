using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.Documents.Client;

namespace Company.Function  // This line has been added to make a new version of the codes so commit can be created and push can be done.
{
    public static class CreateRating
    {
        static HttpClient client = new HttpClient();

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "OpenHacks",
                collectionName: "Ratings",
                ConnectionStringSetting = "cosmosdb_DOCUMENTDB")]
                    IAsyncCollector<UserRating> document,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var userRating = new UserRating(){
                userId = data?.userId,
                productId = data?.productId,
                locationName = data?.locationName,
                rating = data?.rating,
                userNotes = data?.userNotes,
            };

            // 必須チェック
            if (string.IsNullOrEmpty(userRating.userId) || string.IsNullOrEmpty(userRating.productId)){
                return new BadRequestObjectResult("Please pass the userId and the productId in the request body");
            }

            // 存在チェック
            var userResponse = await client.GetAsync($"https://serverlessohlondonuser.azurewebsites.net/api/GetUser?userId={userRating.userId}");
            if (!userResponse.IsSuccessStatusCode){
                return new BadRequestObjectResult("Please pass a valid userId.");
            }
            var productResponse = await client.GetAsync($"https://serverlessohlondonproduct.azurewebsites.net/api/GetProduct?productId={userRating.productId}");
            if (!productResponse.IsSuccessStatusCode){
                return new BadRequestObjectResult("Please pass a valid productId.");
            }

            if (!(userRating.rating >=0 && userRating.rating <=5)){
                return new BadRequestObjectResult("Please pass a rating beetween 0 and 5.");
            }

            // IDとTimestampの記載
            userRating.id = Guid.NewGuid().ToString();
            userRating.timestamp = DateTimeOffset.Now;
            
            await document.AddAsync(userRating);

            return (ActionResult)new OkObjectResult(userRating);
        }
    }

    public class UserRating
    {
        public string id {get; set;}
        public string userId { get; set; }
        public string productId { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }

        public DateTimeOffset timestamp {get;set;}
    }

}
