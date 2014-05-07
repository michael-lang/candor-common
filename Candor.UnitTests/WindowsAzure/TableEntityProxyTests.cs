using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Candor.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using NUnit.Framework;

namespace Candor.WindowsAzure
{
    [TestFixture]
    public class TableEntityProxyTests
    {
        [Test]
        public void WriteEntityContact()
        {
            var contact = new Contact
            {
                ContactId = "A",
                DisplayName = "Bob"
            };

            var context = new OperationContext();
            var proxy = new TableEntityProxy<Contact>(contact);
            var props = proxy.WriteEntity(context);
            Assert.AreEqual("A", props["ContactId"].StringValue);
            Assert.AreEqual("Bob", props["DisplayName"].StringValue);
        }
        [Test]
        public void WriteEntityContactMessage()
        {
            var msg = new ContactMessage
            {
                From = new Contact
                {
                    ContactId = "A",
                    DisplayName = "Bob"
                },
                To = new Contact
                {
                    ContactId = "B",
                    DisplayName = "Jane"
                },
                MessageBody = "Test"
            };

            var context = new OperationContext();
            var proxy = new TableEntityProxy<ContactMessage>(msg);
            var props = proxy.WriteEntity(context);
            Assert.AreEqual("A", props["FromContactId"].StringValue);
            Assert.AreEqual("Bob", props["FromDisplayName"].StringValue);
            Assert.AreEqual("B", props["ToContactId"].StringValue);
            Assert.AreEqual("Jane", props["ToDisplayName"].StringValue);
        }
    }
}