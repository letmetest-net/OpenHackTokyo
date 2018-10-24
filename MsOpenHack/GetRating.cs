using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;

namespace Company.Function
{
    public static class GetRating
    {
        [FunctionName("GetRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetRating/id/{ratingId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "OpenHacks",
                collectionName: "Ratings",
                SqlQuery="select * from c where c.id={ratingId}", 
                ConnectionStringSetting = "cosmosdb_DOCUMENTDB")
                ]
                   IEnumerable<UserRating> userRatings,
            ILogger log)
        {
            if (userRatings.Count() == 0){
                return new BadRequestObjectResult("Please pass a valid ratingId.");
            }
            return (ActionResult)new OkObjectResult(userRatings.First());
        }
    }
}
