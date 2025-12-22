using Desafio.Umbler.Models;
using Desafio.Umbler.Repositories;
using Desafio.Umbler.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainServiceErrorTests
    {
        [TestMethod]
        public async Task GetDomainInfoAsync_DnsQueryFails_ReturnsNull()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Org",
                Raw = "Test WHOIS"
            };

            // DNS retorna sem registro
            var dnsResult = new DnsQueryResult
            {
                HasRecord = false,
                IpAddress = null,
                Ttl = 0
            };

            mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

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
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_WhoisServiceThrowsException_ThrowsException()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";

            mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ThrowsAsync(new Exception("WHOIS service unavailable"));

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

            //act & assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await domainService.GetDomainInfoAsync(domainName));
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_DnsServiceThrowsException_ThrowsException()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Org",
                Raw = "Test WHOIS"
            };

            mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ThrowsAsync(new Exception("DNS service unavailable"));

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

            //act & assert
            await Assert.ThrowsExceptionAsync<Exception>(async () =>
                await domainService.GetDomainInfoAsync(domainName));
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_EmptyIpAddress_ReturnsNull()
        {
            //arrange
            var mockWhoisService = new Mock<IWhoisService>();
            var mockDnsService = new Mock<IDnsService>();
            var mockRepository = new Mock<IDomainRepository>();
            var mockLogger = new Mock<ILogger<DomainService>>();

            var domainName = "test.com";

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Org",
                Raw = "Test WHOIS"
            };

            // DNS retorna com HasRecord=true mas IP vazio
            var dnsResult = new DnsQueryResult
            {
                HasRecord = true,
                IpAddress = "",
                Ttl = 3600
            };

            mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

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
            Assert.IsNull(result);
        }
    }
}

