using System;
using System.Threading.Tasks;
using Xunit;
using TrafficLight.Models;
using Moq;
using TrafficLight.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace XUnitTestTrafficLight
{
    public class ObservationControllerTests
    {
        [Fact]
        public async Task Add_SendDataWithoutCreatingSequence()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            var controller = new ObservationController(mockDb.Object);
            int expectedCode = 404;
            string expectedResponse = "{ status = error, msg = The sequence isn't found }";
            var obseq = new ObservationWithSequence();

            // Act
            var actionResult = await controller.Add(obseq);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            var response = ((ObjectResult)actionResult).Value.ToString();
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_1110111_0011101()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            //arrange data to send
            var obseq = new ObservationWithSequence();
            obseq.observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;
            int expectedCode = 200;
            string expectedResponse = "{ status = ok, response = { start = [2,8,82,88], missing = [0000000,1000000] } }";
            var controller = new ObservationController(mockDb.Object);

            // Act
            var actionResult = await controller.Add(obseq);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            var response = ((ObjectResult)actionResult).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_1110111_0011101_and_1110111_0010000()
        {
            // Arrange to create sequence
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            int expectedCode = 200;
            string expectedResponse = "{ status = ok, response = { start = [2,8,82,88], missing = [0000000,1000010] } }";
            var controller = new ObservationController(mockDb.Object);

            //arrange data to send 1110111_0011101
            var obseq = new ObservationWithSequence();
            observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;

            // Act 1
            var actionResult1 = await controller.Add(obseq);

            //arrange data to send 1110111_0010000
            var obseq2 = new ObservationWithSequence();
            var observation2 = new Observation();
            observation2.color = "green";
            observation2.numbers = new List<string>() { "1110111", "0010000" };
            obseq2.sequence = sequence;
            obseq2.observation = observation2;

            // Act 2
            var actionResult2 = await controller.Add(obseq2);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult2;
            var response = ((ObjectResult)actionResult2).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_1110111_0011101_and_1110111_0010000_and_red()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            int expectedCode = 200;
            string expectedResponse = "{ status = ok, response = { start = [2], missing = [0000000,1000010] } }";
            var controller = new ObservationController(mockDb.Object);

            //arrange data to send 1110111_0011101
            var obseq = new ObservationWithSequence();
            observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;

            // Act 1
            var actionResult1 = await controller.Add(obseq);

            //arrange data to send 1110111_0010000
            var obseq2 = new ObservationWithSequence();
            var observation2 = new Observation();
            observation2.color = "green";
            observation2.numbers = new List<string>() { "1110111", "0010000" };
            obseq2.sequence = sequence;
            obseq2.observation = observation2;

            // Act 2
            var actionResult2 = await controller.Add(obseq2);

            //arrange data to send RED status
            var obseq3 = new ObservationWithSequence();
            var observation3 = new Observation();
            observation3.color = "red";
            obseq3.sequence = sequence;
            obseq3.observation = observation3;

            //Act 3
            var actionResult3 = await controller.Add(obseq3);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult3;
            var response = ((ObjectResult)actionResult3).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_send_after_red()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            int expectedCode = 422;
            string expectedResponse = "{ status = error, msg = The red observation should be the last }";
            var controller = new ObservationController(mockDb.Object);

            //arrange data to send 1110111_0011101
            var obseq = new ObservationWithSequence();
            observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;

            // Act 1
            var actionResult1 = await controller.Add(obseq);

            //arrange data to send 1110111_0010000
            var obseq2 = new ObservationWithSequence();
            var observation2 = new Observation();
            observation2.color = "green";
            observation2.numbers = new List<string>() { "1110111", "0010000" };
            obseq2.sequence = sequence;
            obseq2.observation = observation2;

            // Act 2
            var actionResult2 = await controller.Add(obseq2);

            //arrange data to send RED status
            var obseq3 = new ObservationWithSequence();
            var observation3 = new Observation();
            observation3.color = "red";
            obseq3.sequence = sequence;
            obseq3.observation = observation3;

            //Act 3
            var actionResult3 = await controller.Add(obseq3);

            //arrange data to send after RED status
            var obseq4 = new ObservationWithSequence();
            var observation4 = new Observation();
            observation4.color = "green";
            observation4.numbers = new List<string>() { "1110111", "0010000" };
            obseq4.sequence = sequence;
            obseq4.observation = observation4;

            //Act 4
            var actionResult4 = await controller.Add(obseq4);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult4;
            var response = ((ObjectResult)actionResult4).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_no_solutions_found()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            int expectedCode = 422;
            string expectedResponse = "{ status = error, msg = No solutions found }";
            var controller = new ObservationController(mockDb.Object);

            //arrange data to send 1110111_0011101
            var obseq = new ObservationWithSequence();
            observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;

            // Act 1
            var actionResult1 = await controller.Add(obseq);

            ////arrange data to send 1110111_0010000
            //var obseq2 = new ObservationWithSequence();
            //var observation2 = new Observation();
            //observation2.color = "green";
            //observation2.numbers = new List<string>() { "1110111", "0010000" };
            //obseq2.sequence = sequence;
            //obseq2.observation = observation2;

            //// Act 2
            //var actionResult2 = await controller.Add(obseq2);

            //arrange data to send RED status
            var obseq3 = new ObservationWithSequence();
            var observation3 = new Observation();
            observation3.color = "red";
            obseq3.sequence = sequence;
            obseq3.observation = observation3;

            //Act 3
            var actionResult3 = await controller.Add(obseq3);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult3;
            var response = ((ObjectResult)actionResult3).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_format_error()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            //arrange data to send
            var obseq = new ObservationWithSequence();
            obseq.observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011" };//ìàëî öèôð
            obseq.sequence = sequence;
            obseq.observation = observation;
            int expectedCode = 400;
            string expectedResponse = "{ status = error, msg = Format error }";
            var controller = new ObservationController(mockDb.Object);

            // Act
            var actionResult = await controller.Add(obseq);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            var response = ((ObjectResult)actionResult).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_duplicate_numbers()//çíà÷åíèÿ îòïðàâëåíû äâàæäû
        {
            // Arrange to create sequence
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            int expectedCode = 422;
            string expectedResponse = "{ status = error, msg = No solutions found }";
            var controller = new ObservationController(mockDb.Object);

            //arrange data to send 1110111_0011101
            var obseq = new ObservationWithSequence();
            observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;

            // Act 1
            var actionResult1 = await controller.Add(obseq);

            //arrange data to send 1110111_0010000
            var obseq2 = new ObservationWithSequence();
            var observation2 = new Observation();
            observation2.color = "green";
            observation2.numbers = new List<string>() { "1110111", "0011101" };
            obseq2.sequence = sequence;
            obseq2.observation = observation2;

            // Act 2
            var actionResult2 = await controller.Add(obseq2);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult2;
            var response = ((ObjectResult)actionResult2).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Add_unknown_color_error()
        {
            // Arrange
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            //arrange data to send
            var obseq = new ObservationWithSequence();
            obseq.observation = new Observation();
            observation.color = "yellow";
            observation.numbers = new List<string>() { "1110111", "0011000" };
            obseq.sequence = sequence;
            obseq.observation = observation;
            int expectedCode = 422;
            string expectedResponse = "{ status = error, msg = Unknown color }";
            var controller = new ObservationController(mockDb.Object);

            // Act
            var actionResult = await controller.Add(obseq);

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            var response = ((ObjectResult)actionResult).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Save()
        {
            // Arrange to create sequence
            var mockDb = new Mock<MyDb>();
            Guid sequence = Guid.NewGuid();
            var observation = new Observation();
            var sd = new SeqData();
            //create sequence
            await mockDb.Object.Set(sequence, sd);
            int expectedCode = 200;
            string expectedResponse = "{ status = ok, response = Export to file is done }";
            var controller = new ObservationController(mockDb.Object);

            //arrange data to send 1110111_0011101
            var obseq = new ObservationWithSequence();
            observation = new Observation();
            observation.color = "green";
            observation.numbers = new List<string>() { "1110111", "0011101" };
            obseq.sequence = sequence;
            obseq.observation = observation;

            // Act 1
            var actionResult1 = await controller.Add(obseq);

            //arrange data to send 1110111_0010000
            var obseq2 = new ObservationWithSequence();
            var observation2 = new Observation();
            observation2.color = "green";
            observation2.numbers = new List<string>() { "1110111", "0010000" };
            obseq2.sequence = sequence;
            obseq2.observation = observation2;

            // Act 2
            var actionResult2 = await controller.Add(obseq2);

            //Save
            var actionResult3 = await controller.Save();

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult3;
            var response = ((ObjectResult)actionResult3).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

        [Fact]
        public async Task Load()
        {
            // Arrange to create sequence
            var mockDb = new Mock<MyDb>();
            int expectedCode = 200;
            string expectedResponse = "{ status = ok, response = Load from file is done }";
            var controller = new ObservationController(mockDb.Object);

            // Act
            var actionResult = await controller.Load();

            // Assert
            var statusCodeResult = (IStatusCodeActionResult)actionResult;
            var response = ((ObjectResult)actionResult).Value.ToString();
            ;
            Assert.Equal(expectedCode, statusCodeResult.StatusCode);
            Assert.Equal(expectedResponse, response);
        }

    }
}
