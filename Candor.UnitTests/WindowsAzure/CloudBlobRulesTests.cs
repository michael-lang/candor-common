using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Candor.WindowsAzure.Storage.Blob;
using NUnit.Framework;

namespace Candor.WindowsAzure
{
    [TestFixture]
    public class CloudBlobRulesTests
    {
        [Test]
        public void GetContainerWithDashes()
        {
            var start = "this--is-my-container-name";
            var end = start.GetValidBlobContainerName();

            Assert.AreEqual("thisismycontainername", end);
        }
        [Test]
        public void GetContainerWithDashesAndCase()
        {
            var start = "This--Is-My-Container-Name";
            var end = start.GetValidBlobContainerName();

            Assert.AreEqual("thisismycontainername", end);
        }
        [Test]
        public void GetContainerWithSpaces()
        {
            var start = "this  is my container name";
            var end = start.GetValidBlobContainerName();

            Assert.AreEqual("thisismycontainername", end);
        }
        [Test]
        public void GetContainerWithJunk()
        {
            var start = "this%#is*my container!name|";
            var end = start.GetValidBlobContainerName();

            Assert.AreEqual("thisismycontainername", end);
        }
        [Test]
        public void GetBlobWithRepeatedDots()
        {
            var start = "f1/folder2.././...//..///asd234as////d321.txt../asu8..txt";
            var end = start.GetValidBlobName();

            Assert.AreEqual("f1/folder2.asd234as/d321.txt.asu8.txt", end);
        }
        [Test]
        public void GetBlobWithRepeatedDotsAndUnderscore()
        {
            var start = "f1/folder2_.././...//..///asd__234as////d321.txt../asu8..txt";
            var end = start.GetValidBlobName();

            Assert.AreEqual("f1/folder2_.asd__234as/d321.txt.asu8.txt", end);
        }
        [Test]
        public void GetBlobWithRepeatedDotsAndOtherNotStrict()
        {
            var start = "f1/folder2_.././...//..///asd 234-as////d321.txt../asu8..txt";
            var end = start.GetValidBlobName(strict:false);

            Assert.AreEqual("f1/folder2_.asd 234-as/d321.txt.asu8.txt", end);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetBlobWithRepeatedDotsAndOtherStrict()
        {
            var start = "f1/folder2_.././...//..///asd 234-as////d321.txt../asu8..txt";
            var end = start.GetValidBlobName(strict:true);

            Assert.AreEqual("f1/folder2_.asd 234-as/d321.txt.asu8.txt", end);
        }
    }
}
