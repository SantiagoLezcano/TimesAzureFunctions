using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
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

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Times time = JsonConvert.DeserializeObject<Times>(requestBody);
            if (string.IsNullOrEmpty(time?.Id.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a Id"
                });
            }
            string filter = TableQuery.GenerateFilterConditionForInt("Id", QueryComparisons.Equal, time.Id);
            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>().Where(filter);
            TableQuerySegment<TimeEntity> registers = await timeTable.ExecuteQuerySegmentedAsync(query, null);
            int inEmployed = 0;
            int outEmployed = 0;
            foreach (TimeEntity register in registers)
            {
                if (register.type == 0)
                {
                    inEmployed++;
                }
                if (register.type == 1)
                {
                    outEmployed++;
                }
            }
            TimeEntity timeEntity = null;
            if (inEmployed != outEmployed)
            {
                if (time.type != 1)
                {
                    return new BadRequestObjectResult(new Response
                    {
                        IsSuccess = false,
                        Message = "The employee has not marked a output"
                    });
                }
                else
                {
                    timeEntity = new TimeEntity
                    {
                        Id = time.Id,
                        dateCreate = time.dateCreate,
                        type = time.type,
                        consolidate = false,
                        ETag = "*",
                        PartitionKey = "TIME",
                        RowKey = Guid.NewGuid().ToString()
                    };
                }
            }
            else
            {
                if (time.type != 0)
                {
                    return new BadRequestObjectResult(new Response
                    {
                        IsSuccess = false,
                        Message = "The employee has not marked a input"
                    });
                }
                else
                {
                    timeEntity = new TimeEntity
                    {
                        Id = time.Id,
                        dateCreate = time.dateCreate,
                        type = time.type,
                        consolidate = false,
                        ETag = "*",
                        PartitionKey = "TIME",
                        RowKey = Guid.NewGuid().ToString()
                    };
                }
            }

            TableOperation addOperation = TableOperation.Insert(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = "New register stored in a table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
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
           [Table("times", Connection = "AzureWebJobsStorage")] CloudTable timeEntity,
           ILogger log)
        {
            log.LogInformation("Get all todos received.");

            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> timis = await timeEntity.ExecuteQuerySegmentedAsync(query, null);


            string message = "Retrieved all register.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timis
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