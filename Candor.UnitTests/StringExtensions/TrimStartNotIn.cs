using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Candor.StringExtensions
{
    [TestFixture]
    public class TrimStartNotIn
    {
        [Test]
        public void Trim_Co()
        {
            var s = "Co.";
            var s2 = s.TrimStartNotIn(LexicalCharacterSet.AsciiAlphaNumeric.Characters);

            Assert.AreEqual(s, s2);
        }
        [Test]
        public void Trim__Co()
        {
            var s = "_Co.";
            var s2 = s.TrimStartNotIn(LexicalCharacterSet.AsciiAlphaNumeric.Characters);

            Assert.AreEqual("Co.", s2);
        }
    }
}
