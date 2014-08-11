using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Candor.StringExtensions
{
    [TestFixture]
    public class ReplaceNotIn
    {
        [Test]
        public void Replace_Co()
        {
            var s = "Co.";
            var s2 = s.ReplaceNotIn(LexicalCharacterSet.AsciiAlphaNumeric.Characters, "");

            Assert.AreEqual("Co", s2);
        }
        [Test]
        public void Replace__Co()
        {
            var s = "_Co.";
            var s2 = s.ReplaceNotIn(LexicalCharacterSet.AsciiAlphaNumeric.Characters, "");

            Assert.AreEqual("Co", s2);
        }
        [Test]
        public void Replace__Co_4_e()
        {
            var s = "_Co$4.e.";
            var s2 = s.ReplaceNotIn(LexicalCharacterSet.AsciiAlphaNumeric.Characters, "");

            Assert.AreEqual("Co4e", s2);
        }
    }
}
