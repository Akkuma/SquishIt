using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using SquishIt.Framework;
using SquishIt.Framework.Cachers;
using SquishIt.Framework.Files;
using SquishIt.Framework.JavaScript;
using SquishIt.Framework.Minifiers.JavaScript;
using SquishIt.Framework.Tests.Mocks;
using SquishIt.Framework.Utilities;
using SquishIt.Tests.Stubs;
using SquishIt.Tests.Helpers;

namespace SquishIt.Tests
{
    [TestFixture]
    public class JavaScriptBundleTests
    {
        private static string TEST1_UNMINIFIED = TestUtilities.NormalizeLineEndings(@"function product(a, b) {
	return a * b;
}

function sum(a, b) {
	return a + b;
}
");

        private string javaScript2 = TestUtilities.NormalizeLineEndings(@"function sum(a, b){
																						return a + b;
																			 }");

        private JavaScriptBundle javaScriptBundle;
        private JavaScriptBundle javaScriptBundle2;
        private JavaScriptBundle debugJavaScriptBundle;
        private JavaScriptBundle debugJavaScriptBundle2;
        private FileWriterFactory fileWriterFactory = new FileWriterFactory(new RetryableFileOpener(), 5);
        private FileReaderFactory fileReaderFactory = new FileReaderFactory(new RetryableFileOpener(), 5);
        private StubCurrentDirectoryWrapper currentDirectoryWrapper;
        private IHasher hasher;
        private ApplicationCache stubBundleCache;
        private static string FilePath = FileSystem.TempFilePath;
        private string Test1Path;
        private string Test2Path;
        private string TestUnderscoresPath;
        private string EmbeddedResourcePath;

        private const string TEST1_MINIFIED = "function product(a,b){return a*b}function sum(a,b){return a+b}";
        private const string TEST2_MINIFIED = "function sum(a,b){return a+b}";
        private const string JSMIN_MINIFIED = "\nfunction product(a,b){return a*b;}\nfunction sum(a,b){return a+b;}";
        private string minifiedOutput;

        private int outputFileNumber = 0;
        private string outputFileRoot = FilePath + TestUtilities.PreparePath(@"\js\output_");
        private string currentOutputFile;

        private string ResolveToCurrentDirectory(string filePath)
        {
            return FileSystem.ResolveAppRelativePathToFileSystem(Environment.CurrentDirectory + filePath);
        }

        private string GetResolvedTag(string filePath)
        {
            return String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", filePath);
        }


        [SetUp]
        public void Setup()
        {
            var nonDebugStatusReader = new StubDebugStatusReader(false);
            var debugStatusReader = new StubDebugStatusReader(true);
            currentDirectoryWrapper = new StubCurrentDirectoryWrapper();
            stubBundleCache = new ApplicationCache();

            var retryableFileOpener = new RetryableFileOpener();
            hasher = new Hasher(retryableFileOpener);

            //fileReaderFactory.SetContents(javaScript);

            javaScriptBundle = new JavaScriptBundle(nonDebugStatusReader,
                                                        fileWriterFactory,
                                                        fileReaderFactory,
                                                        currentDirectoryWrapper,
                                                        hasher,
                                                        stubBundleCache);

            javaScriptBundle2 = new JavaScriptBundle(nonDebugStatusReader,
                                                        fileWriterFactory,
                                                        fileReaderFactory,
                                                        currentDirectoryWrapper,
                                                        hasher,
                                                        stubBundleCache);

            debugJavaScriptBundle = new JavaScriptBundle(debugStatusReader,
                                                        fileWriterFactory,
                                                        fileReaderFactory,
                                                        currentDirectoryWrapper,
                                                        hasher,
                                                        stubBundleCache);

            debugJavaScriptBundle2 = new JavaScriptBundle(debugStatusReader,
                                                        fileWriterFactory,
                                                        fileReaderFactory,
                                                        currentDirectoryWrapper,
                                                        hasher,
                                                        stubBundleCache);

            outputFileNumber += 1;
            currentOutputFile = outputFileRoot + outputFileNumber + ".js";
        }

        [TearDown]
        public void Clean()
        {
            if (File.Exists(currentOutputFile))
            {
                using (var fileReader = fileReaderFactory.GetFileReader(currentOutputFile))
                {
                    Assert.AreEqual(minifiedOutput ?? TEST1_MINIFIED, fileReader.ReadToEnd());
                }
            }

            minifiedOutput = null;
        }

        [TestFixtureSetUp]
        public void Init()
        {
            Test1Path = ResolveToCurrentDirectory("/js/test1.js");
            Test2Path = ResolveToCurrentDirectory("/js/test2.js");
            EmbeddedResourcePath = ResolveToCurrentDirectory("/js/embedded.js");
            TestUnderscoresPath = ResolveToCurrentDirectory("/js/test_underscores.js");
            string[] pathsToNormalize = { Test1Path, Test2Path, TestUnderscoresPath };
            foreach (var path in pathsToNormalize)
	        {
                string content;
                using (var fileReader = fileReaderFactory.GetFileReader(path))
                {
                    content = TestUtilities.NormalizeLineEndings(fileReader.ReadToEnd());
                }

                using (var fileWriter = fileWriterFactory.GetFileWriter(path))
                {
                    fileWriter.Write(content);
                }
	        }

            Directory.CreateDirectory(FilePath + TestUtilities.PreparePath(@"\js"));
        }

        [TestFixtureTearDown]
        public void Dispose()
        {
            Directory.Delete(FilePath, true);
        }

        [Test]
        public void CanBundleJavaScript()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .Render(currentOutputFile);

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F"), tag);
            //Assert.AreEqual(TEST1_MINIFIED, currentFileReader.ReadToEnd());
        }

        
        [Test]
        public void CanBundleJavaScriptWithQuerystringParameter()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .Render(currentOutputFile + "?v=2");

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F&v=2"), tag);
        }
        
        [Test]
        public void CanCreateNamedBundle()
        {
            javaScriptBundle
                    .Add(Test1Path)
                    .AsNamed("TestNamed", currentOutputFile);

            var tag = javaScriptBundle.RenderNamed("TestNamed");

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F"), tag);
            //Assert.AreEqual(TEST1_MINIFIED, currentFileReader.ReadToEnd());
        }

        [Test]
        public void CanBundleJavaScriptWithRemote()
        {
            var tag = javaScriptBundle
                    .AddRemote(Test1Path, "http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js")
                    .Add(Test1Path)
                    .Render(currentOutputFile);


            Assert.AreEqual("<script type=\"text/javascript\" src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js\"></script>" + GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F"), tag);
            //Assert.AreEqual(TEST1_MINIFIED, currentFileReader.ReadToEnd());
        }

        [Test]
        public void CanBundleJavaScriptWithRemoteAndQuerystringParameter()
        {
            var tag = javaScriptBundle
                    .AddRemote(Test1Path, "http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js")
                    .Add(Test1Path)
                    .Render(currentOutputFile + "?v=2_2");

            Assert.AreEqual("<script type=\"text/javascript\" src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js\"></script>" + GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F&v=2_2"), tag);
        }

        [Test]
        public void CanCreateNamedBundleWithRemote()
        {
            javaScriptBundle
                    .AddRemote(Test1Path, "http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js")
                    .Add(Test1Path)
                    .AsNamed("TestCdn", currentOutputFile);

            var tag = javaScriptBundle.RenderNamed("TestCdn");

            Assert.AreEqual("<script type=\"text/javascript\" src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.4.2/jquery.min.js\"></script>" + GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F"), tag);
        }

        [Test]
        public void CanBundleJavaScriptWithEmbeddedResource()
        {
            var tag = javaScriptBundle
                    .AddEmbeddedResource(Test1Path, "SquishIt.Tests://js.embedded.js")
                    .Render(currentOutputFile);

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F"), tag);
        }

        [Test]
        public void CanDebugBundleJavaScriptWithEmbeddedResource()
        {
            var tag = debugJavaScriptBundle
                    .AddEmbeddedResource(EmbeddedResourcePath, "SquishIt.Tests://js.embedded.js")
                    .Render(currentOutputFile);

            Assert.AreEqual("<script type=\"text/javascript\" src=\""+ EmbeddedResourcePath + "\"></script>\n", tag);
        }

        [Test]
        public void CanRenderDebugTags()
        {
            debugJavaScriptBundle
                    .Add(Test1Path)
                    .Add(Test2Path)
                    .AsNamed("TestWithDebug", currentOutputFile);

            var tag = debugJavaScriptBundle.RenderNamed("TestWithDebug");

            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n<script type=\"text/javascript\" src=\"" + Test2Path + "\"></script>\n", tag);
        }

        [Test]
        public void CanRenderDebugTagsTwice()
        {
            debugJavaScriptBundle
                    .Add(Test1Path)
                    .Add(Test2Path)
                    .AsNamed("TestWithDebug", currentOutputFile);

            debugJavaScriptBundle2
                    .Add(Test1Path)
                    .Add(Test2Path)
                    .AsNamed("TestWithDebug", currentOutputFile);

            var tag1 = debugJavaScriptBundle.RenderNamed("TestWithDebug");
            var tag2 = debugJavaScriptBundle2.RenderNamed("TestWithDebug");

            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n<script type=\"text/javascript\" src=\"" + Test2Path + "\"></script>\n", tag1);
            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n<script type=\"text/javascript\" src=\"" + Test2Path + "\"></script>\n", tag2);
        }

        [Test]
        public void CanCreateNamedBundleWithDebug()
        {
            debugJavaScriptBundle
                    .Add(Test1Path)
                    .Add(Test2Path)
                    .AsNamed("NamedWithDebug", currentOutputFile);

            var tag = debugJavaScriptBundle.RenderNamed("NamedWithDebug");

            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n<script type=\"text/javascript\" src=\"" + Test2Path + "\"></script>\n", tag);
        }

        [Test]
        public void CanCreateBundleWithNullMinifer()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .WithMinifier<NullMinifier>()
                    .Render(currentOutputFile);

            minifiedOutput = TEST1_UNMINIFIED;

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=16A3DC886F2A8804CB8519BF19092591"), tag);
        }

        [Test]
        public void CanCreateBundleWithJsMinMinifer()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .WithMinifier<JsMinMinifier>()
                    .Render(currentOutputFile);

            minifiedOutput = JSMIN_MINIFIED;

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=1152F52E552066D5B087E85930D59223"), tag);
        }

        [Test]
        public void CanCreateBundleWithJsMinMiniferByPassingInstance()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .WithMinifier(new JsMinMinifier())
                    .Render(currentOutputFile);

            minifiedOutput = JSMIN_MINIFIED;

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=1152F52E552066D5B087E85930D59223"), tag);
        }

        [Test]
        public void CanCreateEmbeddedBundleWithJsMinMinifer()
        {
            var tag = javaScriptBundle
                    .AddEmbeddedResource(Test1Path, "SquishIt.Tests://js.embedded.js")
                    .WithMinifier<JsMinMinifier>()
                    .Render(currentOutputFile);

            minifiedOutput = JSMIN_MINIFIED;

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=1152F52E552066D5B087E85930D59223"), tag);
        }

        /*[Test]
        public void CanCreateBundleWithClosureMinifer()
        {
                var tag = javaScriptBundle
                        .Add("~/js/test.js")
                        .WithMinifier(JavaScriptMinifiers.Closure)
                        .Render("~/js/output_8.js");

                Assert.AreEqual("<script type=\"text/javascript\" src=\"js/output_8.js?r=00DFDFFC4078EFF6DFCC6244EAB77420\"></script>", tag);
                Assert.AreEqual("function product(a,b){return a*b}function sum(a,b){return a+b};\r\n", fileWriterFactory.Files[@"\js\output_8.js"]);
        }*/

        [Test]
        public void CanRenderOnlyIfFileMissing()
        {
            //fileReaderFactory.SetFileExists(false);

            javaScriptBundle
                    .Add(Test1Path)
                    .RenderOnlyIfOutputFileMissing()
                    .Render(currentOutputFile);

            using (var fr = fileReaderFactory.GetFileReader(currentOutputFile))
            {
                Assert.AreEqual(TEST1_MINIFIED, fr.ReadToEnd());
            }

            //fileReaderFactory.SetContents(javaScript2);
            //fileReaderFactory.SetFileExists(true);
            javaScriptBundle.ClearCache();

            javaScriptBundle
                    .Add(Test2Path)
                    .RenderOnlyIfOutputFileMissing()
                    .Render(currentOutputFile);

           // Assert.AreEqual("function product(a,b){return a*b}function sum(a,b){return a+b}", fileReaderFactory.GetFileReader(TestUtilities.PreparePathRelativeToWorkingDirectory(@"\js\output_9.js")).ReadToEnd());
        }

        [Test]
        public void CanRerenderFiles()
        {
            //fileReaderFactory.SetFileExists(false);

            javaScriptBundle
                    .Add(Test1Path)
                    .Render(currentOutputFile);

            using (var fr = fileReaderFactory.GetFileReader(currentOutputFile))
            {
                Assert.AreEqual(TEST1_MINIFIED, fr.ReadToEnd());
            }

            //fileReaderFactory.SetContents(javaScript2);
            //fileReaderFactory.SetFileExists(true);
            //fileWriterFactory.Files.Clear();
            javaScriptBundle.ClearCache();

            javaScriptBundle2
                    .Add(Test2Path)
                    .Render(currentOutputFile);

            minifiedOutput = TEST2_MINIFIED;
        }

        [Test]
        public void CanBundleJavaScriptWithHashInFilename()
        {
            currentOutputFile = currentOutputFile.Replace(".js", "");
            currentOutputFile += "#.js";

            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .Render(currentOutputFile);

            Assert.AreEqual(GetResolvedTag(currentOutputFile.Replace("#", "E36D384488ABCF73BCCE650C627FB74F")), tag);
        }

        [Test]
        public void CanBundleJavaScriptWithUnderscoresInName()
        {
            currentOutputFile = currentOutputFile.Replace(".js", "");
            currentOutputFile += "#.js";

            var tag = javaScriptBundle
                    .Add(TestUnderscoresPath)
                    .Render(currentOutputFile);

            Assert.AreEqual(GetResolvedTag(currentOutputFile.Replace("#", "E36D384488ABCF73BCCE650C627FB74F")), tag);
            //Assert.AreEqual("function product(a,b){return a*b}function sum(a,b){return a+b}", fileReaderFactory.GetFileReader(TestUtilities.PreparePathRelativeToWorkingDirectory(@"\js\outputunder_E36D384488ABCF73BCCE650C627FB74F.js")).ReadToEnd());
        }

        [Test]
        public void CanCreateNamedBundleWithForcedRelease()
        {
            debugJavaScriptBundle
                    .Add(Test1Path)
                    .ForceRelease()
                    .AsNamed("ForceRelease", currentOutputFile);

            var tag = javaScriptBundle.RenderNamed("ForceRelease");

            Assert.AreEqual(GetResolvedTag(currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F"), tag);
            //Assert.AreEqual("function product(a,b){return a*b}function sum(a,b){return a+b}", fileReaderFactory.GetFileReader(TestUtilities.PreparePathRelativeToWorkingDirectory(@"\js\output_forcerelease.js")).ReadToEnd());
        }

        [Test]
        public void CanBundleJavaScriptWithSingleAttribute()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .WithAttribute("charset", "utf-8")
                    .Render(currentOutputFile );

            Assert.AreEqual("<script type=\"text/javascript\" charset=\"utf-8\" src=\"" + currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F\"></script>", tag);
        }

        [Test]
        public void CanBundleJavaScriptWithSingleMultipleAttributes()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .WithAttribute("charset", "utf-8")
                    .WithAttribute("other", "value")
                    .Render(currentOutputFile);

            Assert.AreEqual("<script type=\"text/javascript\" charset=\"utf-8\" other=\"value\" src=\"" + currentOutputFile + "?r=E36D384488ABCF73BCCE650C627FB74F\"></script>", tag);
        }

        [Test]
        public void CanDebugBundleWithAttribute()
        {
            string tag = debugJavaScriptBundle
                    .Add(Test1Path)
                    .Add(Test2Path)
                    .WithAttribute("charset", "utf-8")
                    .Render(currentOutputFile);

            Assert.AreEqual("<script type=\"text/javascript\" charset=\"utf-8\" src=\"" + Test1Path + "\"></script>\n<script type=\"text/javascript\" charset=\"utf-8\" src=\"" + Test2Path + "\"></script>\n", tag);
        }

        [Test]
        public void CanCreateCachedBundle()
        {
            var tag = javaScriptBundle
                    .Add(Test1Path)
                    .AsCached("Test", currentOutputFile);

            var content = javaScriptBundle.RenderCached("Test");

           // Assert.AreEqual("<script type=\"text/javascript\" src=\"js/output_2.js?r=E36D384488ABCF73BCCE650C627FB74F\"></script>", tag);
            Assert.AreEqual(TEST1_MINIFIED, content);
        }

        [Test]
        public void CanCreateCachedBundleAssetTag()
        {
            javaScriptBundle.ClearCache();

            javaScriptBundle
                    .Add(Test1Path)
                    .AsCached("Test", "~/assets/js/main");

            var content = javaScriptBundle.RenderCached("Test");

            var tag = javaScriptBundle.RenderCachedAssetTag("Test");

            //Assert.AreEqual("<script type=\"text/javascript\" src=\"assets/js/main?r=E36D384488ABCF73BCCE650C627FB74F\"></script>", tag);
            Assert.AreEqual(TEST1_MINIFIED, content);
        }

        [Test]
        public void CanCreateCachedBundleWithDebug()
        {
            javaScriptBundle.ClearCache();

            var tag = debugJavaScriptBundle
                    .Add(Test1Path)
                    .AsCached("Test", currentOutputFile);

            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n", tag);
        }

        [Test]
        public void CanChangeNamedBundleToDebug()
        {
            javaScriptBundle
                    .Add(Test1Path)
                    .Add(Test2Path)
                    .AsNamed("ChangedToDebug", currentOutputFile);

            var tag = javaScriptBundle2.GetNamed("ChangedToDebug").ForceDebug().RenderNamed("ChangedToDebug");

            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n<script type=\"text/javascript\" src=\"" + Test2Path + "\"></script>\n", tag);
        }


        [Test]
        public void CanRemoveAssets()
        {
            var tag = debugJavaScriptBundle
                        .Add(Test1Path)
                        .Add(Test2Path)
                        .Add(TestUnderscoresPath)
                        .Remove(Test2Path, TestUnderscoresPath)
                        .Render(currentOutputFile);

            Assert.AreEqual("<script type=\"text/javascript\" src=\"" + Test1Path + "\"></script>\n", tag);
        }
    }
}