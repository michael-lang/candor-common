using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Candor
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
    }
}
