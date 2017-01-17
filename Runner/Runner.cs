﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Voronoi;


namespace Runner
{
    internal class Runner
    {
        private const string MissingArgs = "Error: missing file location or number of plot points";

        private const int Width = 1920;
	    private const int Height = 1080;

	    private static int Main2(string[] args)
		{
            if (args.Length < 2)
            {
                Console.WriteLine(MissingArgs);
                return 1;
            }
            try
            {
                var numberOfPointsToPlot = int.Parse(args[0]);
                // Create a bitmap.
                var bmp = new Bitmap(args[1]);
                // Retrieve the bitmap data from the the bitmap.
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, bmp.PixelFormat);
                //Create a new bitmap.
                var newBitmap = new Bitmap(bmp.Width, bmp.Height, bmpData.Stride, bmp.PixelFormat, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
                //calls run
                Run(bmp, newBitmap, numberOfPointsToPlot);
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                return 1;
            }

            return 0;
		}

        private static void Main()
        {
            Console.Out.WriteLine("Started");
            var output = Fortunes.Run(Width, Height, 500);
            output.OutputConsole();
            output.OutputFile(Width, Height);
            Console.Out.WriteLine("Finished");
        }

	    private static void Run(Bitmap originalImage, Bitmap newImage, int numberOfPointsToPlot)
	    {
            //
	        while (true)
	        {
                //diagrams
	            while (true)
	            {
                    //regions
	                break;
	            }
                //average De
	            break;
	        }
            //call printer
	    }
	}
}
