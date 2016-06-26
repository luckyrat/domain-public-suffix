using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using DomainPublicSuffix;

namespace DomainPublicSuffix.Tests
{
    /// <summary>
    /// Summary description for DomainTests
    /// </summary>
    [TestClass]
    public class DomainTests
    {
        private DomainName outDomain = null;

        public DomainTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            TLDRulesCache.Init(@"C:\temp\publicSuffixDomainCache.txt");
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ParseNormalDomain()
        {
            DomainName.TryParse("photos.dropbox.com", out outDomain);
            Assert.AreEqual<string>("dropbox", outDomain.Domain);
        }

        [TestMethod]
        public void ParseNormalDomain2()
        {
            DomainName.TryParse("downloads.luckyrat.co.uk", out outDomain);
            Assert.AreEqual<string>("luckyrat", outDomain.Domain);
            Assert.AreEqual<string>("co.uk", outDomain.TLD);
            Assert.AreEqual<string>("downloads", outDomain.SubDomain);
            Assert.AreEqual<string>("luckyrat.co.uk", outDomain.RegistrableDomain);
        }

        [TestMethod]
        public void ParseExceptionDomain()
        {
            DomainName.TryParse("example.city.kawasaki.jp", out outDomain);
            Assert.AreEqual<string>("example", outDomain.Domain);
        }

        [TestMethod]
        public void RepeatedLookupIsCorrectAndFaster()
        {
            DomainName outDomain2 = null;
            DomainName outDomain3 = null;
            // First request is just to be certain we have initialised the TLDRulesCache before we start the real test
            DomainName.TryParse("keefox.org", out outDomain);
            var s = Stopwatch.StartNew();
            DomainName.TryParse("tutorial.keefox.org", out outDomain2);
            var t1 = s.ElapsedTicks;
            s.Restart();
            DomainName.TryParse("tutorial.keefox.org", out outDomain3);
            var t2 = s.ElapsedTicks;
            Assert.AreEqual<string>(outDomain2.RegistrableDomain, outDomain3.RegistrableDomain);

            // Make sure it was at least a third faster 2nd time around (if not, our in memory cache is not working well)
            Assert.IsTrue(t1*0.66 > t2);
        }

        // Mixed case.
        [TestMethod]
        public void StandardTest1()
        {
            checkPublicSuffix("COM", null);
        }
        [TestMethod]
        public void StandardTest2()
        {
            checkPublicSuffix("example.COM", "example.com");
        }
        [TestMethod]
        public void StandardTest3()
        {
            checkPublicSuffix("WwW.example.COM", "example.com");
        }


        // Leading dot.
        [TestMethod]
        public void StandardTest4()
        {
            checkPublicSuffix(".com", null);
        }
        [TestMethod]
        public void StandardTest5()
        {
            checkPublicSuffix(".example", null);
        }
        [TestMethod]
        public void StandardTest6()
        {
            checkPublicSuffix(".example.com", null);
        }
        [TestMethod]
        public void StandardTest7()
        {
            checkPublicSuffix(".example.example", null);
        }

        // Listed, but non-Internet, TLD.
        [TestMethod]
        public void StandardTest8()
        {
            checkPublicSuffix("local", null);
        }
        [TestMethod]
        public void StandardTest9()
        {
            checkPublicSuffix("example.local", null);
        }
        [TestMethod]
        public void StandardTest10()
        {
            checkPublicSuffix("b.example.local", null);
        }
        [TestMethod]
        public void StandardTest11()
        {
            checkPublicSuffix("a.b.example.local", null);
        }
        // TLD with only 1 rule.

        [TestMethod]
        public void StandardTest12()
        {
            checkPublicSuffix("biz", null);
        }
        [TestMethod]
        public void StandardTest13()
        {
            checkPublicSuffix("domain.biz", "domain.biz");
        }
        [TestMethod]
        public void StandardTest14()
        {
            checkPublicSuffix("b.domain.biz", "domain.biz");
        }
        [TestMethod]
        public void StandardTest15()
        {
            checkPublicSuffix("a.b.domain.biz", "domain.biz");
        }

        // TLD with some 2-level rules.
        [TestMethod]
        public void StandardTest16()
        {
            checkPublicSuffix("com", null);
        }
        [TestMethod]
        public void StandardTest17()
        {
            checkPublicSuffix("example.com", "example.com");
        }
        [TestMethod]
        public void StandardTest18()
        {
            checkPublicSuffix("b.example.com", "example.com");
        }
        [TestMethod]
        public void StandardTest19()
        {
            checkPublicSuffix("a.b.example.com", "example.com");
        }
        [TestMethod]
        public void StandardTest20()
        {
            checkPublicSuffix("uk.com", null);
        }
        [TestMethod]
        public void StandardTest21()
        {
            checkPublicSuffix("example.uk.com", "example.uk.com");
        }
        [TestMethod]
        public void StandardTest22()
        {
            checkPublicSuffix("b.example.uk.com", "example.uk.com");
        }
        [TestMethod]
        public void StandardTest23()
        {
            checkPublicSuffix("a.b.example.uk.com", "example.uk.com");
        }
        [TestMethod]
        public void StandardTest24()
        {
            checkPublicSuffix("test.ac", "test.ac");
        }


        // TLD with only 1 (wildcard) rule.
        [TestMethod]
        public void StandardTest25()
        {
            checkPublicSuffix("bd", null);
        }
        [TestMethod]
        public void StandardTest26()
        {
            checkPublicSuffix("c.bd", null);
        }
        [TestMethod]
        public void StandardTest27()
        {
            checkPublicSuffix("b.c.bd", "b.c.bd");
        }
        [TestMethod]
        public void StandardTest28()
        {
            checkPublicSuffix("a.b.c.bd", "b.c.bd");
        }


        // More complex TLD.
        [TestMethod]
        public void StandardTest29()
        {
            checkPublicSuffix("jp", null);
        }
        [TestMethod]
        public void StandardTest30()
        {
            checkPublicSuffix("test.jp", "test.jp");
        }
        [TestMethod]
        public void StandardTest31()
        {
            checkPublicSuffix("www.test.jp", "test.jp");
        }
        [TestMethod]
        public void StandardTest32()
        {
            checkPublicSuffix("ac.jp", null);
        }
        [TestMethod]
        public void StandardTest33()
        {
            checkPublicSuffix("test.ac.jp", "test.ac.jp");
        }
        [TestMethod]
        public void StandardTest34()
        {
            checkPublicSuffix("www.test.ac.jp", "test.ac.jp");
        }
        [TestMethod]
        public void StandardTest35()
        {
            checkPublicSuffix("kyoto.jp", null);
        }
        [TestMethod]
        public void StandardTest36()
        {
            checkPublicSuffix("test.kyoto.jp", "test.kyoto.jp");
        }
        [TestMethod]
        public void StandardTest37()
        {
            checkPublicSuffix("ide.kyoto.jp", null);
        }
        [TestMethod]
        public void StandardTest38()
        {
            checkPublicSuffix("b.ide.kyoto.jp", "b.ide.kyoto.jp");
        }
        [TestMethod]
        public void StandardTest39()
        {
            checkPublicSuffix("a.b.ide.kyoto.jp", "b.ide.kyoto.jp");
        }
        [TestMethod]
        public void StandardTest40()
        {
            checkPublicSuffix("c.kobe.jp", null);
        }
        [TestMethod]
        public void StandardTest41()
        {
            checkPublicSuffix("b.c.kobe.jp", "b.c.kobe.jp");
        }
        [TestMethod]
        public void StandardTest42()
        {
            checkPublicSuffix("a.b.c.kobe.jp", "b.c.kobe.jp");
        }
        [TestMethod]
        public void StandardTest43()
        {
            checkPublicSuffix("city.kobe.jp", null);
        }
        [TestMethod]
        public void StandardTest44()
        {
            checkPublicSuffix("www.city.kobe.jp", "www.city.kobe.jp");
        }


        // TLD with a wildcard rule and exceptions.
        [TestMethod]
        public void StandardTest45()
        {
            checkPublicSuffix("ck", null);
        }
        [TestMethod]
        public void StandardTest46()
        {
            checkPublicSuffix("test.ck", null);
        }
        [TestMethod]
        public void StandardTest47()
        {
            checkPublicSuffix("b.test.ck", "b.test.ck");
        }
        [TestMethod]
        public void StandardTest48()
        {
            checkPublicSuffix("a.b.test.ck", "b.test.ck");
        }
        [TestMethod]
        public void StandardTest49()
        {
            checkPublicSuffix("www.ck", null);
        }
        [TestMethod]
        public void StandardTest50()
        {
            checkPublicSuffix("www.www.ck", "www.www.ck");
        }


        // US K12.
        [TestMethod]
        public void StandardTest51()
        {
            checkPublicSuffix("us", null);
        }
        [TestMethod]
        public void StandardTest52()
        {
            checkPublicSuffix("test.us", "test.us");
        }
        [TestMethod]
        public void StandardTest53()
        {
            checkPublicSuffix("www.test.us", "test.us");
        }
        [TestMethod]
        public void StandardTest54()
        {
            checkPublicSuffix("ak.us", null);
        }
        [TestMethod]
        public void StandardTest55()
        {
            checkPublicSuffix("test.ak.us", "test.ak.us");
        }
        [TestMethod]
        public void StandardTest56()
        {
            checkPublicSuffix("www.test.ak.us", "test.ak.us");
        }
        [TestMethod]
        public void StandardTest57()
        {
            checkPublicSuffix("k12.ak.us", null);
        }
        [TestMethod]
        public void StandardTest58()
        {
            checkPublicSuffix("test.k12.ak.us", "test.k12.ak.us");
        }
        [TestMethod]
        public void StandardTest59()
        {
            checkPublicSuffix("www.test.k12.ak.us", "test.k12.ak.us");
        }


        // IDN labels.
        [TestMethod]
        public void StandardTest60()
        {
            checkPublicSuffix("食狮.com.cn", "食狮.com.cn");
        }
        [TestMethod]
        public void StandardTest61()
        {
            checkPublicSuffix("食狮.公司.cn", "食狮.公司.cn");
        }
        [TestMethod]
        public void StandardTest62()
        {
            checkPublicSuffix("www.食狮.公司.cn", "食狮.公司.cn");
        }
        [TestMethod]
        public void StandardTest63()
        {
            checkPublicSuffix("shishi.公司.cn", "shishi.公司.cn");
        }
        [TestMethod]
        public void StandardTest64()
        {
            checkPublicSuffix("公司.cn", null);
        }
        [TestMethod]
        public void StandardTest65()
        {
            checkPublicSuffix("食狮.中国", "食狮.中国");
        }
        [TestMethod]
        public void StandardTest66()
        {
            checkPublicSuffix("www.食狮.中国", "食狮.中国");
        }
        [TestMethod]
        public void StandardTest67()
        {
            checkPublicSuffix("shishi.中国", "shishi.中国");
        }
        [TestMethod]
        public void StandardTest68()
        {
            checkPublicSuffix("中国", null);
        }


        // Same as above, but punycoded.
        //[TestMethod]
        //public void StandardTest69()
        //{
        //    checkPublicSuffix("xn--85x722f.com.cn", "xn--85x722f.com.cn");
        //}
        //[TestMethod]
        //public void StandardTest70()
        //{
        //    checkPublicSuffix("xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
        //}
        //[TestMethod]
        //public void StandardTest71()
        //{
        //    checkPublicSuffix("www.xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
        //}
        //[TestMethod]
        //public void StandardTest72()
        //{
        //    checkPublicSuffix("shishi.xn--55qx5d.cn", "shishi.xn--55qx5d.cn");
        //}
        //[TestMethod]
        //public void StandardTest73()
        //{
        //    checkPublicSuffix("xn--55qx5d.cn", null);
        //}
        //[TestMethod]
        //public void StandardTest74()
        //{
        //    checkPublicSuffix("xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
        //}
        //[TestMethod]
        //public void StandardTest75()
        //{
        //    checkPublicSuffix("www.xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
        //}
        //[TestMethod]
        //public void StandardTest76()
        //{
        //    checkPublicSuffix("shishi.xn--fiqs8s", "shishi.xn--fiqs8s");
        //}
        //[TestMethod]
        //public void StandardTest77()
        //{
        //    checkPublicSuffix("xn--fiqs8s", null);
        //}


        // Unlisted TLD.
        //[TestMethod]
        //public void StandardTest8()
        //{
        //    checkPublicSuffix("example", null);
        //}
        //[TestMethod]
        //public void StandardTest9()
        //{
        //    checkPublicSuffix("example.example", "example.example");
        //}
        //[TestMethod]
        //public void StandardTest10()
        //{
        //    checkPublicSuffix("b.example.example", "example.example");
        //}
        //[TestMethod]
        //public void StandardTest11()
        //{
        //    checkPublicSuffix("a.b.example.example", "example.example");
        //}



        private void checkPublicSuffix(string p1, string p2)
        {
            DomainName.TryParse(p1, out outDomain);
            Assert.AreEqual<string>(p2, outDomain != null ? outDomain.RegistrableDomain : null);
        }

    }
}
