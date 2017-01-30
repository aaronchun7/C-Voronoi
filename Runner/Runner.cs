﻿using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Voronoi;

namespace Runner
{
    internal class Runner
    {
        private const string MissingArgs = "Error Args: {Number Of Points To Plot} {Image To Voronoi} {File Name} {File Directory}";
        private const int C = 50;

        private static int Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine(MissingArgs);
                return 1;
            }
            try
            {
                var numberOfPointsToPlot = int.Parse(args[0]);
                var originalImage = new Bitmap(args[1]);
                ReadonlyBitmap.Set(originalImage, originalImage.Width, originalImage.Height);
                var bmpData = originalImage.LockBits(new Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadOnly, originalImage.PixelFormat);
//                var newBitmap = new Bitmap(bmp.Width, bmp.Height, bmpData.Stride, bmp.PixelFormat, bmpData.Scan0);
                var newBitmap = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                Run(newBitmap, numberOfPointsToPlot, args[2], args[3]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
            return 0;
        }

        private static void Run(Bitmap newImage, int numberOfPointsToPlot, string fileName, string fileDirectory)
	    {
            var nums = Enumerable.Range(0, C).ToArray();
            var result = new ConcurrentDictionary<VoronoiOutput, double>();
	        Parallel.ForEach(nums, _ =>
	            {
                    var voronoiOutput = Fortunes.Run(ReadonlyBitmap.Get().Width, ReadonlyBitmap.Get().Height, numberOfPointsToPlot);
                    var averageDeltaE = voronoiOutput.CalculateAccuracy(ReadonlyBitmap.Get());
                    result.TryAdd(voronoiOutput, averageDeltaE);
	            });
            var bestVoronoi = result.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
	        using (var writer = new Drawer(newImage))
	        {
                writer.DrawVoronoi(bestVoronoi);
                writer.SaveToNewImageFile(fileName, @fileDirectory);
            }
        }
	}
}
