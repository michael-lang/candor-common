using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Candor.Reflection
{
    [TestFixture]
    public class ReflectionExtensionsTests
    {
        [Test]
        public void ReflectionExtensionDescriptionGetsAttributeValue()
        {
            MockAccount account = new MockAccount() { AccountNumber = 1234567 };

            Assert.AreEqual("The unqiue number for this account", account.Description(p => p.AccountNumber));
        }
        [Test]
        public void ReflectionExtensionDescriptionBlankWhenAttributeMissing()
        {
            MockAccount account = new MockAccount() { AccountNumber = 1234567 };

            Assert.AreEqual("", account.Description(p => p.Blah));
        }
        [Test]
        public void ReflectionExtensionEnumDescriptionGetsAttributeValue()
        {
            MockAccount account = new MockAccount() { AccountNumber = 1234567, AccountType = AccountType.Type1 };

            Assert.AreEqual("The first type of account", account.AccountType.Description());
        }
        [Test]
        public void ReflectionExtensionEnumDescriptionMatchesPropertyNameWhenAttributeMissing()
        {
            MockAccount account = new MockAccount() { AccountNumber = 1234567, AccountType = AccountType.Type2 };

            Assert.AreEqual("Type2", account.AccountType.Description());
        }

        [Test]
        public void ReflectionExtensionDisplayNameGetsAttributeValue()
        {
            MockAccount account = new MockAccount() { AccountNumber = 1234567 };

            Assert.AreEqual("Account Number", account.DisplayName(p => p.AccountNumber));
        }
        [Test]
        public void ReflectionExtensionDisplayNameMatchesPropertyNameWhenAttributeMissing()
        {
            MockAccount account = new MockAccount() { AccountNumber = 1234567 };

            Assert.AreEqual("Blah", account.DisplayName(p => p.Blah));
        }
    }
}
