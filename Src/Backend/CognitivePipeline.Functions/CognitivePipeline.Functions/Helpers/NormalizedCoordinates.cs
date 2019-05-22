using System;
using System.Collections.Generic;
using System.Text;

namespace CognitivePipeline.Functions.Helpers
{
    public class NormalizedCoordinates
    {
        public static double[] GetNormalizedCoordinates(double left, double top, double width, double height, double imageWidth, double imageHeight)
        {
            return new double[] { left / imageWidth, top / imageHeight, width / imageWidth, height / height };
        }

        public static double[] GetBoundingCoordinates(double left, double top, double width, double height, double imageWidth, double imageHeight)
        {
            return new double[] { left * imageWidth, top * imageHeight, width * imageWidth, height * imageHeight };
        }
    }
}
