using Desafio.Umbler.Models;
using Desafio.Umbler.Repositories;
using Desafio.Umbler.Services;
using Desafio.Umbler.ViewModels;
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
    public class DomainServiceCacheTests
    {
        private Mock<IWhoisService> _mockWhoisService;
        private Mock<IDnsService> _mockDnsService;
        private Mock<IDomainRepository> _mockRepository;
        private Mock<ILogger<DomainService>> _mockLogger;
        private IMemoryCache _memoryCache;
        private DomainSettings _settings;

        [TestInitialize]
        public void Setup()
        {
            _mockWhoisService = new Mock<IWhoisService>();
            _mockDnsService = new Mock<IDnsService>();
            _mockRepository = new Mock<IDomainRepository>();
            _mockLogger = new Mock<ILogger<DomainService>>();

            var cacheOptions = Options.Create(new MemoryCacheOptions());
            _memoryCache = new MemoryCache(cacheOptions);
            _settings = new DomainSettings
            {
                MinimumTtlSeconds = 60,
                MemoryCacheExpirationMinutes = 5
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _memoryCache?.Dispose();
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_ReturnsFromMemoryCache_WhenAvailable()
        {
            // Arrange
            var domainName = "test.com";
            var cachedViewModel = new DomainViewModel
            {
                Name = domainName,
                Ip = "192.168.0.1",
                HostedAt = "Test Host",
                NameServers = new System.Collections.Generic.List<string> { "ns1.test.com" }
            };

            var cacheKey = $"domain_info_{domainName.ToLowerInvariant()}";
            _memoryCache.Set(cacheKey, cachedViewModel, TimeSpan.FromMinutes(5));

            var domainService = new DomainService(
                _mockDnsService.Object,
                _mockWhoisService.Object,
                _mockRepository.Object,
                _memoryCache,
                _settings,
                _mockLogger.Object);

            // Act
            var result = await domainService.GetDomainInfoAsync(domainName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(cachedViewModel.Name, result.Name);
            Assert.AreEqual(cachedViewModel.Ip, result.Ip);
            Assert.AreEqual(cachedViewModel.HostedAt, result.HostedAt);

            // Verificar que não chamou o repositório (cache funcionou)
            _mockRepository.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_UsesMemoryCache_AndAddsToCache()
        {
            // Arrange
            var domainName = "test.com";
            var ipAddress = "192.168.0.1";

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Organization",
                Raw = "Domain: test.com"
            };

            var ipWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Host Company",
                Raw = "IP: 192.168.0.1"
            };

            var dnsResult = new DnsQueryResult
            {
                IpAddress = ipAddress,
                Ttl = 3600,
                HasRecord = true
            };

            var nameServers = new System.Collections.Generic.List<string> { "ns1.test.com" };

            _mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            _mockWhoisService.Setup(s => s.QueryAsync(ipAddress))
                .ReturnsAsync(ipWhoisResponse);

            _mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

            _mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            _mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync((Domain)null);

            var domainService = new DomainService(
                _mockDnsService.Object,
                _mockWhoisService.Object,
                _mockRepository.Object,
                _memoryCache,
                _settings,
                _mockLogger.Object);

            // Act - Primeira chamada (não está no cache)
            var result1 = await domainService.GetDomainInfoAsync(domainName);

            // Assert - Primeira chamada
            Assert.IsNotNull(result1);
            Assert.AreEqual(domainName, result1.Name);
            _mockRepository.Verify(r => r.GetByNameAsync(domainName), Times.Once);

            // Act - Segunda chamada (deve estar no cache)
            var result2 = await domainService.GetDomainInfoAsync(domainName);

            // Assert - Segunda chamada (não deve chamar repositório novamente)
            Assert.IsNotNull(result2);
            Assert.AreEqual(result1.Name, result2.Name);
            Assert.AreEqual(result1.Ip, result2.Ip);
            _mockRepository.Verify(r => r.GetByNameAsync(domainName), Times.Once); // Ainda apenas 1 vez
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_AppliesMinimumTtl_WhenTtlFromDnsIsLower()
        {
            // Arrange
            var domainName = "test.com";
            var ipAddress = "192.168.0.1";
            var minimumTtl = 60;
            var dnsTtl = 30; // TTL do DNS é menor que o mínimo

            _settings.MinimumTtlSeconds = minimumTtl;

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Organization",
                Raw = "Domain: test.com"
            };

            var ipWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Host Company",
                Raw = "IP: 192.168.0.1"
            };

            var dnsResult = new DnsQueryResult
            {
                IpAddress = ipAddress,
                Ttl = dnsTtl,
                HasRecord = true
            };

            var nameServers = new System.Collections.Generic.List<string>();

            _mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            _mockWhoisService.Setup(s => s.QueryAsync(ipAddress))
                .ReturnsAsync(ipWhoisResponse);

            _mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

            _mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            _mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync((Domain)null);

            var domainService = new DomainService(
                _mockDnsService.Object,
                _mockWhoisService.Object,
                _mockRepository.Object,
                _memoryCache,
                _settings,
                _mockLogger.Object);

            // Act
            await domainService.GetDomainInfoAsync(domainName);

            // Assert
            // Verificar que o TTL mínimo foi aplicado
            _mockRepository.Verify(r => r.AddAsync(It.Is<Domain>(d => d.Ttl >= minimumTtl)), Times.Once);
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_UsesDnsTtl_WhenHigherThanMinimum()
        {
            // Arrange
            var domainName = "test.com";
            var ipAddress = "192.168.0.1";
            var minimumTtl = 60;
            var dnsTtl = 3600; // TTL do DNS é maior que o mínimo

            _settings.MinimumTtlSeconds = minimumTtl;

            var domainWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Organization",
                Raw = "Domain: test.com"
            };

            var ipWhoisResponse = new WhoisResponse
            {
                OrganizationName = "Test Host Company",
                Raw = "IP: 192.168.0.1"
            };

            var dnsResult = new DnsQueryResult
            {
                IpAddress = ipAddress,
                Ttl = dnsTtl,
                HasRecord = true
            };

            var nameServers = new System.Collections.Generic.List<string>();

            _mockWhoisService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(domainWhoisResponse);

            _mockWhoisService.Setup(s => s.QueryAsync(ipAddress))
                .ReturnsAsync(ipWhoisResponse);

            _mockDnsService.Setup(s => s.QueryAsync(domainName))
                .ReturnsAsync(dnsResult);

            _mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            _mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync((Domain)null);

            var domainService = new DomainService(
                _mockDnsService.Object,
                _mockWhoisService.Object,
                _mockRepository.Object,
                _memoryCache,
                _settings,
                _mockLogger.Object);

            // Act
            await domainService.GetDomainInfoAsync(domainName);

            // Assert
            // Verificar que o TTL do DNS foi usado (não o mínimo)
            _mockRepository.Verify(r => r.AddAsync(It.Is<Domain>(d => d.Ttl == dnsTtl)), Times.Once);
        }

        [TestMethod]
        public async Task GetDomainInfoAsync_RespectsMinimumTtl_WhenCheckingExpiration()
        {
            // Arrange
            var domainName = "test.com";
            var minimumTtl = 120;
            var domainTtl = 30; // TTL do domínio é menor que o mínimo

            _settings.MinimumTtlSeconds = minimumTtl;

            var existingDomain = new Domain
            {
                Name = domainName,
                Ip = "192.168.0.1",
                UpdatedAt = DateTime.Now.AddSeconds(-90), // 90 segundos atrás
                Ttl = domainTtl,
                HostedAt = "Test Host"
            };

            var nameServers = new System.Collections.Generic.List<string> { "ns1.test.com" };

            _mockRepository.Setup(r => r.GetByNameAsync(domainName))
                .ReturnsAsync(existingDomain);

            _mockDnsService.Setup(s => s.GetNameServersAsync(domainName))
                .ReturnsAsync(nameServers);

            var domainService = new DomainService(
                _mockDnsService.Object,
                _mockWhoisService.Object,
                _mockRepository.Object,
                _memoryCache,
                _settings,
                _mockLogger.Object);

            // Act
            var result = await domainService.GetDomainInfoAsync(domainName);

            // Assert
            // Deve usar o domínio do cache porque o TTL efetivo (mínimo) ainda não expirou
            // 90 segundos < 120 segundos (TTL mínimo)
            Assert.IsNotNull(result);
            Assert.AreEqual(existingDomain.Name, result.Name);
            
            // Não deve atualizar porque o TTL efetivo ainda é válido
            _mockWhoisService.Verify(s => s.QueryAsync(It.IsAny<string>()), Times.Never);
            _mockDnsService.Verify(s => s.QueryAsync(It.IsAny<string>()), Times.Never);
        }
    }
}

