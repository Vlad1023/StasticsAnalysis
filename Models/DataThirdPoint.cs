using System;
using System.Collections.Generic;
using System.Text;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Linq;

namespace EMPI_Proj.Models
{
    public class DataThirdPoint
    {
        private double avgValue;
        private double standartDeviationOfDataset;
        private readonly double LengthOfDataset;
        private readonly IEnumerable<DataThirdPointViewModel> dataThirdPoints;
        public DataThirdPoint(IEnumerable<DataFirstPoint> dataFirstPoint)
        {
            this.LengthOfDataset = dataFirstPoint.Count();
            var tmpDataset = new List<DataThirdPointViewModel>();
            tmpDataset.Add(DefineAverageDataset(dataFirstPoint));
            tmpDataset.Add(DefineMedianDataset(dataFirstPoint));
            tmpDataset.Add(DefineStdDataset(dataFirstPoint));
            tmpDataset.Add(DefineSkewnessDataset(dataFirstPoint));
            tmpDataset.Add(DefineKurtosisDataset(dataFirstPoint));
            tmpDataset.Add(DefineAntikurtosisDataset(dataFirstPoint));
            tmpDataset.Add(DefineMinDataset(dataFirstPoint));
            tmpDataset.Add(DefineMaxDataset(dataFirstPoint));

            this.dataThirdPoints = tmpDataset;
        }
        private DataThirdPointViewModel DefineAverageDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            var allValues = dataFirstPoints.Select(el => el.Value);
            this.avgValue = allValues.Average();
            this.standartDeviationOfDataset = StandartDeviationOfDataset(dataFirstPoints);
            var stdOfAvg = StandartDeviationOfDataset(dataFirstPoints) / Math.Sqrt(allValues.Count());
            var confIntervalLowBorder = CondifenceIntervalForAvgValueLowBorder(dataFirstPoints, stdOfAvg);
            var confItervalUpperBorder = CondifenceIntervalForAvgValueUpperBorder(dataFirstPoints, stdOfAvg);
            return new DataThirdPointViewModel("Mean", avgValue, stdOfAvg, confIntervalLowBorder, confItervalUpperBorder);
        }
        private DataThirdPointViewModel DefineMedianDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            var median = dataFirstPoints.Select(el => el.Value).Median();
            int j = (int)((LengthOfDataset / 2.0) - MathHelper.GetQuantilOfNormalDistribution(0.975) * (Math.Sqrt(LengthOfDataset / 2.0)));
            int k = (int)((LengthOfDataset / 2.0) + 1 + MathHelper.GetQuantilOfNormalDistribution(0.975) * (Math.Sqrt(LengthOfDataset / 2.0)));
            return new DataThirdPointViewModel("Median", median, null, dataFirstPoints.ElementAt(j).Value, dataFirstPoints.ElementAt(k).Value);
        }
        private DataThirdPointViewModel DefineStdDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            var lowBorder = CondifenceIntervalForStdValueLowBorder(dataFirstPoints);
            var upperBorder = CondifenceIntervalForStdValueUpperBorder(dataFirstPoints);
            var stdOfStd = StdOfStd(dataFirstPoints);
            return new DataThirdPointViewModel("Standart deviation", this.standartDeviationOfDataset, stdOfStd, lowBorder, upperBorder);
        }

        private DataThirdPointViewModel DefineSkewnessDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            var sumOfDifference = dataFirstPoints.Sum(x => Math.Pow(x.Value - this.avgValue,3));
            var A1 = (1.0 / (LengthOfDataset * Math.Pow(StandartBiasDeviationOfDataset(dataFirstPoints), 3))) * sumOfDifference;
            var std = Math.Sqrt(6.0 * (LengthOfDataset - 2) / ((LengthOfDataset + 1) * (LengthOfDataset + 3)));
            var studentQuantil = MathHelper.GetStudentQuantilDistribution(0.975, LengthOfDataset - 1);
            var lowBorder = A1 - studentQuantil * std;
            var upperBorder = A1 + studentQuantil * std;
            return new DataThirdPointViewModel("Coefficient of skewness", A1, std, lowBorder, upperBorder);
        }
        private DataThirdPointViewModel DefineKurtosisDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            var sumOfDifference = dataFirstPoints.Sum(x => Math.Pow(x.Value - this.avgValue, 4));
            var E1 = ((1.0 / (LengthOfDataset * Math.Pow(StandartBiasDeviationOfDataset(dataFirstPoints), 4))) * sumOfDifference) - 3;
            var std = Math.Sqrt((24 * LengthOfDataset * (LengthOfDataset - 2) * (LengthOfDataset - 3)) / (Math.Pow((LengthOfDataset + 1),2) * (LengthOfDataset + 3) * (LengthOfDataset + 5)));
            var studentQuantil = MathHelper.GetStudentQuantilDistribution(0.975, LengthOfDataset - 1);
            var lowBorder = E1 - studentQuantil * std;
            var upperBorder = E1 + studentQuantil * std;
            return new DataThirdPointViewModel("Coefficient of kurtosis", E1, std, lowBorder, upperBorder);
        }

        private DataThirdPointViewModel DefineAntikurtosisDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            var sumOfDifference = dataFirstPoints.Sum(x => Math.Pow(x.Value - this.avgValue, 4));
            var E1 = ((1.0 / (LengthOfDataset * Math.Pow(StandartBiasDeviationOfDataset(dataFirstPoints), 4))) * sumOfDifference) - 3;
            return new DataThirdPointViewModel("Coefficient of antikurtosis", 1.0/ Math.Sqrt(E1 + 3), null, null, null);
        }

        private DataThirdPointViewModel DefineMaxDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            return new DataThirdPointViewModel("Max", dataFirstPoints.Select(el => el.Value).Max() , null, null, null);
        }

        private DataThirdPointViewModel DefineMinDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            return new DataThirdPointViewModel("Min", dataFirstPoints.Select(el => el.Value).Min(), null, null, null);
        }

        private double StdOfStd(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            return this.standartDeviationOfDataset / Math.Sqrt(2 * LengthOfDataset);
        }

        private double StandartDeviationOfDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            double upperPart = 0;
            foreach (var item in dataFirstPoints)
            {
                upperPart += Math.Pow(item.Value - this.avgValue, 2);
            }
            return Math.Sqrt(upperPart / (LengthOfDataset - 1));
        }

        private double StandartBiasDeviationOfDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            double upperPart = 0;
            foreach (var item in dataFirstPoints)
            {
                upperPart += Math.Pow(item.Value - this.avgValue, 2);
            }
            return Math.Sqrt(upperPart / (LengthOfDataset));
        }

        private double CondifenceIntervalForStdValueLowBorder(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            double pirsonQuantil = MathHelper.GetPirsonQuantilDistribution(0.975, LengthOfDataset - 1);
            var result = this.standartDeviationOfDataset * Math.Sqrt((LengthOfDataset - 1)/ pirsonQuantil);
            return result;
        }

        private double CondifenceIntervalForStdValueUpperBorder(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            double pirsonQuantil = MathHelper.GetPirsonQuantilDistribution(0.025, LengthOfDataset - 1);
            var result = this.standartDeviationOfDataset * Math.Sqrt((LengthOfDataset - 1) / pirsonQuantil);
            return result;
        }

        private double CondifenceIntervalForAvgValueLowBorder(IEnumerable<DataFirstPoint> dataFirstPoints, double std)
        {
            double studentQuantil = MathHelper.GetStudentQuantilDistribution(0.975, LengthOfDataset - 1);
            var result = this.avgValue - studentQuantil * std;
            return result;
        }

        private double CondifenceIntervalForAvgValueUpperBorder(IEnumerable<DataFirstPoint> dataFirstPoints, double std)
        {
            double studentQuantil = MathHelper.GetStudentQuantilDistribution(0.975, LengthOfDataset - 1);
            var result = this.avgValue + studentQuantil * std;
            return result;
        }

        public IEnumerable<DataThirdPointViewModel> DataThirdPoints => dataThirdPoints;
    }

    public class DataThirdPointViewModel
    {
        public DataThirdPointViewModel(string paramName, double? value, double? standartDeviation, double? CondifenceIntervalLowBorder, double? CondifenceIntervalUpperBorder)
        {
            this.paramName = paramName;
            this.value = value;
            this.standartDeviation = standartDeviation;
            this.CondifenceIntervalLowBorder = CondifenceIntervalLowBorder;
            this.CondifenceIntervalUpperBorder = CondifenceIntervalUpperBorder;
        }
        private readonly string paramName;
        private readonly double? value;
        private readonly double? standartDeviation;
        private readonly double? CondifenceIntervalLowBorder;
        private readonly double? CondifenceIntervalUpperBorder;

        public string ParamName => this.paramName;
        public string Value => this.value == null ? "-" : Math.Round(this.value.Value, 3).ToString();
        public string StandartSqrDeviation => this.standartDeviation == null ? "-" : Math.Round(this.standartDeviation.Value, 3).ToString();
        public string CondifenceInterval
        {
            get
            {
                return this.CondifenceIntervalLowBorder == null || this.CondifenceIntervalUpperBorder == null ?
                    "-" :
                    $"{Math.Round(this.CondifenceIntervalLowBorder.Value, 3)} ; {Math.Round(this.CondifenceIntervalUpperBorder.Value, 3)}";
            }
        }
    }
}
