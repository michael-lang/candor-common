using NUnit.Framework;

namespace Candor
{
    [TestFixture]
    public class ToCharacterSetAsciiAutoTests
    {
        [Test]
        public void ToCharacterSet_NoCase_1234()
        {
            var s = "1234";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_8999999999999999999()
        {
            var s = "8999999999999999999";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_10()
        {
            var s = "10";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_99()
        {
            var s = "99";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_099()
        {
            var s = "099";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase__99()
        {
            var s = " 99";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_99b()
        {
            var s = "99b";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlphaNumericLower", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_99z()
        {
            var s = "99z";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlphaNumericLower", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_099z()
        {
            var s = "099z";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlphaNumericLower", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_098z()
        {
            var s = "098z";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlphaNumericLower", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_Shiz()
        {
            var s = "Shiz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_ShJz()
        {
            var s = "ShJz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_Sha()
        {
            var s = "Sha";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase_zzz()
        {
            var s = "zzz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlphaLower", cs.Name);
        }
        [Test]
        public void ToCharacterSet_NoCase__zzz()
        {
            var s = " zzz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, true);

            Assert.AreEqual("AsciiAlphaLower", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_1234()
        {
            var s = "1234";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_8999999999999999999()
        {
            var s = "8999999999999999999";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_10()
        {
            var s = "10";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_99()
        {
            var s = "99";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_099()
        {
            var s = "099";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase__99()
        {
            var s = " 99";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("Numeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_99b()
        {
            var s = "99b";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlphaNumeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_99z()
        {
            var s = "99z";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlphaNumeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_099z()
        {
            var s = "099z";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlphaNumeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_098z()
        {
            var s = "098z";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlphaNumeric", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_Shiz()
        {
            var s = "Shiz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_ShJz()
        {
            var s = "ShJz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_Sha()
        {
            var s = "Sha";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_ShA()
        {
            var s = "ShA";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase_zzz()
        {
            var s = "zzz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);

            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
        [Test]
        public void ToCharacterSet_YesCase__zzz()
        {
            var s = " zzz";
            var cs = LexicalCharacterSetType.AsciiAuto.ToCharacterSet(s, false);
            
            Assert.AreEqual("AsciiAlpha", cs.Name);
        }
    }
}
