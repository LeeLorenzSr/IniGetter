using NUnit.Framework;

namespace IniGetter.Tests
{
    public class Tests
    {
        [Test]
        public void AlternativeKeyValuePairDelimiterTest()
        {
            IniFile iniTest = new IniFile("Data\\ColonDelimiter.ini", new IniOptions() { NameValueDelimiter = ':' });

            Assert.AreEqual("FirstValue", iniTest.Get("FirstSection", "FirstKey", "-"));
            Assert.IsTrue(iniTest.Get("FirstSection", "SecondKey", false));
        }

        [Test]
        public void CreationAndWriteTests()
        {
            IniFile iniTest = new IniFile();

            iniTest.Set("SectionAlpha", "FirstKey", "FirstValue");

            Assert.AreEqual("FirstValue", iniTest.Get("SectionAlpha", "FirstKey", "-"));
        }

        [Test]
        public void GarbageInTest()
        {
            IniFile iniTest = new IniFile("Data\\Garbage.ini");

            Assert.NotZero(iniTest.ParseWarnings.Length);
            Assert.Zero(iniTest.GetSectionNames().Length);
        }

        [Test]
        public void MergeWithPrefixTest()
        {
            IniFile iniTest = new IniFile("Data\\FirstMerge.ini", null, "FirstFile.");
            iniTest.Load("Data\\SecondMerge.ini", true, "SecondFile.");

            Assert.Zero(iniTest.ParseWarnings.Length);
            Assert.AreEqual("FirstValue", iniTest.Get("FirstFile.FirstSection", "FirstKey", "-"));
            Assert.AreEqual("SecondFileFirstValue", iniTest.Get("SecondFile.FirstSection", "FirstKey", "-"));
        }

        [Test]
        public void MultiLineLoadAndCheckTests()
        {
            IniFile iniTest = new IniFile("Data\\MultiLine.ini", new IniOptions() { MultilineSupport = true });
            Assert.AreEqual("This is a multiline value!", iniTest.Get("FirstSection", "FirstValue", "-"));
            Assert.AreEqual("Checking for EOL", iniTest.Get("FirstSection", "SecondValue", "-"));
        }

        [Test]
        public void ReplicationTest()
        {
            IniFile iniTest = new IniFile("Data\\TestIni01.ini", new IniOptions() { MultilineSupport = true });

            string sGetFirstIni = iniTest.ToString();
            IniFile iniDuplicate = new IniFile();
            iniDuplicate.LoadFromContent(sGetFirstIni);
            Assert.IsTrue(iniDuplicate.Get("first section", "FirstSectionBoolean01", false));
            Assert.AreEqual(sGetFirstIni, iniDuplicate.ToString());
        }

        [Test]
        public void PlusOperatorTest()
        {
            IniFile iniTestA = new IniFile("Data\\FirstMerge.ini");
            IniFile iniTestB = new IniFile("Data\\MergeColon.ini", new IniOptions() { NameValueDelimiter = ':' });

            var iniResult = iniTestA + iniTestB;

            Assert.AreEqual("FirstValue", iniResult.Get("firstsection", "FirstKey", ""));
            Assert.IsTrue(iniResult.Get("firstsection", "secondkey", false));
            Assert.AreEqual(10.123, iniResult.Get("firstsection", "ThirdKey", 1.2));
            Assert.AreEqual("ColonValue", iniResult.Get("firstsection", "FourthKey", ""));
            Assert.IsTrue( iniResult.Get("FirstSection", "FifthKey", false));
        }

        [Test]
        public void PlusEqualOperatorTest()
        {
            IniFile iniTestA = new IniFile("Data\\FirstMerge.ini");
            IniFile iniTestB = new IniFile("Data\\MergeColon.ini", new IniOptions() { NameValueDelimiter = ':' });

            iniTestA += iniTestB;

            Assert.AreEqual("FirstValue", iniTestA.Get("firstsection", "FirstKey", ""));
            Assert.IsTrue(iniTestA.Get("firstsection", "secondkey", false));
            Assert.AreEqual(10.123, iniTestA.Get("firstsection", "ThirdKey", 1.2));
            Assert.AreEqual("ColonValue", iniTestA.Get("firstsection", "FourthKey", ""));
            Assert.IsTrue(iniTestA.Get("FirstSection", "FifthKey", false));
        }

        [SetUp]
        public void Setup()
        {
        }
    }
}