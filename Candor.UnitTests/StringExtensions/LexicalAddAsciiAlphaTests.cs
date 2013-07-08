using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Candor.StringExtensions
{
    [TestFixture]
    public class LexicalAddAsciiAlphaTests
    {
        [Test]
        public void Add_NoCase_Mike_26()
        {
            var s = "Mike";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlpha, true, 26);

            Assert.AreEqual("Mile", s2);
        }



        [Test]
        public void Add_YesCase_Mike_26()
        {
            var s = "Mike";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlpha, false, 26);

            Assert.AreEqual("Mikr", s2);
        }
    }
}
