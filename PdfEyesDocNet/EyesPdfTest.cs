using NUnit.Framework;
using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Applitools.Images;
using Applitools.Utils.Geometry;
using System;
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
            using var docReader = DocLib.Instance.GetDocReader(
            pdfFileName,
            new PageDimensions(1.0d));

            int pageCount = docReader.GetPageCount();

            Assert.Greater(pageCount, 0, "Should have page count greater than zero.");

            // Get Size of first page, and use as Eyes viewport size
            var viewportWidth = docReader.GetPageReader(0).GetPageWidth();
            var viewportHeight = docReader.GetPageReader(0).GetPageHeight();

            // Open Applitools Eyes Test Session with viewport size set
            eyes.Open(
                "PdfEyesDocNet",
                pdfFileName,
                new RectangleSize(viewportWidth, viewportHeight));

            for (int i = 0; i < pageCount; i++)
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

                // Get memory stream for PNG formatted image
                using var ms = new MemoryStream();
                img.SaveAsPng(ms);

                // Send PNG memory stream byte array as Eyes Checkpoint image
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
