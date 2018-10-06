using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScottClayton.Image;
using System;
using System.Drawing;

namespace ScottClayton.CAPTCHA.Tests.Image
{
    [TestClass]
    public class ImageExtensionsTests
    {
        [TestMethod]
        public void TestGetLAB()
        {
            Color rgb1 = Color.FromArgb(165, 25, 57);
            LAB lab1 = rgb1.GetLAB();

            // Testing against data from http://colormine.org/convert/rgb-to-lab
            Assert.AreEqual(Math.Round(lab1.L, 10), Math.Round(35.96503817999396, 10));
            Assert.AreEqual(Math.Round(lab1.a, 10), Math.Round(55.51661972639224, 10));
            Assert.AreEqual(Math.Round(lab1.b, 10), Math.Round(19.28293113792695, 10));
        }

        [TestMethod]
        public void TestGetEDeltaColorDifference()
        {
            Color rgb1 = Color.FromArgb(34, 195, 19);
            Color rgb2 = Color.FromArgb(23, 57, 187);

            double edelta = rgb1.GetEDeltaColorDifference(rgb2);

            // Testing against data from http://colormine.org/delta-e-calculator
            Assert.AreEqual(Math.Round(edelta, 3), Math.Round(177.543, 3));
        }
    }
}