using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TimesAzureFunctions.Function.Entities;
using TimesAzureFunctions.Functions.Entities;

namespace TimesAzureFunctions.Test.Helpers
{
    public class TestFactory
    {
        public static TimeEntity GetBiometricEntity()
        {
            return new TimeEntity
            {
                ETag = "*",
                PartitionKey = "Employed Record",
                RowKey = Guid.NewGuid().ToString(),
                Id = new Random().Next(1, 6),
                type = new Random().Next(0, 2),
                dateCreate = DateTime.UtcNow,
                consolidate = false,
            };
        }
        public static List<TimeEntity> GetBiometricEntities()
        {
            return new List<TimeEntity>();
        }

        public static List<ConsolidateBiometricEntity> GetBiometricConsolidatedEntities()
        {
            return new List<ConsolidateBiometricEntity>();
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid timeId, TimeEntity TimeRequest)
        {
            string request = JsonConvert.SerializeObject(TimeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{timeId}"
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(Guid timeId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{timeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(TimeEntity TimeRequest)
        {
            string request = JsonConvert.SerializeObject(TimeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }
        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }
        public static TimeEntity GeTimerRequest()
        {
            return new TimeEntity
            {
                Id = new Random().Next(1, 6),
                type = new Random().Next(0, 2),
                dateCreate = DateTime.UtcNow,
                consolidate = false
            };
        }

        private static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public static DefaultHttpRequest CreateHttpRequest(string date)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{date}"
            };
        }

        public static ILogger CreateLogger(LoggerTypes type= LoggerTypes.Null)
        {
            ILogger logger;
            if(type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}
