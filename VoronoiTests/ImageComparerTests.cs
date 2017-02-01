﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voronoi;

namespace VoronoiTests
{
    [TestClass]
    public class ImageComparerTests
    {
        private static List<IntPoint2D> GetIntPoint2DListOfRegion()
        {
            var regionOfPoints = new List<IntPoint2D>();
            for (var i = 1; i <= 50; ++i)
            {
                for (var j = 1; j <= 50; ++j)
                {
                    regionOfPoints.Add(new IntPoint2D(i,j));
                }
            }
            return regionOfPoints;
        }

        [TestMethod]
        public void CalculateRegionsDeltaEList_SameImage_ExpectZero()
        {
            var originalBitmap = new Bitmap(@"./images/ImageComparerTestWhitePicture");
            var squareRegionofPoints = GetIntPoint2DListOfRegion();
            var site = new IntPoint2D(25, 25);
            var imageComparer = new ImageComparer();
            
            var allDeltaEList = imageComparer.CalculateRegionsDeltaEList(originalBitmap, squareRegionofPoints, site);
            Assert.AreEqual(0.0, allDeltaEList.Average());
        }

        [TestMethod]
        public void CalculateRegionsDeltaEList_DifferentImages_ExpectNonZero()
        {
            var originalBitmap = new Bitmap(@"./images/ImageComparerTestWhiteBlackPicture");
            var squareRegionofPoints = GetIntPoint2DListOfRegion();
            var site = new IntPoint2D(25, 25);
            var imageComparer = new ImageComparer();

            var allDeltaEList = imageComparer.CalculateRegionsDeltaEList(originalBitmap, squareRegionofPoints, site);
            Assert.AreNotEqual(0.0, allDeltaEList.Average());
        }
    }
}
