using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using TimesAzureFunctions.common.Model;
using TimesAzureFunctions.common.Responses;
using TimesAzureFunctions.Function.Entities;


namespace TimesAzureFunctions.Function.Functions
{
    public static class TimesApi
    {
        [FunctionName(nameof(RegisterEmployed))]
        public static async Task<IActionResult> RegisterEmployed(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "times")] HttpRequest req,
            [Table("times", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Times tim = JsonConvert.DeserializeObject<Times>(requestBody);

            if (string.IsNullOrEmpty(tim?.Id.ToString()) || string.IsNullOrEmpty(tim?.type.ToString())
                || (!int.Equals(tim.type, 0) && !int.Equals(tim.type, 1)))
            {
                log.LogInformation("A bad request was returned");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a type 0 or 1 and an id."
                });
            }

            TimeEntity employeeMonitoringEntity = new TimeEntity
            {
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*",
                PartitionKey = "EMPLOYEERECORD",
                Id = tim.Id,
                type = tim.type,
                dateCreate = Convert.ToDateTime(tim.dateCreate, new CultureInfo("en-US")),
                consolidate = false
            };

            TableOperation addOperation = TableOperation.Insert(employeeMonitoringEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = "New employee time monitoring record has been stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeMonitoringEntity
            });
        }

        [FunctionName(nameof(updateEmployed))]
        public static async Task<IActionResult> updateEmployed(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "times/{id}")] HttpRequest req,
            [Table("times", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for register: {id}, recived");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Times time = JsonConvert.DeserializeObject<Times>(requestBody);

            //Validate register id
            TableOperation findOperation = TableOperation.Retrieve<TimeEntity>("TIME", id);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found"
                });
            }

            //Update register
            TimeEntity timeEntity = (TimeEntity)findResult.Result;
            if (!string.IsNullOrEmpty(time.Id.ToString()))
            {
                timeEntity.Id = time.Id;
            }
            if (!string.IsNullOrEmpty(time.dateCreate.ToString()))
            {
                timeEntity.dateCreate = time.dateCreate;
            }
            if (!string.IsNullOrEmpty(time.type.ToString()))
            {
                timeEntity.type = time.type;
            }

            TableOperation addOperation = TableOperation.Replace(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = $"Register: {id}, update in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
        [FunctionName(nameof(GetAllRegister))]
        public static async Task<IActionResult> GetAllRegister(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "times")] HttpRequest req,
           [Table("times", Connection = "AzureWebJobsStorage")] CloudTable times,
           ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> ti = await times.ExecuteQuerySegmentedAsync(query, null);


            string message = "Retrieved all register.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = ti
            });
        }

        [FunctionName(nameof(GetregisterById))]
        public static IActionResult GetregisterById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "times/{id}")] HttpRequest req,
            [Table("times", "TIME", "{id}", Connection = "AzureWebJobsStorage")] TimeEntity timeEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get register by Id: {id} received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found"
                });
            }

            string message = $"Register: {timeEntity.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
        [FunctionName(nameof(DeleteRegister))]
        public static async Task<IActionResult> DeleteRegister(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "times/{id}")] HttpRequest req,
            [Table("times", "TIME", "{id}", Connection = "AzureWebJobsStorage")] TimeEntity timeEntity,
            [Table("times", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete todo: {id}, received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Todo not found"
                });
            }

            await timeTable.ExecuteAsync(TableOperation.Delete(timeEntity));
            string message = $"Todo: {timeEntity.RowKey}, Delete.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
    }
}