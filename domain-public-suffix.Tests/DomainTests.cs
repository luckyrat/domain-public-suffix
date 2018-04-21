﻿using System;
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
            Assert.AreEqual<string>("city", outDomain.Domain);
            Assert.AreEqual<string>("example", outDomain.SubDomain);
            Assert.AreEqual<string>("city.kawasaki.jp", outDomain.RegistrableDomain);
        }

        [TestMethod]
        public void ParseExceptionDomainWhereTLDOccursInSubdomain()
        {
            //  Try parsing an 'exception' domain where the TLD part also occurs in the subdomain part
            DomainName.TryParse("www.ck.www.ck", out outDomain);

            //  The domain should be parsed as 'www'
            Assert.AreEqual<string>("www", outDomain.Domain);
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

        [TestMethod]
        public void RepeatedNormalLookupIsCorrect()
        {
            DomainName outDomain2 = null;
            DomainName outDomain3 = null;
            DomainName.TryParse("www.kee.pm", out outDomain);
            DomainName.TryParse("www.kee.pm", out outDomain2);
            DomainName.TryParse("www.kee.pm", out outDomain3);
            Assert.AreEqual<string>(outDomain2.RegistrableDomain, outDomain.RegistrableDomain);
            Assert.AreEqual<string>(outDomain2.RegistrableDomain, outDomain3.RegistrableDomain);
        }

        [TestMethod]
        public void RepeatedPrivateLookupIsCorrect()
        {
            DomainName outDomain2 = null;
            DomainName outDomain3 = null;
            DomainName.TryParse("www.kee.notatld", out outDomain);
            DomainName.TryParse("www.kee.notatld", out outDomain2);
            DomainName.TryParse("www.kee.notatld", out outDomain3);
            Assert.AreEqual<string>(outDomain2.RegistrableDomain, outDomain.RegistrableDomain);
            Assert.AreEqual<string>(outDomain2.RegistrableDomain, outDomain3.RegistrableDomain);
        }

        [TestMethod]
        public void ParseNormalDomainWhereTLDOccursInDomain()
        {
            //  Try parsing a 'normal' domain where the TLD part also occurs in the domain part
            DomainName.TryParse("russian.cntv.cn", out outDomain);

            //  The domain should be parsed as 'cntv'
            Assert.AreEqual<string>("cntv", outDomain.Domain);
        }

        [TestMethod]
        public void ParseWildcardDomain()
        {
            //  Try parsing a 'wildcard' domain
            DomainName.TryParse("photos.verybritish.co.uk", out outDomain);

            //  The domain should be parsed as 'verybritish'
            Assert.AreEqual<string>("verybritish", outDomain.Domain);

            //  The TLD is 'co.uk'
            Assert.AreEqual<string>("co.uk", outDomain.TLD);
            
            //  The subdomain is everything else to the left of the domain:
            Assert.AreEqual<string>("photos", outDomain.SubDomain);
        }

        [TestMethod]
        public void ParseWildcardDomainWhereTLDOccursInDomain()
        {
            //  Try parsing a 'wildcard' domain where the TLD part also occurs in the domain part
            DomainName.TryParse("com.er.com.er", out outDomain);

            //  The domain should be parsed as 'er'
            Assert.AreEqual<string>("er", outDomain.Domain);
        }

        [TestMethod]
        public void ParsePrivateWildcardDomain()
        {
            DomainName.TryParse("my.fun.test.compute.amazonaws.com.cn", out outDomain);

            Assert.AreEqual<string>("fun", outDomain.Domain);
            Assert.AreEqual<string>("test.compute.amazonaws.com.cn", outDomain.TLD);
            Assert.AreEqual<string>("my", outDomain.SubDomain);
        }

        [TestMethod]
        public void ParsePrivateDomain()
        {
            DomainName.TryParse("my.fun.test.us-east-1.amazonaws.com", out outDomain);

            Assert.AreEqual<string>("test", outDomain.Domain);
            Assert.AreEqual<string>("us-east-1.amazonaws.com", outDomain.TLD);
            Assert.AreEqual<string>("my.fun", outDomain.SubDomain);
        }
        
        // Mixed case.
        [TestMethod]
        public void StandardTest1()
        {
            checkRegistrableDomain("COM", null);
        }
        [TestMethod]
        public void StandardTest2()
        {
            checkRegistrableDomain("example.COM", "example.com");
        }
        [TestMethod]
        public void StandardTest3()
        {
            checkRegistrableDomain("WwW.example.COM", "example.com");
        }


        // Leading dot.
        [TestMethod]
        public void StandardTest4()
        {
            checkRegistrableDomain(".com", null);
        }
        [TestMethod]
        public void StandardTest5()
        {
            checkRegistrableDomain(".example", null);
        }
        [TestMethod]
        public void StandardTest6()
        {
            checkRegistrableDomain(".example.com", null);
        }
        [TestMethod]
        public void StandardTest7()
        {
            checkRegistrableDomain(".example.example", null);
        }

        // Listed, but non-Internet, TLD.
        [TestMethod]
        public void StandardTest8()
        {
            checkRegistrableDomain("local", null);
        }
        [TestMethod]
        public void StandardTest9()
        {
            checkRegistrableDomain("example.local", null);
        }
        [TestMethod]
        public void StandardTest10()
        {
            checkRegistrableDomain("b.example.local", null);
        }
        [TestMethod]
        public void StandardTest11()
        {
            checkRegistrableDomain("a.b.example.local", null);
        }
        // TLD with only 1 rule.

        [TestMethod]
        public void StandardTest12()
        {
            checkRegistrableDomain("biz", null);
        }
        [TestMethod]
        public void StandardTest13()
        {
            checkRegistrableDomain("domain.biz", "domain.biz");
        }
        [TestMethod]
        public void StandardTest14()
        {
            checkRegistrableDomain("b.domain.biz", "domain.biz");
        }
        [TestMethod]
        public void StandardTest15()
        {
            checkRegistrableDomain("a.b.domain.biz", "domain.biz");
        }

        // TLD with some 2-level rules.
        [TestMethod]
        public void StandardTest16()
        {
            checkRegistrableDomain("com", null);
        }
        [TestMethod]
        public void StandardTest17()
        {
            checkRegistrableDomain("example.com", "example.com");
        }
        [TestMethod]
        public void StandardTest18()
        {
            checkRegistrableDomain("b.example.com", "example.com");
        }
        [TestMethod]
        public void StandardTest19()
        {
            checkRegistrableDomain("a.b.example.com", "example.com");
        }
        [TestMethod]
        public void StandardTest20()
        {
            checkRegistrableDomain("uk.com", null);
        }
        [TestMethod]
        public void StandardTest21()
        {
            checkRegistrableDomain("example.uk.com", "example.uk.com");
        }
        [TestMethod]
        public void StandardTest22()
        {
            checkRegistrableDomain("b.example.uk.com", "example.uk.com");
        }
        [TestMethod]
        public void StandardTest23()
        {
            checkRegistrableDomain("a.b.example.uk.com", "example.uk.com");
        }
        [TestMethod]
        public void StandardTest24()
        {
            checkRegistrableDomain("test.ac", "test.ac");
        }


        // TLD with only 1 (wildcard) rule.
        [TestMethod]
        public void StandardTest25()
        {
            checkRegistrableDomain("mm", null);
        }
        [TestMethod]
        public void StandardTest26()
        {
            checkRegistrableDomain("c.mm", null);
        }
        [TestMethod]
        public void StandardTest27()
        {
            checkRegistrableDomain("b.c.mm", "b.c.mm");
        }
        [TestMethod]
        public void StandardTest28()
        {
            checkRegistrableDomain("a.b.c.mm", "b.c.mm");
        }


        // More complex TLD.
        [TestMethod]
        public void StandardTest29()
        {
            checkRegistrableDomain("jp", null);
        }
        [TestMethod]
        public void StandardTest30()
        {
            checkRegistrableDomain("test.jp", "test.jp");
        }
        [TestMethod]
        public void StandardTest31()
        {
            checkRegistrableDomain("www.test.jp", "test.jp");
        }
        [TestMethod]
        public void StandardTest32()
        {
            checkRegistrableDomain("ac.jp", null);
        }
        [TestMethod]
        public void StandardTest33()
        {
            checkRegistrableDomain("test.ac.jp", "test.ac.jp");
        }
        [TestMethod]
        public void StandardTest34()
        {
            checkRegistrableDomain("www.test.ac.jp", "test.ac.jp");
        }
        [TestMethod]
        public void StandardTest35()
        {
            checkRegistrableDomain("kyoto.jp", null);
        }
        [TestMethod]
        public void StandardTest36()
        {
            checkRegistrableDomain("test.kyoto.jp", "test.kyoto.jp");
        }
        [TestMethod]
        public void StandardTest37()
        {
            checkRegistrableDomain("ide.kyoto.jp", null);
        }
        [TestMethod]
        public void StandardTest38()
        {
            checkRegistrableDomain("b.ide.kyoto.jp", "b.ide.kyoto.jp");
        }
        [TestMethod]
        public void StandardTest39()
        {
            checkRegistrableDomain("a.b.ide.kyoto.jp", "b.ide.kyoto.jp");
        }
        [TestMethod]
        public void StandardTest40()
        {
            checkRegistrableDomain("c.kobe.jp", null);
        }
        [TestMethod]
        public void StandardTest41()
        {
            checkRegistrableDomain("b.c.kobe.jp", "b.c.kobe.jp");
        }
        [TestMethod]
        public void StandardTest42()
        {
            checkRegistrableDomain("a.b.c.kobe.jp", "b.c.kobe.jp");
        }
        [TestMethod]
        public void StandardTest43()
        {
            checkRegistrableDomain("city.kobe.jp", "city.kobe.jp");
        }
        [TestMethod]
        public void StandardTest44()
        {
            checkRegistrableDomain("www.city.kobe.jp", "city.kobe.jp");
        }


        // TLD with a wildcard rule and exceptions.
        [TestMethod]
        public void StandardTest45()
        {
            checkRegistrableDomain("ck", null);
        }
        [TestMethod]
        public void StandardTest46()
        {
            checkRegistrableDomain("test.ck", null);
        }
        [TestMethod]
        public void StandardTest47()
        {
            checkRegistrableDomain("b.test.ck", "b.test.ck");
        }
        [TestMethod]
        public void StandardTest48()
        {
            checkRegistrableDomain("a.b.test.ck", "b.test.ck");
        }
        [TestMethod]
        public void StandardTest49()
        {
            checkRegistrableDomain("www.ck", "www.ck");
        }
        [TestMethod]
        public void StandardTest50()
        {
            checkRegistrableDomain("www.www.ck", "www.ck");
        }

        // US K12.
        [TestMethod]
        public void StandardTest51()
        {
            checkRegistrableDomain("us", null);
        }
        [TestMethod]
        public void StandardTest52()
        {
            checkRegistrableDomain("test.us", "test.us");
        }
        [TestMethod]
        public void StandardTest53()
        {
            checkRegistrableDomain("www.test.us", "test.us");
        }
        [TestMethod]
        public void StandardTest54()
        {
            checkRegistrableDomain("ak.us", null);
        }
        [TestMethod]
        public void StandardTest55()
        {
            checkRegistrableDomain("test.ak.us", "test.ak.us");
        }
        [TestMethod]
        public void StandardTest56()
        {
            checkRegistrableDomain("www.test.ak.us", "test.ak.us");
        }
        [TestMethod]
        public void StandardTest57()
        {
            checkRegistrableDomain("k12.ak.us", null);
        }
        [TestMethod]
        public void StandardTest58()
        {
            checkRegistrableDomain("test.k12.ak.us", "test.k12.ak.us");
        }
        [TestMethod]
        public void StandardTest59()
        {
            checkRegistrableDomain("www.test.k12.ak.us", "test.k12.ak.us");
        }


        // IDN labels.
        [TestMethod]
        public void StandardTest60()
        {
            checkRegistrableDomain("食狮.com.cn", "食狮.com.cn");
        }
        [TestMethod]
        public void StandardTest61()
        {
            checkRegistrableDomain("食狮.公司.cn", "食狮.公司.cn");
        }
        [TestMethod]
        public void StandardTest62()
        {
            checkRegistrableDomain("www.食狮.公司.cn", "食狮.公司.cn");
        }
        [TestMethod]
        public void StandardTest63()
        {
            checkRegistrableDomain("shishi.公司.cn", "shishi.公司.cn");
        }
        [TestMethod]
        public void StandardTest64()
        {
            checkRegistrableDomain("公司.cn", null);
        }
        [TestMethod]
        public void StandardTest65()
        {
            checkRegistrableDomain("食狮.中国", "食狮.中国");
        }
        [TestMethod]
        public void StandardTest66()
        {
            checkRegistrableDomain("www.食狮.中国", "食狮.中国");
        }
        [TestMethod]
        public void StandardTest67()
        {
            checkRegistrableDomain("shishi.中国", "shishi.中国");
        }
        [TestMethod]
        public void StandardTest68()
        {
            checkRegistrableDomain("中国", null);
        }


        // Same as above, but punycoded.
        [TestMethod]
        public void StandardTest69()
        {
            checkRegistrableDomain("xn--85x722f.com.cn", "xn--85x722f.com.cn");
        }
        [TestMethod]
        public void StandardTest70()
        {
            checkRegistrableDomain("xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
        }
        [TestMethod]
        public void StandardTest71()
        {
            checkRegistrableDomain("www.xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
        }
        [TestMethod]
        public void StandardTest72()
        {
            checkRegistrableDomain("shishi.xn--55qx5d.cn", "shishi.xn--55qx5d.cn");
        }
        [TestMethod]
        public void StandardTest73()
        {
            checkRegistrableDomain("xn--55qx5d.cn", null);
        }
        [TestMethod]
        public void StandardTest74()
        {
            checkRegistrableDomain("xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
        }
        [TestMethod]
        public void StandardTest75()
        {
            checkRegistrableDomain("www.xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
        }
        [TestMethod]
        public void StandardTest76()
        {
            checkRegistrableDomain("shishi.xn--fiqs8s", "shishi.xn--fiqs8s");
        }
        [TestMethod]
        public void StandardTest77()
        {
            checkRegistrableDomain("xn--fiqs8s", null);
        }


        // Unlisted TLD.
        [TestMethod]
        public void StandardTest78()
        {
            checkRegistrableDomain("example", null);
        }
        [TestMethod]
        public void StandardTest79()
        {
            checkRegistrableDomain("example.example", "example.example");
        }
        [TestMethod]
        public void StandardTest80()
        {
            checkRegistrableDomain("b.example.example", "example.example");
        }
        [TestMethod]
        public void StandardTest81()
        {
            checkRegistrableDomain("a.b.example.example", "example.example");
        }

        private void checkRegistrableDomain(string p1, string p2)
        {
            DomainName.TryParse(p1, out outDomain);
            Assert.AreEqual<string>(p2, outDomain != null ? outDomain.RegistrableDomain : null);
        }

    }
}
