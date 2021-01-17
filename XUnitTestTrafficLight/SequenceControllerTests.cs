using System;
using System.Threading.Tasks;
using Xunit;
using TrafficLight.Models;
using Moq;
using TrafficLight.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace XUnitTestTrafficLight
{
    public class SequenceControllerTests
    {
        [Fact]
        public async Task Create_Sequence()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            var controller = new SequenceController(mockDb.Object);
            int expectedCode = 201;

            // Act
            var actionResult = await controller.Create();
            var statusCodeResult = (IStatusCodeActionResult)actionResult;

            // Assert
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
        }
    }
}
