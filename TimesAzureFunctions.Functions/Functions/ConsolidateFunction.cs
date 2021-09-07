using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimesAzureFunctions.common.Responses;
using TimesAzureFunctions.Function.Entities;

namespace TimesAzureFunctions.Functions.Entities
{
    public static class ConsolidateFunction
    {
        [FunctionName("consolidateMinutes")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo Timer,
            [Table("times", Connection = "AzureWebJobsStorage")] CloudTable consolidateBiometricTable,
            [Table("ConsolidatedBiometric", Connection = "AzureWebJobsStorage")] CloudTable TimeConsolidatedTable,
            ILogger log)
        {

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> EmploiesConsolidated = await consolidateBiometricTable.ExecuteQuerySegmentedAsync(query, null);

            List<TimeEntity> EmploConsoOrdered = EmploiesConsolidated
                                                .Where(x => x.consolidate.Equals(false))
                                                .OrderBy(x => x.dateCreate)
                                                .OrderBy(x => x.Id)
                                                .ToList();

            int i = 1;
            int cantConsolidate = 0;
            foreach (TimeEntity EmploBiomeEntity in EmploConsoOrdered)
            {
                int wMin = 0;
                if (!(EmploBiomeEntity).Equals(EmploConsoOrdered.Last()))
                {
                    if (EmploBiomeEntity.Id.Equals(EmploConsoOrdered[i].Id) &&
                        EmploConsoOrdered[i].type.Equals(1))
                    {
                        TimeSpan workedTime = EmploConsoOrdered[i].dateCreate - EmploBiomeEntity.dateCreate;

                        EmploConsoOrdered[i].consolidate = true;
                        EmploBiomeEntity.consolidate = true;

                        TableOperation addOperation = TableOperation.Replace(EmploBiomeEntity);
                        await consolidateBiometricTable.ExecuteAsync(addOperation);

                        TableOperation addOperation2 = TableOperation.Replace(EmploConsoOrdered[i]);
                        await consolidateBiometricTable.ExecuteAsync(addOperation2);

                        string filter = TableQuery.GenerateFilterConditionForInt("Id", QueryComparisons.Equal, EmploBiomeEntity.Id);
                        TableQuery<ConsolidateBiometricEntity> q = new TableQuery<ConsolidateBiometricEntity>().Where(filter);
                        TableQuerySegment<ConsolidateBiometricEntity> timeConsolidated = await TimeConsolidatedTable.ExecuteQuerySegmentedAsync(q, null);

                        ConsolidateBiometricEntity ConBioEnti = timeConsolidated.Results.FirstOrDefault();

                        if (timeConsolidated.Results.Count > 0)
                        {
                            if (ConBioEnti.Date.Equals(EmploBiomeEntity.dateCreate.Date))
                            {
                                ConBioEnti.AcumMinutes = +wMin;

                                TableOperation addOperation3 = TableOperation.Replace(timeConsolidated.Results.FirstOrDefault());
                                await TimeConsolidatedTable.ExecuteAsync(addOperation3);
                            }
                            else
                            {
                                ConsolidateBiometricEntity timeConsolidatedEntity = new ConsolidateBiometricEntity
                                {
                                    Id = EmploBiomeEntity.Id,
                                    Date = EmploBiomeEntity.dateCreate.Date,
                                    AcumMinutes = wMin,
                                    ETag = "*",
                                    RowKey = Guid.NewGuid().ToString(),
                                    PartitionKey = "TIMECONSOLIDATED"
                                };
                                TableOperation addOperation3 = TableOperation.Replace(timeConsolidated.Results.FirstOrDefault());
                                await TimeConsolidatedTable.ExecuteAsync(addOperation3);
                            }
                        }
                        else
                        {
                            ConsolidateBiometricEntity timeConsolidatedEntity = new ConsolidateBiometricEntity
                            {
                                Id = EmploBiomeEntity.Id,
                                Date = EmploBiomeEntity.dateCreate.Date,
                                AcumMinutes = wMin,
                                ETag = "*",
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "TIMECONSOLIDATED"
                            };
                            TableOperation addOperation4 = TableOperation.Insert(timeConsolidatedEntity);
                            await TimeConsolidatedTable.ExecuteAsync(addOperation4);
                        }
                        cantConsolidate += 2;
                    }
                }
                i++;
            }
            log.LogInformation($"{cantConsolidate} has been consolidated");
        }
        [FunctionName(nameof(GetConsolidateByDay))]
        public static async Task<IActionResult> GetConsolidateByDay(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ConsolidatedBiometric/{dateToConvert}")] HttpRequest req,
        [Table("ConsolidatedBiometric", Connection = "AzureWebJobsStorage")] CloudTable TimeConsolidatedTable,
        string dateToConvert,
        ILogger log)
        {
            log.LogInformation("Get all times consolidated received.");

            DateTime date = Convert.ToDateTime(dateToConvert, new CultureInfo("en-US"));

            TableQuery<ConsolidateBiometricEntity> query = new TableQuery<ConsolidateBiometricEntity>();
            TableQuerySegment<ConsolidateBiometricEntity> timeConsolidated = await TimeConsolidatedTable.ExecuteQuerySegmentedAsync(query, null);

            List<ConsolidateBiometricEntity> list = timeConsolidated
                                                    .Where(x => x.Date.Day.Equals(date.Day))
                                                    .ToList();

            string message = "Retrieved all times consolidated in day :" + date.Day;
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = list
            });

        }
    }
}