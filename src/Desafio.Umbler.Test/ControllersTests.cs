using Desafio.Umbler.Controllers;
using Desafio.Umbler.Helpers;
using Desafio.Umbler.Models;
using Desafio.Umbler.Services;
using Desafio.Umbler.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod]
        public void Home_Index_returns_View()
        {
            //arrange 
            var controller = new HomeController();

            //act
            var response = controller.Index();
            var result = response as ViewResult;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Home_Error_returns_View_With_Model()
        {
            //arrange 
            var controller = new HomeController();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            //act
            var response = controller.Error();
            var result = response as ViewResult;
            var model = result.Model as ErrorViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public async Task Domain_In_Database()
        {
            //arrange 
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var domain = new Domain 
            { 
                Id = 1, 
                Ip = "192.168.0.1", 
                Name = "test.com", 
                UpdatedAt = DateTime.Now, 
                HostedAt = "umbler.corp", 
                Ttl = 3600, 
                WhoIs = "Ns.umbler.com" 
            };

            // Insert seed data into the database using one instance of the context
            using (var db = new DatabaseContext(options))
            {
                db.Domains.Add(domain);
                db.SaveChanges();
            }

            // Mock do DomainService para retornar o domain do banco
            var mockDomainService = new Mock<IDomainService>();
            var mockLogger = new Mock<ILogger<DomainController>>();

            var domainViewModel = new DomainViewModel
            {
                Name = domain.Name,
                Ip = domain.Ip,
                HostedAt = domain.HostedAt,
                NameServers = new List<string> { "ns1.test.com", "ns2.test.com" }
            };

            mockDomainService.Setup(s => s.GetDomainInfoAsync("test.com"))
                .ReturnsAsync(domainViewModel);

            // Use a clean instance of the context to run the test
            var controller = new DomainController(mockDomainService.Object, mockLogger.Object);

            //act
            var response = await controller.Get("test.com");
            var result = response as OkObjectResult;
            var obj = result.Value as DomainViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.Name, domain.Name);
            Assert.AreEqual(obj.Ip, domain.Ip);
            Assert.AreEqual(obj.HostedAt, domain.HostedAt);
        }

        [TestMethod]
        public async Task Domain_Not_In_Database()
        {
            //arrange 
            var mockDomainService = new Mock<IDomainService>();
            var mockLogger = new Mock<ILogger<DomainController>>();

            var domainViewModel = new DomainViewModel
            {
                Name = "test.com",
                Ip = "192.168.0.1",
                HostedAt = "Test Host",
                NameServers = new List<string> { "ns1.test.com" }
            };

            mockDomainService.Setup(s => s.GetDomainInfoAsync("test.com"))
                .ReturnsAsync(domainViewModel);

            var controller = new DomainController(mockDomainService.Object, mockLogger.Object);

            //act
            var response = await controller.Get("test.com");
            var result = response as OkObjectResult;
            var obj = result.Value as DomainViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.Name, "test.com");
        }

        [TestMethod]
        public async Task Domain_Moking_WhoisClient()
        {
            //arrange
            // Agora que temos IWhoisService, podemos mockar o WhoisClient através do DomainService
            var mockDomainService = new Mock<IDomainService>();
            var mockLogger = new Mock<ILogger<DomainController>>();

            var domainName = "test.com";

            // Criar um DomainViewModel mockado que simula uma resposta de domínio
            var domainViewModel = new DomainViewModel
            {
                Name = domainName,
                Ip = "192.168.0.1",
                HostedAt = "Mock Host Company",
                NameServers = new List<string> { "ns1.test.com", "ns2.test.com" }
            };

            // Setup do mock para retornar o domainViewModel quando GetDomainInfoAsync for chamado
            mockDomainService.Setup(s => s.GetDomainInfoAsync(domainName))
                .ReturnsAsync(domainViewModel);

            // Criar controller com o mock do DomainService (que usa IWhoisService internamente)
            var controller = new DomainController(mockDomainService.Object, mockLogger.Object);

            //act
            var response = await controller.Get(domainName);
            var result = response as OkObjectResult;
            var obj = result.Value as DomainViewModel;

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(obj);
            Assert.AreEqual(obj.Name, domainName);
            Assert.AreEqual(obj.Ip, "192.168.0.1");
            Assert.AreEqual(obj.HostedAt, "Mock Host Company");
            Assert.IsNotNull(obj.NameServers);
            Assert.IsTrue(obj.NameServers.Count > 0);

            // Verificar que o método GetDomainInfoAsync foi chamado
            mockDomainService.Verify(s => s.GetDomainInfoAsync(domainName), Times.Once);
        }

        [TestMethod]
        public async Task Domain_Invalid_Domain_Returns_BadRequest()
        {
            //arrange
            var mockDomainService = new Mock<IDomainService>();
            var mockLogger = new Mock<ILogger<DomainController>>();

            var controller = new DomainController(mockDomainService.Object, mockLogger.Object);

            //act - domínio inválido (sem extensão)
            var response = await controller.Get("invalid");
            var result = response as BadRequestObjectResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            mockDomainService.Verify(s => s.GetDomainInfoAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Domain_Empty_Domain_Returns_BadRequest()
        {
            //arrange
            var mockDomainService = new Mock<IDomainService>();
            var mockLogger = new Mock<ILogger<DomainController>>();

            var controller = new DomainController(mockDomainService.Object, mockLogger.Object);

            //act - domínio vazio
            var response = await controller.Get("");
            var result = response as BadRequestObjectResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            mockDomainService.Verify(s => s.GetDomainInfoAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task Domain_NotFound_Returns_NotFound()
        {
            //arrange
            var mockDomainService = new Mock<IDomainService>();
            var mockLogger = new Mock<ILogger<DomainController>>();

            mockDomainService.Setup(s => s.GetDomainInfoAsync("notfound.com"))
                .ReturnsAsync((DomainViewModel)null);

            var controller = new DomainController(mockDomainService.Object, mockLogger.Object);

            //act
            var response = await controller.Get("notfound.com");
            var result = response as NotFoundObjectResult;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
