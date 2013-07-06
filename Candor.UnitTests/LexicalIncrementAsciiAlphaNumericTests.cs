using NUnit.Framework;

namespace Candor
{
    [TestFixture]
    public class LexicalIncrementAsciiAlphaNumericTests
    {
        [Test]
        public void LexicalIncrement_NoCase_1234()
        {
            var s = "1234";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("1235", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_8999999999999999999()
        {
            var s = "8999999999999999999";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("899999999999999999a", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_10()
        {
            var s = "10";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("11", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_99()
        {
            var s = "99";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("9a", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_099()
        {
            var s = "099";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("09a", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase__99()
        {
            var s = " 99";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual(" 9a", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_99b()
        {
            var s = "99b";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("99c", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_99z()
        {
            var s = "99z";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("9a0", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_099z()
        {
            var s = "099z";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("09a0", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_098z()
        {
            var s = "098z";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("0990", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_Shiz()
        {
            var s = "Shiz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("ShJ0", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_ShJz()
        {
            var s = "ShJz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("ShK0", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_Sha()
        {
            var s = "Sha";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("ShB", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase_zzz()
        {
            var s = "zzz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("zzz0", s2);
        }
        [Test]
        public void LexicalIncrement_NoCase__zzz()
        {
            var s = " zzz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, true);

            Assert.AreEqual("0000", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_1234()
        {
            var s = "1234";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("1235", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_8999999999999999999()
        {
            var s = "8999999999999999999";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("899999999999999999A", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_10()
        {
            var s = "10";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("11", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_99()
        {
            var s = "99";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("9A", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_099()
        {
            var s = "099";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("09A", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase__99()
        {
            var s = " 99";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual(" 9A", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_99b()
        {
            var s = "99b";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("99C", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_99z()
        {
            var s = "99z";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("9A0", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_099z()
        {
            var s = "099z";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("09A0", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_098z()
        {
            var s = "098z";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("0990", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_Shiz()
        {
            var s = "Shiz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("ShJ0", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_ShJz()
        {
            var s = "ShJz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("Shj0", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_Sha()
        {
            var s = "Sha";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("ShB", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_ShA()
        {
            var s = "ShA";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("Sha", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase_zzz()
        {
            var s = "zzz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("zzz0", s2);
        }
        [Test]
        public void LexicalIncrement_YesCase__zzz()
        {
            var s = " zzz";
            var s2 = s.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric, false);

            Assert.AreEqual("0000", s2);
        }
    }
}
