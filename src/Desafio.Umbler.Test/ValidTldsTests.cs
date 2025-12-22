using Desafio.Umbler.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Desafio.Umbler.Test
{
    [TestClass]
    public class ValidTldsTests
    {
        [TestMethod]
        public void IsValid_CommonGtld_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid("com"));
            Assert.IsTrue(ValidTlds.IsValid("org"));
            Assert.IsTrue(ValidTlds.IsValid("net"));
            Assert.IsTrue(ValidTlds.IsValid("edu"));
            Assert.IsTrue(ValidTlds.IsValid("gov"));
        }

        [TestMethod]
        public void IsValid_NewGtld_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid("app"));
            Assert.IsTrue(ValidTlds.IsValid("dev"));
            Assert.IsTrue(ValidTlds.IsValid("io"));
            Assert.IsTrue(ValidTlds.IsValid("tech"));
            Assert.IsTrue(ValidTlds.IsValid("online"));
            Assert.IsTrue(ValidTlds.IsValid("site"));
            Assert.IsTrue(ValidTlds.IsValid("xyz"));
        }

        [TestMethod]
        public void IsValid_CommonCountryCode_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid("br"));
            Assert.IsTrue(ValidTlds.IsValid("us"));
            Assert.IsTrue(ValidTlds.IsValid("uk"));
            Assert.IsTrue(ValidTlds.IsValid("ca"));
            Assert.IsTrue(ValidTlds.IsValid("au"));
            Assert.IsTrue(ValidTlds.IsValid("de"));
            Assert.IsTrue(ValidTlds.IsValid("fr"));
        }

        [TestMethod]
        public void IsValid_CaseInsensitive_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid("COM"));
            Assert.IsTrue(ValidTlds.IsValid("Com"));
            Assert.IsTrue(ValidTlds.IsValid("cOm"));
            Assert.IsTrue(ValidTlds.IsValid("BR"));
            Assert.IsTrue(ValidTlds.IsValid("Br"));
        }

        [TestMethod]
        public void IsValid_WithDotPrefix_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid(".com"));
            Assert.IsTrue(ValidTlds.IsValid(".br"));
            Assert.IsTrue(ValidTlds.IsValid(".io"));
        }

        [TestMethod]
        public void IsValid_InvalidTld_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.IsFalse(ValidTlds.IsValid("invalid"));
            Assert.IsFalse(ValidTlds.IsValid("xxx"));
            Assert.IsFalse(ValidTlds.IsValid("abc123"));
            Assert.IsFalse(ValidTlds.IsValid("test-tld"));
        }

        [TestMethod]
        public void IsValid_EmptyOrNull_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.IsFalse(ValidTlds.IsValid(""));
            Assert.IsFalse(ValidTlds.IsValid(null));
            Assert.IsFalse(ValidTlds.IsValid("   "));
        }

        [TestMethod]
        public void GetAll_ReturnsNonEmptyCollection()
        {
            // Arrange & Act
            var allTlds = ValidTlds.GetAll();

            // Assert
            Assert.IsNotNull(allTlds);
            Assert.IsTrue(System.Linq.Enumerable.Count(allTlds) > 0);
        }

        [TestMethod]
        public void GetAll_ReturnsSortedCollection()
        {
            // Arrange
            var allTlds = ValidTlds.GetAll().ToList();

            // Act
            var sorted = allTlds.OrderBy(tld => tld).ToList();

            // Assert
            CollectionAssert.AreEqual(sorted, allTlds);
        }

        [TestMethod]
        public void Count_ReturnsPositiveNumber()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.Count > 0);
        }

        [TestMethod]
        public void IsValid_AsianCountryCodes_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid("jp"));
            Assert.IsTrue(ValidTlds.IsValid("cn"));
            Assert.IsTrue(ValidTlds.IsValid("in"));
            Assert.IsTrue(ValidTlds.IsValid("kr"));
            Assert.IsTrue(ValidTlds.IsValid("sg"));
            Assert.IsTrue(ValidTlds.IsValid("hk"));
        }

        [TestMethod]
        public void IsValid_SouthAmericanCountryCodes_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(ValidTlds.IsValid("mx"));
            Assert.IsTrue(ValidTlds.IsValid("ar"));
            Assert.IsTrue(ValidTlds.IsValid("cl"));
            Assert.IsTrue(ValidTlds.IsValid("co"));
            Assert.IsTrue(ValidTlds.IsValid("pe"));
        }
    }
}

