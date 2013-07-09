using NUnit.Framework;

namespace Candor.StringExtensions
{
    [TestFixture]
    public class LexicalAddNumericTests
    {
        [Test]
        public void Add_00000_1000()
        {
            var s = "00000";
            var s2 = s.LexicalAdd(LexicalCharacterSet.Numeric, false, 1000);

            Assert.AreEqual("01000", s2);
        }
        [Test]
        public void Add_00055_45()
        {
            var s = "00055";
            var s2 = s.LexicalAdd(LexicalCharacterSet.Numeric, false, 45);

            Assert.AreEqual("00100", s2);
        }
        [Test]
        public void Add_00028_99()
        {
            var s = "00028";
            var s2 = s.LexicalAdd(LexicalCharacterSet.Numeric, false, 99);

            Assert.AreEqual("00127", s2);
        }
        [Test]
        public void Add_00028_87()
        {
            var s = "00028";
            var s2 = s.LexicalAdd(LexicalCharacterSet.Numeric, false, 87);

            Assert.AreEqual("00115", s2);
        }
        [Test]
        public void Add_99999_87()
        {
            var s = "99999";
            var s2 = s.LexicalAdd(LexicalCharacterSet.Numeric, false, 87);

            Assert.AreEqual("000086", s2);
        }
        [Test]
        public void Add_099999_87()
        {
            var s = "099999";
            var s2 = s.LexicalAdd(LexicalCharacterSet.Numeric, false, 87);

            Assert.AreEqual("100086", s2);
        }
    }
}

// Use this to get what a value should be for LexicalAdd.
//var tmp = source;
//for (int i = 0; i < count; i++)
//{
//    tmp = tmp.LexicalIncrement(charSet, ignoreCase);
//}
//return tmp;