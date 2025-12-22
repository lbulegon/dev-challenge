using Desafio.Umbler.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class DomainValidatorTests
    {
        [TestMethod]
        public void ValidateDomain_ValidDomain_ReturnsValid()
        {
            //arrange
            var domain = "umbler.com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsTrue(result.IsValid);
            Assert.IsNull(result.ErrorMessage);
            Assert.AreEqual("umbler.com", result.NormalizedDomain);
        }

        [TestMethod]
        public void ValidateDomain_ValidDomainWithSubdomain_ReturnsValid()
        {
            //arrange
            var domain = "www.umbler.com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual("umbler.com", result.NormalizedDomain);
        }

        [TestMethod]
        public void ValidateDomain_ValidDomainWithProtocol_ReturnsValid()
        {
            //arrange
            var domain = "https://umbler.com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual("umbler.com", result.NormalizedDomain);
        }

        [TestMethod]
        public void ValidateDomain_EmptyDomain_ReturnsInvalid()
        {
            //arrange
            var domain = "";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.AreEqual("Nome do domínio é obrigatório", result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateDomain_NullDomain_ReturnsInvalid()
        {
            //arrange
            string domain = null;

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateDomain_DomainWithoutExtension_ReturnsInvalid()
        {
            //arrange
            var domain = "umbler";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateDomain_DomainWithSpaces_ReturnsInvalid()
        {
            //arrange
            var domain = "umbler .com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("espaços"));
        }

        [TestMethod]
        public void ValidateDomain_DomainStartingWithDot_ReturnsInvalid()
        {
            //arrange
            var domain = ".umbler.com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("ponto"));
        }

        [TestMethod]
        public void ValidateDomain_DomainWithConsecutiveDots_ReturnsInvalid()
        {
            //arrange
            var domain = "umbler..com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("pontos consecutivos"));
        }

        [TestMethod]
        public void ValidateDomain_DomainStartingWithHyphen_ReturnsInvalid()
        {
            //arrange
            var domain = "-umbler.com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.ErrorMessage.Contains("hífen"));
        }

        [TestMethod]
        public void ValidateDomain_ValidDomainWithMultipleSubdomains_ReturnsValid()
        {
            //arrange
            var domain = "api.v1.umbler.com";

            //act
            var result = DomainValidator.ValidateDomain(domain);

            //assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual("api.v1.umbler.com", result.NormalizedDomain);
        }
    }
}

