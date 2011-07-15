using NUnit.Framework;
using SquishIt.Framework.CSS;
using SquishIt.Framework.Minifiers;
using SquishIt.Framework.Minifiers.CSS;

namespace SquishIt.Tests
{
    [TestFixture]
    public class CssCompressorsTests
    {
        private string css = @" li {
                                    margin-bottom:0.1em;
                                    margin-left:0;
                                    margin-top:0.1em;
                                }

                                th {
                                    font-weight:normal;
                                    vertical-align:bottom;
                                }

                                .FloatRight {
                                    float:right;
                                }
                                
                                .FloatLeft {
                                    float:left;
                                }";

        [Test]
        public void NullCompressorTest()
        {
            var cssCompressor = MinifierFactory.Get<NullCompressor>();
            var uncompressedCss = cssCompressor.Minify(css);
            Assert.AreEqual(css, uncompressedCss);
        }

        [Test]
        public void YuiCompressorTest()
        {
            var cssCompressor = MinifierFactory.Get<YuiCompressor>();
            var compressedCss = cssCompressor.Minify(css);
            Assert.AreEqual(" li{margin-bottom:.1em;margin-left:0;margin-top:.1em}th{font-weight:400;vertical-align:bottom}.FloatRight{float:right}.FloatLeft{float:left}", compressedCss);
        }

        [Test]
        public void MsCompressorTest()
        {
            var cssCompressor = MinifierFactory.Get<MsCompressor>();
            var compressedCss = cssCompressor.Minify(css);
            Assert.AreEqual("li{margin-bottom:.1em;margin-left:0;margin-top:.1em}th{font-weight:normal;vertical-align:bottom}.FloatRight{float:right}.FloatLeft{float:left}", compressedCss);
        }
    }
}