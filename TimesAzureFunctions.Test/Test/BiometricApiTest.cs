using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TimesAzureFunctions.Function.Entities;
using TimesAzureFunctions.Function.Functions;
using TimesAzureFunctions.Test.Helpers;
using Xunit;

namespace TimesAzureFunctions.Test.Test
{
    public class BiometricApiTest
    {
        public readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void RegisterEmployed_Should_Return200()
        {
            //Arrange
            MockCloudTableEmployeBiometric mockBio = new MockCloudTableEmployeBiometric(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            TimeEntity TimeRequets = TestFactory.GeTimerRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(TimeRequets);

            //Act
            IActionResult response = await TimesApi.RegisterEmployed(request, mockBio, logger);
            //Assert 
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }
        [Fact]
        public async void UpdateBiometricEmployed_Should_Return200()
        {
            //Arrange
            MockCloudTableEmployeBiometric mockBio = new MockCloudTableEmployeBiometric(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            TimeEntity TimeRequets = TestFactory.GeTimerRequest();
            Guid Idr = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(Idr,TimeRequets);
            //Act
            IActionResult response = await TimesApi.updateEmployed(request, mockBio, Idr.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void GetregisterById_Should_Return200()
        {
            //Arrange
            TimeEntity TimeEntity = TestFactory.GetBiometricEntity();
            Guid Idr = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(Idr);
            //Act
            IActionResult response = TimesApi.GetregisterById(request, TimeEntity, Idr.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        }
        [Fact]
        public async void GetAllRegister_Should_Return200()
        {
            //Arrange 
            MockCloudTableEmployeBiometric mockBio = new MockCloudTableEmployeBiometric(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //Act
            IActionResult response = await TimesApi.GetAllRegister(request, mockBio, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeleteRegister_Should_Return200()
        {
            //Arrange
            MockCloudTableEmployeBiometric mockBio = new MockCloudTableEmployeBiometric(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            TimeEntity TimeEntitiy = TestFactory.GetBiometricEntity();
            Guid Idr = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(Idr);

            //Act
            IActionResult response = await TimesApi.DeleteRegister(request, TimeEntitiy, mockBio, Idr.ToString(), logger);
        }

    }
}
