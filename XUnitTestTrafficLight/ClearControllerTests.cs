using System;
using System.Threading.Tasks;
using Xunit;
using TrafficLight.Models;
using Moq;
using TrafficLight.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace XUnitTestTrafficLight
{
    public class ClearControllerTests
    {
        [Fact]
        public async Task Clear()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            var controller = new ClearController(mockDb.Object);
            int expectedCode = 200;

            // Act
            var actionResult = await controller.Index();
            var statusCodeResult = (IStatusCodeActionResult)actionResult;

            // Assert
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
        }
    }
}
