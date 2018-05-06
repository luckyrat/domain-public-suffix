using NUnit.Framework;
using System.Diagnostics;

namespace DomainPublicSuffix.Tests
{
	/// <summary>
	/// Summary description for DomainTests
	/// </summary>
	[TestFixture]
    public class DomainTests
    {
        private DomainName outDomain = null;

		[OneTimeSetUp]
        public static void MyClassInitialize()
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

        [Test]
        public void ParseNormalDomain()
        {
            DomainName.TryParse("photos.dropbox.com", out outDomain);
            Assert.AreEqual("dropbox", outDomain.Domain);
        }

        [Test]
        public void ParseNormalDomain2()
        {
            DomainName.TryParse("downloads.luckyrat.co.uk", out outDomain);
            Assert.AreEqual("luckyrat", outDomain.Domain);
            Assert.AreEqual("co.uk", outDomain.TLD);
            Assert.AreEqual("downloads", outDomain.SubDomain);
            Assert.AreEqual("luckyrat.co.uk", outDomain.RegistrableDomain);
        }

        [Test]
        public void ParseExceptionDomain()
        {
            DomainName.TryParse("example.city.kawasaki.jp", out outDomain);
            Assert.AreEqual("city", outDomain.Domain);
            Assert.AreEqual("example", outDomain.SubDomain);
            Assert.AreEqual("city.kawasaki.jp", outDomain.RegistrableDomain);
        }

        [Test]
        public void ParseExceptionDomainWhereTLDOccursInSubdomain()
        {
            //  Try parsing an 'exception' domain where the TLD part also occurs in the subdomain part
            DomainName.TryParse("www.ck.www.ck", out outDomain);

            //  The domain should be parsed as 'www'
            Assert.AreEqual("www", outDomain.Domain);
        }

        [Test]
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
            Assert.AreEqual(outDomain2.RegistrableDomain, outDomain3.RegistrableDomain);

            // Make sure it was at least a third faster 2nd time around (if not, our in memory cache is not working well)
            Assert.IsTrue(t1*0.66 > t2);
        }

        [Test]
        public void RepeatedNormalLookupIsCorrect()
        {
            DomainName outDomain2 = null;
            DomainName outDomain3 = null;
            DomainName.TryParse("www.kee.pm", out outDomain);
            DomainName.TryParse("www.kee.pm", out outDomain2);
            DomainName.TryParse("www.kee.pm", out outDomain3);
            Assert.AreEqual(outDomain2.RegistrableDomain, outDomain.RegistrableDomain);
            Assert.AreEqual(outDomain2.RegistrableDomain, outDomain3.RegistrableDomain);
        }

        [Test]
        public void RepeatedPrivateLookupIsCorrect()
        {
            DomainName outDomain2 = null;
            DomainName outDomain3 = null;
            DomainName.TryParse("www.kee.notatld", out outDomain);
            DomainName.TryParse("www.kee.notatld", out outDomain2);
            DomainName.TryParse("www.kee.notatld", out outDomain3);
            Assert.AreEqual(outDomain2.RegistrableDomain, outDomain.RegistrableDomain);
            Assert.AreEqual(outDomain2.RegistrableDomain, outDomain3.RegistrableDomain);
        }

        [Test]
        public void ParseNormalDomainWhereTLDOccursInDomain()
        {
            //  Try parsing a 'normal' domain where the TLD part also occurs in the domain part
            DomainName.TryParse("russian.cntv.cn", out outDomain);

            //  The domain should be parsed as 'cntv'
            Assert.AreEqual("cntv", outDomain.Domain);
        }

        [Test]
        public void ParseWildcardDomain()
        {
            //  Try parsing a 'wildcard' domain
            DomainName.TryParse("photos.verybritish.co.uk", out outDomain);

            //  The domain should be parsed as 'verybritish'
            Assert.AreEqual("verybritish", outDomain.Domain);

            //  The TLD is 'co.uk'
            Assert.AreEqual("co.uk", outDomain.TLD);
            
            //  The subdomain is everything else to the left of the domain:
            Assert.AreEqual("photos", outDomain.SubDomain);
        }

        [Test]
        public void ParseWildcardDomainWhereTLDOccursInDomain()
        {
            //  Try parsing a 'wildcard' domain where the TLD part also occurs in the domain part
            DomainName.TryParse("com.er.com.er", out outDomain);

            //  The domain should be parsed as 'er'
            Assert.AreEqual("er", outDomain.Domain);
        }

        [Test]
        public void ParsePrivateWildcardDomain()
        {
            DomainName.TryParse("my.fun.test.compute.amazonaws.com.cn", out outDomain);

            Assert.AreEqual("fun", outDomain.Domain);
            Assert.AreEqual("test.compute.amazonaws.com.cn", outDomain.TLD);
            Assert.AreEqual("my", outDomain.SubDomain);
        }

        [Test]
        public void ParsePrivateDomain()
        {
            DomainName.TryParse("my.fun.test.us-east-1.amazonaws.com", out outDomain);

            Assert.AreEqual("test", outDomain.Domain);
            Assert.AreEqual("us-east-1.amazonaws.com", outDomain.TLD);
            Assert.AreEqual("my.fun", outDomain.SubDomain);
        }
        
        // Mixed case.
        [Test]
        public void StandardTest1()
        {
            checkRegistrableDomain("COM", null);
        }
        [Test]
        public void StandardTest2()
        {
            checkRegistrableDomain("example.COM", "example.com");
        }
        [Test]
        public void StandardTest3()
        {
            checkRegistrableDomain("WwW.example.COM", "example.com");
        }


        // Leading dot.
        [Test]
        public void StandardTest4()
        {
            checkRegistrableDomain(".com", null);
        }
        [Test]
        public void StandardTest5()
        {
            checkRegistrableDomain(".example", null);
        }
        [Test]
        public void StandardTest6()
        {
            checkRegistrableDomain(".example.com", null);
        }
        [Test]
        public void StandardTest7()
        {
            checkRegistrableDomain(".example.example", null);
        }

        // Listed, but non-Internet, TLD.
        [Test]
        public void StandardTest8()
        {
            checkRegistrableDomain("local", null);
        }
        [Test]
        public void StandardTest9()
        {
            checkRegistrableDomain("example.local", null);
        }
        [Test]
        public void StandardTest10()
        {
            checkRegistrableDomain("b.example.local", null);
        }
        [Test]
        public void StandardTest11()
        {
            checkRegistrableDomain("a.b.example.local", null);
        }
        // TLD with only 1 rule.

        [Test]
        public void StandardTest12()
        {
            checkRegistrableDomain("biz", null);
        }
        [Test]
        public void StandardTest13()
        {
            checkRegistrableDomain("domain.biz", "domain.biz");
        }
        [Test]
        public void StandardTest14()
        {
            checkRegistrableDomain("b.domain.biz", "domain.biz");
        }
        [Test]
        public void StandardTest15()
        {
            checkRegistrableDomain("a.b.domain.biz", "domain.biz");
        }

        // TLD with some 2-level rules.
        [Test]
        public void StandardTest16()
        {
            checkRegistrableDomain("com", null);
        }
        [Test]
        public void StandardTest17()
        {
            checkRegistrableDomain("example.com", "example.com");
        }
        [Test]
        public void StandardTest18()
        {
            checkRegistrableDomain("b.example.com", "example.com");
        }
        [Test]
        public void StandardTest19()
        {
            checkRegistrableDomain("a.b.example.com", "example.com");
        }
        [Test]
        public void StandardTest20()
        {
            checkRegistrableDomain("uk.com", null);
        }
        [Test]
        public void StandardTest21()
        {
            checkRegistrableDomain("example.uk.com", "example.uk.com");
        }
        [Test]
        public void StandardTest22()
        {
            checkRegistrableDomain("b.example.uk.com", "example.uk.com");
        }
        [Test]
        public void StandardTest23()
        {
            checkRegistrableDomain("a.b.example.uk.com", "example.uk.com");
        }
        [Test]
        public void StandardTest24()
        {
            checkRegistrableDomain("test.ac", "test.ac");
        }


        // TLD with only 1 (wildcard) rule.
        [Test]
        public void StandardTest25()
        {
            checkRegistrableDomain("mm", null);
        }
        [Test]
        public void StandardTest26()
        {
            checkRegistrableDomain("c.mm", null);
        }
        [Test]
        public void StandardTest27()
        {
            checkRegistrableDomain("b.c.mm", "b.c.mm");
        }
        [Test]
        public void StandardTest28()
        {
            checkRegistrableDomain("a.b.c.mm", "b.c.mm");
        }


        // More complex TLD.
        [Test]
        public void StandardTest29()
        {
            checkRegistrableDomain("jp", null);
        }
        [Test]
        public void StandardTest30()
        {
            checkRegistrableDomain("test.jp", "test.jp");
        }
        [Test]
        public void StandardTest31()
        {
            checkRegistrableDomain("www.test.jp", "test.jp");
        }
        [Test]
        public void StandardTest32()
        {
            checkRegistrableDomain("ac.jp", null);
        }
        [Test]
        public void StandardTest33()
        {
            checkRegistrableDomain("test.ac.jp", "test.ac.jp");
        }
        [Test]
        public void StandardTest34()
        {
            checkRegistrableDomain("www.test.ac.jp", "test.ac.jp");
        }
        [Test]
        public void StandardTest35()
        {
            checkRegistrableDomain("kyoto.jp", null);
        }
        [Test]
        public void StandardTest36()
        {
            checkRegistrableDomain("test.kyoto.jp", "test.kyoto.jp");
        }
        [Test]
        public void StandardTest37()
        {
            checkRegistrableDomain("ide.kyoto.jp", null);
        }
        [Test]
        public void StandardTest38()
        {
            checkRegistrableDomain("b.ide.kyoto.jp", "b.ide.kyoto.jp");
        }
        [Test]
        public void StandardTest39()
        {
            checkRegistrableDomain("a.b.ide.kyoto.jp", "b.ide.kyoto.jp");
        }
        [Test]
        public void StandardTest40()
        {
            checkRegistrableDomain("c.kobe.jp", null);
        }
        [Test]
        public void StandardTest41()
        {
            checkRegistrableDomain("b.c.kobe.jp", "b.c.kobe.jp");
        }
        [Test]
        public void StandardTest42()
        {
            checkRegistrableDomain("a.b.c.kobe.jp", "b.c.kobe.jp");
        }
        [Test]
        public void StandardTest43()
        {
            checkRegistrableDomain("city.kobe.jp", "city.kobe.jp");
        }
        [Test]
        public void StandardTest44()
        {
            checkRegistrableDomain("www.city.kobe.jp", "city.kobe.jp");
        }


        // TLD with a wildcard rule and exceptions.
        [Test]
        public void StandardTest45()
        {
            checkRegistrableDomain("ck", null);
        }
        [Test]
        public void StandardTest46()
        {
            checkRegistrableDomain("test.ck", null);
        }
        [Test]
        public void StandardTest47()
        {
            checkRegistrableDomain("b.test.ck", "b.test.ck");
        }
        [Test]
        public void StandardTest48()
        {
            checkRegistrableDomain("a.b.test.ck", "b.test.ck");
        }
        [Test]
        public void StandardTest49()
        {
            checkRegistrableDomain("www.ck", "www.ck");
        }
        [Test]
        public void StandardTest50()
        {
            checkRegistrableDomain("www.www.ck", "www.ck");
        }

        // US K12.
        [Test]
        public void StandardTest51()
        {
            checkRegistrableDomain("us", null);
        }
        [Test]
        public void StandardTest52()
        {
            checkRegistrableDomain("test.us", "test.us");
        }
        [Test]
        public void StandardTest53()
        {
            checkRegistrableDomain("www.test.us", "test.us");
        }
        [Test]
        public void StandardTest54()
        {
            checkRegistrableDomain("ak.us", null);
        }
        [Test]
        public void StandardTest55()
        {
            checkRegistrableDomain("test.ak.us", "test.ak.us");
        }
        [Test]
        public void StandardTest56()
        {
            checkRegistrableDomain("www.test.ak.us", "test.ak.us");
        }
        [Test]
        public void StandardTest57()
        {
            checkRegistrableDomain("k12.ak.us", null);
        }
        [Test]
        public void StandardTest58()
        {
            checkRegistrableDomain("test.k12.ak.us", "test.k12.ak.us");
        }
        [Test]
        public void StandardTest59()
        {
            checkRegistrableDomain("www.test.k12.ak.us", "test.k12.ak.us");
        }


        // IDN labels.
        [Test]
        public void StandardTest60()
        {
            checkRegistrableDomain("食狮.com.cn", "食狮.com.cn");
        }
        [Test]
        public void StandardTest61()
        {
            checkRegistrableDomain("食狮.公司.cn", "食狮.公司.cn");
        }
        [Test]
        public void StandardTest62()
        {
            checkRegistrableDomain("www.食狮.公司.cn", "食狮.公司.cn");
        }
        [Test]
        public void StandardTest63()
        {
            checkRegistrableDomain("shishi.公司.cn", "shishi.公司.cn");
        }
        [Test]
        public void StandardTest64()
        {
            checkRegistrableDomain("公司.cn", null);
        }
        [Test]
        public void StandardTest65()
        {
            checkRegistrableDomain("食狮.中国", "食狮.中国");
        }
        [Test]
        public void StandardTest66()
        {
            checkRegistrableDomain("www.食狮.中国", "食狮.中国");
        }
        [Test]
        public void StandardTest67()
        {
            checkRegistrableDomain("shishi.中国", "shishi.中国");
        }
        [Test]
        public void StandardTest68()
        {
            checkRegistrableDomain("中国", null);
        }


        // Same as above, but punycoded.
        [Test]
        public void StandardTest69()
        {
            checkRegistrableDomain("xn--85x722f.com.cn", "xn--85x722f.com.cn");
        }
        [Test]
        public void StandardTest70()
        {
            checkRegistrableDomain("xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
        }
        [Test]
        public void StandardTest71()
        {
            checkRegistrableDomain("www.xn--85x722f.xn--55qx5d.cn", "xn--85x722f.xn--55qx5d.cn");
        }
        [Test]
        public void StandardTest72()
        {
            checkRegistrableDomain("shishi.xn--55qx5d.cn", "shishi.xn--55qx5d.cn");
        }
        [Test]
        public void StandardTest73()
        {
            checkRegistrableDomain("xn--55qx5d.cn", null);
        }
        [Test]
        public void StandardTest74()
        {
            checkRegistrableDomain("xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
        }
        [Test]
        public void StandardTest75()
        {
            checkRegistrableDomain("www.xn--85x722f.xn--fiqs8s", "xn--85x722f.xn--fiqs8s");
        }
        [Test]
        public void StandardTest76()
        {
            checkRegistrableDomain("shishi.xn--fiqs8s", "shishi.xn--fiqs8s");
        }
        [Test]
        public void StandardTest77()
        {
            checkRegistrableDomain("xn--fiqs8s", null);
        }


        // Unlisted TLD.
        [Test]
        public void StandardTest78()
        {
            checkRegistrableDomain("example", null);
        }
        [Test]
        public void StandardTest79()
        {
            checkRegistrableDomain("example.example", "example.example");
        }
        [Test]
        public void StandardTest80()
        {
            checkRegistrableDomain("b.example.example", "example.example");
        }
        [Test]
        public void StandardTest81()
        {
            checkRegistrableDomain("a.b.example.example", "example.example");
        }

        private void checkRegistrableDomain(string p1, string p2)
        {
            DomainName.TryParse(p1, out outDomain);
            Assert.AreEqual(p2, outDomain != null ? outDomain.RegistrableDomain : null);
        }

    }
}
