using Desafio.Umbler.Models;
using Desafio.Umbler.Repositories;
using Desafio.Umbler.Services;
using Desafio.Umbler.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainServiceTests
    {
        [TestMethod]
        public async Task GetDomainInfoAsync_With_WhoisService_Mock_Returns_DomainViewModel()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";
            var ipAddress = "192.168.0.1";

            // Mock WhoisResponse para o domínio
            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Organization",
                Raw = "Domain: test.com\nRegistrar: Test Registrar"
            };

            // Mock WhoisResponse para o IP
            var ipWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Host Company",
                Raw = "IP: 192.168.0.1\nOrganization: Test Host Company"
            };

            // Mock DnsQueryResult
            var dnsResult = new DnsQueryResult
            {
                IpAddress = ipAddress,
                Ttl = 3600,
                HasRecord = true
            };

            var nameServers = new List<string> { "ns1.test.com", "ns2.test.com" };

            // Setup dos mocks
            mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            mockWhoisService.Setup(s => s.QueryAsync(ipAddress))
                .ReturnsAsync(ipWhoisResponse);

            mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

            mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync((Domain)null);

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var settings = new DomainSettings { MinimumTtlSeconds = 60, MemoryCacheExpirationMinutes = 5 };

            var domainService = new DomainService(
                mockDnsService.Object,
                mockWhoisService.Object,
                mockRepository.Object,
                memoryCache,
                settings,
                mockLogger.Object);

            //act
            var result = await domainService.GetDomainInfoAsync(domainName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(domainName, result.Name);
            Assert.AreEqual(ipAddress, result.Ip);
            Assert.AreEqual("Test Host Company", result.HostedAt);
            Assert.IsNotNull(result.NameServers);
            Assert.AreEqual(2, result.NameServers.Count);

            // Verificar que os métodos foram chamados
            mockWhoisService.Verify(s => s.QueryAsync(domainName), Times.Once);
            mockWhoisService.Verify(s => s.QueryAsync(ipAddress), Times.Once);
            mockDnsService.Verify(s => s.QueryAsync(domainName), Times.Once);
            mockDnsService.Verify(s => s.GetNameServersAsync(domainName), Times.Once);
            mockRepository.Verify(r => r.AddAsync(It.IsAny<Domain>()), Times.Once);
            mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_Returns_Cached_Domain_When_TTL_Not_Expired()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";
            var cachedDomain = new Domain
            {
                Id = 1,
                Name = domainName,
                Ip = "192.168.0.1",
                HostedAt = "Cached Host",
                Ttl = 3600,
                UpdatedAt = DateTime.Now.AddMinutes(-30), // Atualizado há 30 minutos, TTL de 1 hora ainda válido
                WhoIs = "Cached WHOIS"
            };

            var nameServers = new List<string> { "ns1.test.com" };

            mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync(cachedDomain);

            mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var settings = new DomainSettings { MinimumTtlSeconds = 60, MemoryCacheExpirationMinutes = 5 };

            var domainService = new DomainService(
                mockDnsService.Object,
                mockWhoisService.Object,
                mockRepository.Object,
                memoryCache,
                settings,
                mockLogger.Object);

            //act
            var result = await domainService.GetDomainInfoAsync(domainName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(domainName, result.Name);
            Assert.AreEqual(cachedDomain.Ip, result.Ip);
            Assert.AreEqual(cachedDomain.HostedAt, result.HostedAt);

            // Verificar que serviços externos NÃO foram chamados (cache válido)
            mockWhoisService.Verify(s => s.QueryAsync(It.IsAny<string>()), Times.Never);
            mockDnsService.Verify(s => s.QueryAsync(It.IsAny<string>()), Times.Never);
            
            // Apenas NameServers deve ser consultado
            mockDnsService.Verify(s => s.GetNameServersAsync(domainName), Times.Once);
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_Updates_When_TTL_Expired()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";
            var oldIp = "192.168.0.1";
            var newIp = "192.168.0.2";

            // Domain com TTL expirado (atualizado há mais tempo que o TTL)
            var cachedDomain = new Domain
            {
                Id = 1,
                Name = domainName,
                Ip = oldIp,
                HostedAt = "Old Host",
                Ttl = 60, // TTL de 60 segundos
                UpdatedAt = DateTime.Now.AddMinutes(-2), // Atualizado há 2 minutos, TTL expirado
                WhoIs = "Old WHOIS"
            };

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "New Organization",
                Raw = "New WHOIS data"
            };

            var ipWhoisResponse = new WhoisResponse
            {
                OrganizationName = "New Host Company",
                Raw = "New IP WHOIS"
            };

            var dnsResult = new DnsQueryResult
            {
                IpAddress = newIp,
                Ttl = 120,
                HasRecord = true
            };

            var nameServers = new List<string> { "ns1.new.com" };

            mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync(cachedDomain);

            mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            mockWhoisService.Setup(s => s.QueryAsync(newIp))
                .ReturnsAsync(ipWhoisResponse);

            mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

            mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var settings = new DomainSettings { MinimumTtlSeconds = 60, MemoryCacheExpirationMinutes = 5 };

            var domainService = new DomainService(
                mockDnsService.Object,
                mockWhoisService.Object,
                mockRepository.Object,
                memoryCache,
                settings,
                mockLogger.Object);

            //act
            var result = await domainService.GetDomainInfoAsync(domainName);

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(domainName, result.Name);
            Assert.AreEqual(newIp, result.Ip); // Novo IP
            Assert.AreEqual("New Host Company", result.HostedAt); // Novo host

            // Verificar que serviços foram chamados para atualizar (TTL expirado)
            mockWhoisService.Verify(s => s.QueryAsync(domainName), Times.Once);
            mockWhoisService.Verify(s => s.QueryAsync(newIp), Times.Once);
            mockDnsService.Verify(s => s.QueryAsync(domainName), Times.Once);
            mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Domain>()), Times.Once);
        }
    }
}

