using Extreme.Statistics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMPI_Proj
{
    public static class MathHelper
    {
        public static double GetPirsonQuantilDistribution(double p, double v)
        {
            var Up = GetQuantilOfNormalDistribution(p);
            return v * Math.Pow(1.0 - (2.0 / (9.0 * v)) + Up * Math.Sqrt(2.0 / (9.0 * v)), 3);
        }
        public static double GetStudentQuantilDistribution(double p, double v)
        {
            var Up = GetQuantilOfNormalDistribution(p);
            var g1 = 0.25 * (Math.Pow(Up, 3) + Up);
            var g2 = (1.0 / 96.0) * (5 * Math.Pow(Up, 5) + 16 * Math.Pow(Up, 3) + 3 * Up);
            var g3 = (1.0 / 384.0) * (3 * Math.Pow(Up, 7) + 19 * Math.Pow(Up, 5) + 17 * Math.Pow(Up, 3) - 15 * Up);
            var g4 = (1.0 / 92160.0) * (79 * Math.Pow(Up, 9) + 779 * Math.Pow(Up, 7) + 1482 * Math.Pow(Up, 5) - 1920 * Math.Pow(Up, 3) - 945 * Up);
            return Up + (1.0 / v) * g1 + (1.0 / Math.Pow(v, 2)) * g2 + (1.0 / Math.Pow(v, 3)) * g3 + (1.0 / Math.Pow(v, 4)) * g4;
        }
        public static double GetQuantilOfNormalDistribution(double p)
        {
            if (p > 0.5)
                return PhiFunction(1.0 - p);

            return -PhiFunction(p);
        }

        private static double PhiFunction(double pValue)
        {
            double t = Math.Sqrt(-2 * Math.Log(pValue));
            double c0 = 2.515517;
            double c1 = 0.802853;
            double c2 = 0.010328;
            double d1 = 1.432788;
            double d2 = 0.1892659;
            double d3 = 0.001308;

            var upperPart = c0 + c1 * t + c2 * Math.Pow(t, 2);
            var lowerPart = 1 + d1 * t + d2 * Math.Pow(t, 2) + d3 * Math.Pow(t, 3);

            return t - (upperPart / lowerPart);
        }


        public static double Median(this IEnumerable<double> source)
        {
            var data = source.OrderBy(n => n).ToArray();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }
    }
}
