using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TimesAzureFunctions.Functions.Entities;
using TimesAzureFunctions.Test.Helpers;
using Xunit;

namespace TimesAzureFunctions.Test.Test
{
    public class ConsolidatedBiometricApiTest
    {
        public ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

        [Fact]
        public void consolidateMinutes_Should_Log_Message()
        {
            //Arrange
            MockCloudTableEmployeBiometric mockBio = new MockCloudTableEmployeBiometric(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableConsolidatedBiomet mockCon = new MockCloudTableConsolidatedBiomet(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            //Act
            ConsolidateFunction.Run(null,mockBio, mockCon, logger);
            string message = logger.Logs[1];
            //Assert
            Assert.Contains("has been consolidated", message);
        }

        [Fact]
        public async void GetConsolidateByDay_Should_Log_Message()
        {
            //Arrange
            ILogger logger = TestFactory.CreateLogger();
            string date ="09-22-2021";
            DefaultHttpRequest timeConRequest = TestFactory.CreateHttpRequest(date);
            MockCloudTableConsolidatedBiomet mockCon = new MockCloudTableConsolidatedBiomet(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));

            //Act
            IActionResult response = await ConsolidateFunction.GetConsolidateByDay(timeConRequest, mockCon, date, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

    }
}
