using System;
using System.IO;
using NUnit.Framework;

namespace IniGetter.Tests
{
    public class Tests
    {
        [Test]
        public void AlternativeKeyValuePairDelimiterTest()
        {
            var iniTest = new IniFile("Data\\ColonDelimiter.ini", new IniOptions { NameValueDelimiter = ':' });

            Assert.AreEqual("FirstValue", iniTest.Get("FirstSection", "FirstKey", "-"));
            Assert.IsTrue(iniTest.Get("FirstSection", "SecondKey", false));
        }

        [Test]
        public void CreationAndWriteTests()
        {
            var iniTest = new IniFile();

            Assert.IsFalse(iniTest.Set("SectionAlpha", "FirstKey", "FirstValue"));
            Assert.IsTrue(iniTest.Set("SectionAlpha", "FirstKey", "UpdatedValue"));

            Assert.AreEqual("UpdatedValue", iniTest.Get("SectionAlpha", "FirstKey", "-"));
        }

        [Test]
        public void GarbageInTest()
        {
            var iniTest = new IniFile("Data\\Garbage.ini");

            Assert.NotZero(iniTest.ParseWarnings.Length);
            Assert.Zero(iniTest.GetSectionNames().Length);
        }

        [Test]
        public void GetCommentReturnsStoredComment()
        {
            var iniTest = new IniFile();

            iniTest.Set("SectionAlpha", "FirstKey", "FirstValue", "This is a comment");

            Assert.AreEqual("This is a comment", iniTest.GetComment("SectionAlpha", "FirstKey"));
        }

        [Test]
        public void MergeWithPrefixTest()
        {
            var iniTest = new IniFile("Data\\FirstMerge.ini", null, "FirstFile.");
            iniTest.Load("Data\\SecondMerge.ini", true, "SecondFile.");

            Assert.Zero(iniTest.ParseWarnings.Length);
            Assert.AreEqual("FirstValue", iniTest.Get("FirstFile.FirstSection", "FirstKey", "-"));
            Assert.AreEqual("SecondFileFirstValue", iniTest.Get("SecondFile.FirstSection", "FirstKey", "-"));
        }

        [Test]
        public void MultiLineLoadAndCheckTests()
        {
            var iniTest = new IniFile("Data\\MultiLine.ini", new IniOptions { MultilineSupport = true });
            Assert.AreEqual("This is a multiline value!", iniTest.Get("FirstSection", "FirstValue", "-"));
            Assert.AreEqual("Checking for EOL", iniTest.Get("FirstSection", "SecondValue", "-"));
        }

        [Test]
        public void ReplicationTest()
        {
            var iniTest = new IniFile("Data\\TestIni01.ini", new IniOptions { MultilineSupport = true });

            var sGetFirstIni = iniTest.ToString();
            var iniDuplicate = new IniFile();
            Assert.IsTrue(iniDuplicate.LoadFromContent(sGetFirstIni));
            Assert.IsTrue(iniDuplicate.Get("first section", "FirstSectionBoolean01", false));
            Assert.AreEqual(sGetFirstIni, iniDuplicate.ToString());
        }

        [Test]
        public void LoadFromContentReturnsFalseWhenWarningsOccur()
        {
            var iniTest = new IniFile();

            Assert.IsFalse(iniTest.LoadFromContent("[Section\nKey=Value"));
            Assert.NotZero(iniTest.ParseWarnings.Length);
        }

        [Test]
        public void DuplicateKeysWithoutCommentsDoNotThrow()
        {
            var iniTest = new IniFile();

            Assert.IsFalse(iniTest.LoadFromContent("[Section]\nKey=First\nKey=Second"));
            Assert.AreEqual("Second", iniTest.Get("Section", "Key", string.Empty));
            Assert.NotZero(iniTest.ParseWarnings.Length);
        }

        [Test]
        public void PoundCommentsCanBeDisabledForInlineValues()
        {
            var iniTest = new IniFile(new IniOptions { PoundComment = false });

            Assert.IsTrue(iniTest.LoadFromContent("[Section]\nKey=value#notacomment"));
            Assert.AreEqual("value#notacomment", iniTest.Get("Section", "Key", string.Empty));
        }

        [Test]
        public void CompareToHandlesNull()
        {
            var iniTest = new IniFile();

            Assert.AreEqual(1, iniTest.CompareTo(null));
        }

        [Test]
        public void UnescapeStringUnescapesQuotedContent()
        {
            Assert.AreEqual("Line\nBreak", IniFile.UnescapeString("\"Line\\nBreak\""));
        }

        [Test]
        public void UnescapeStringReturnsOriginalWhenQuotedSubstringIsEmbedded()
        {
            Assert.AreEqual("abc\"Line\\nBreak\"def", IniFile.UnescapeString("abc\"Line\\nBreak\"def"));
        }

        [Test]
        public void LoadFromContentHandlesQuotedValuesWithInlineComments()
        {
            var iniTest = new IniFile();

            Assert.IsTrue(iniTest.LoadFromContent("[Section]\nKey=\"Line\\nBreak\";comment"));
            Assert.AreEqual("Line\nBreak", iniTest.Get("Section", "Key", string.Empty));
            Assert.AreEqual("comment", iniTest.GetComment("Section", "Key"));
        }

        [Test]
        public void QuotedValueWithInlineCommentRoundTripsWhenValueContainsCommentMarker()
        {
            var iniTest = new IniFile();

            Assert.IsTrue(iniTest.LoadFromContent("[Section]\nKey=\"semi;colon\";comment"));
            Assert.AreEqual("semi;colon", iniTest.Get("Section", "Key", string.Empty));
            Assert.AreEqual("comment", iniTest.GetComment("Section", "Key"));

            var iniDuplicate = new IniFile();

            Assert.IsTrue(iniDuplicate.LoadFromContent(iniTest.ToString()));
            Assert.AreEqual("semi;colon", iniDuplicate.Get("Section", "Key", string.Empty));
            Assert.AreEqual("comment", iniDuplicate.GetComment("Section", "Key"));
        }

        [Test]
        public void SaveHonorsOptions()
        {
            var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".ini");

            try
            {
                var saveDisabledIni = new IniFile(new IniOptions { ReadOnly = false, AllowSave = false });
                saveDisabledIni.Set("SectionAlpha", "FirstKey", "FirstValue");

                Assert.IsFalse(saveDisabledIni.Save(filePath));
                Assert.IsNotEmpty(saveDisabledIni.LastWarning);

                var writableIni = new IniFile(new IniOptions { ReadOnly = false, AllowSave = true });
                writableIni.Set("SectionAlpha", "FirstKey", "FirstValue");

                Assert.IsTrue(writableIni.Save(filePath));
                Assert.IsTrue(File.Exists(filePath));

                File.Delete(filePath);

                var readOnlyIni = new IniFile(new IniOptions { ReadOnly = true, AllowSave = true });
                readOnlyIni.Set("SectionAlpha", "FirstKey", "FirstValue");

                Assert.IsFalse(readOnlyIni.Save(filePath));
                Assert.IsNotEmpty(readOnlyIni.LastWarning);
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Test]
        public void PlusOperatorTest()
        {
            var iniTestA = new IniFile("Data\\FirstMerge.ini");
            var iniTestB = new IniFile("Data\\MergeColon.ini", new IniOptions { NameValueDelimiter = ':' });

            var iniResult = iniTestA + iniTestB;

            Assert.AreEqual("FirstValue", iniResult.Get("firstsection", "FirstKey", ""));
            Assert.IsTrue(iniResult.Get("firstsection", "secondkey", false));
            Assert.AreEqual(10.123, iniResult.Get("firstsection", "ThirdKey", 1.2));
            Assert.AreEqual("ColonValue", iniResult.Get("firstsection", "FourthKey", ""));
            Assert.IsTrue(iniResult.Get("FirstSection", "FifthKey", false));
        }

        [Test]
        public void PlusEqualOperatorTest()
        {
            var iniTestA = new IniFile("Data\\FirstMerge.ini");
            var iniTestB = new IniFile("Data\\MergeColon.ini", new IniOptions { NameValueDelimiter = ':' });

            iniTestA += iniTestB;

            Assert.AreEqual("FirstValue", iniTestA.Get("firstsection", "FirstKey", ""));
            Assert.IsTrue(iniTestA.Get("firstsection", "secondkey", false));
            Assert.AreEqual(10.123, iniTestA.Get("firstsection", "ThirdKey", 1.2));
            Assert.AreEqual("ColonValue", iniTestA.Get("firstsection", "FourthKey", ""));
            Assert.IsTrue(iniTestA.Get("FirstSection", "FifthKey", false));
        }
    }
}