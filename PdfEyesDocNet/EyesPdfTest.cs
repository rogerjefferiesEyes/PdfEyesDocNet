using NUnit.Framework;
using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Applitools.Images;
using System.IO;

namespace PdfEyesDocNet
{
    public class Tests
    {
        private Eyes eyes;
        private string pdfFileName = "workshop-info-anand-bagmar.pdf";

        [SetUp]
        public void Setup()
        {
            eyes = new Eyes();
        }

        [Test]
        public void EyesPdfTest()
        {
            eyes.Open("PdfEyesDocNet", pdfFileName);
            using var docReader = DocLib.Instance.GetDocReader(
            pdfFileName,
            new PageDimensions(1080, 1920));

            for(int i = 0; i < docReader.GetPageCount(); i++)
            {
                using var pageReader = docReader.GetPageReader(i);

                var rawBytes = pageReader.GetImage();

                var width = pageReader.GetPageWidth();
                var height = pageReader.GetPageHeight();

                // here you are taking a byte array that 
                // represents the pixels directly where as Image.Load<Bgra32>()
                // is expected for an image encoded in png, jpeg etc format
                using var img = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
                // you are likely going to want this as well
                // otherwise you might end up with transparent parts.
                img.Mutate(x => x.BackgroundColor(Color.White));

                using var ms = new MemoryStream();
                img.SaveAsPng(ms);
                eyes.CheckImage(ms.ToArray(), "Page - " + (i + 1));
            }
            eyes.Close();

        }

        [TearDown]
        public void AfterEach()
        {
            eyes.AbortIfNotClosed();
        }

    }
}
