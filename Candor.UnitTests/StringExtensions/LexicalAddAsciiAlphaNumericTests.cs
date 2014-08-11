using NUnit.Framework;

namespace Candor.StringExtensions
{
    [TestFixture]
    public class LexicalAddAsciiAlphaNumericTests
    {
        [Test]
        public void Add_NoCase_Mike_36()
        {
            var s = "Mike";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlphaNumeric, true, 36);

            Assert.AreEqual("Mile", s2);
        }
        [Test]
        public void Add_NoCase_Mije_36()
        {
            var s = "Mije";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlphaNumeric, true, 25);

            Assert.AreEqual("Mik3", s2);
        }
        [Test]
        public void Add_NoCase_A_Z_1()
        {
            var s = "A-Z";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlphaNumeric, true, true, 1);

            Assert.AreEqual("A00", s2);
        }



        [Test]
        public void Add_YesCase_Mike_36()
        {
            var s = "Mike";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlphaNumeric, false, 36);

            Assert.AreEqual("Mikw", s2);
        }
        [Test]
        public void Add_YesCase_Mije_36()
        {
            var s = "Mije";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlphaNumeric, false, 25);

            Assert.AreEqual("MijR", s2);
        }
    }
}
