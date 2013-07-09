using System.Collections.Generic;
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
        public void Add_NoCase_az_27()
        {
            var s = "az";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlpha, true, 27);

            Assert.AreEqual("ca", s2);
        }
        [Test]
        public void Add_NoCase_yz_27()
        {
            var s = "yz";
            var s2 = s.LexicalAdd(LexicalCharacterSet.AsciiAlpha, true, 27);

            Assert.AreEqual("aaa", s2);
        }
        [Test]
        public void Add_NoCase_yz_27_proof()
        {
            var list = new List<string> {" yz", "aaa"};
            list.Sort((x,y) => System.String.Compare(x, y, System.StringComparison.Ordinal));

            Assert.AreEqual(" yz", list[0]);
            Assert.AreEqual("aaa", list[1]);
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
