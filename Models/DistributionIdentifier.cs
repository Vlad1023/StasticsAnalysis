using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMPI_Proj.Models
{
    class DistributionIdentifier
    {
        private readonly int nParameter;
        private readonly double lengthOfClass;
        private readonly IEnumerable<DataFirstPoint> dataFirstPoints;
        private readonly IEnumerable<ParettoParameter> distributionParams;
        private readonly PlotModel modelForRenewedDensityFunction;
        private readonly PlotModel modelForRenewedDistributionFunction;
        public DistributionIdentifier(IEnumerable<DataFirstPoint> dataFirstPoints, PlotModel histogramModel, double lengthOfClass)
        {
            this.dataFirstPoints = dataFirstPoints;
            this.nParameter = dataFirstPoints.Count() - 1;
            this.lengthOfClass = lengthOfClass;

            this.distributionParams = CalculateParams();
            this.modelForRenewedDensityFunction = CalculateModelForRenewedDensityFunction(histogramModel);
            this.modelForRenewedDistributionFunction = CalculateModelForRenewedDistributionFunction();
        }

        public IEnumerable<ParettoParameter> DistributionParams => distributionParams;
        public PlotModel ModelForRenewedDensityFunction => modelForRenewedDensityFunction;
        public PlotModel ModelForRenewedDistributionFunction => modelForRenewedDistributionFunction;

        private IEnumerable<ParettoParameter> CalculateParams()
        {
            var kBiased = calculateKBiased();
            var aBiased = calculateABiased(kBiased);
            var aParam = calculateA(aBiased);
            var kParam = calculateK(aParam, kBiased);

            var stdForA = calculateStdForA();
            var stdForK = calculateStdForK(kBiased);

            return new List<ParettoParameter>
            {
                new ParettoParameter("a", aParam, stdForA, aParam - MathHelper.GetStudentQuantilDistribution(0.975, this.nParameter - 1) * stdForA, aParam + MathHelper.GetStudentQuantilDistribution(0.975, this.nParameter - 1) * stdForA),
                new ParettoParameter("k", kParam, stdForK, kParam - MathHelper.GetStudentQuantilDistribution(0.975, this.nParameter - 1) * stdForK, kParam + MathHelper.GetStudentQuantilDistribution(0.975, this.nParameter - 1) * stdForK)
            };
        }

        private PlotModel CalculateModelForRenewedDensityFunction(PlotModel histogramModel)
        {
            var plotModel = new PlotModel();
            var xAxis = new OxyPlot.Axes.LinearAxis
            { Title = "classRange", Position = AxisPosition.Bottom, LabelFormatter = (param) => Math.Round(param,3).ToString() };
            plotModel.Axes.Add(xAxis);

            var yAxis = new OxyPlot.Axes.LinearAxis
            { Title = "RelativeFreq", Position = AxisPosition.Left };
            plotModel.Axes.Add(yAxis);

            foreach (var item in histogramModel.Series)
            {
                var series = item as RectangleBarSeries;
                if (series != null)
                {
                    var seriesCopy = new RectangleBarSeries();
                    foreach (var subitem in series.Items)
                    {
                        seriesCopy.Items.Add(new RectangleBarItem(subitem.X0, subitem.Y0, subitem.X1, subitem.Y1));
                    }
                    plotModel.Series.Add(seriesCopy);
                }
                else
                {
                    var densitySeries = item as LineSeries;
                    var seriesCopy = new LineSeries();
                    foreach (var subitem in densitySeries.Points)
                    {
                        seriesCopy.Points.Add(new DataPoint(subitem.X, subitem.Y));
                    }
                    plotModel.Series.Add(seriesCopy);
                }

            }

            var lineSeries = new LineSeries();
            lineSeries.Title = "RestoredDensityFunction";
            lineSeries.Color = OxyColor.Parse("#FF0000");
            foreach (var item in this.dataFirstPoints)
            {
                lineSeries.Points.Add(new DataPoint(item.Value, densityFuncForParetto(item.Value) * this.lengthOfClass));
            }
            plotModel.Series.Add(lineSeries);
            return plotModel;
        }

        private PlotModel CalculateModelForRenewedDistributionFunction()
        {
            var plotModel = new PlotModel();
            var xAxis = new OxyPlot.Axes.LinearAxis
            { Title = "value", Position = AxisPosition.Bottom, LabelFormatter = (param) => Math.Round(param, 3).ToString() };
            plotModel.Axes.Add(xAxis);

            var yAxis = new OxyPlot.Axes.LinearAxis
            { Title = "EmpFunctionResult", Position = AxisPosition.Left };
            plotModel.Axes.Add(yAxis);

            var stepSeries = new StairStepSeries();
            foreach (var item in this.dataFirstPoints)
            {
                stepSeries.Points.Add(new DataPoint(item.Value, item.EmpFunctionResult));
            }
            var distributionFunc = new LineSeries();
            foreach (var item in this.dataFirstPoints)
            {
                distributionFunc.Points.Add(new DataPoint(item.Value, distributionFuncForParetto(item.Value)));
            }
            plotModel.Series.Add(stepSeries);
            plotModel.Series.Add(distributionFunc);

            return plotModel;
        }

        private double calculateABiased(double kBiased)
        {
            var n = nParameter;
            return n / this.dataFirstPoints.Sum(el => Math.Log(el.Value / kBiased));
        }

        private double calculateKBiased()
        {
            return this.dataFirstPoints.Min(el => el.Value);
        }

        private double calculateA(double aBiasedParameter)
        {
            return ((this.nParameter - 2.0) / this.nParameter) * aBiasedParameter;
        }

        private double calculateK(double aParameter, double kBiased)
        {
            return (1.0 - (1.0 / (this.nParameter - 1) * aParameter) ) * kBiased;
        }

        private double calculateStdForA()
        {
            return (double)(this.nParameter - 2) / this.nParameter;
        }
        private double calculateStdForK(double kBiased)
        {
            return kBiased * ((double)(this.nParameter - 2) / this.nParameter);
        }

        private double densityFuncForParetto(double x)
        {
            var k = distributionParams.First(el => el.Name == "k").getValue();
            var a = distributionParams.First(el => el.Name == "a").getValue();

            return (a / k) * (Math.Pow((k / x), a + 1));
        }

        private double distributionFuncForParetto(double x)
        {
            var k = distributionParams.First(el => el.Name == "k").getValue();
            var a = distributionParams.First(el => el.Name == "a").getValue();

            return 1 - Math.Pow(k / x, a);
        }
    }

    public class ParettoParameter
    {
        private readonly string name;
        private readonly double value;
        private readonly double std;
        private readonly double lowerBorder;
        private readonly double upperBorder;

        public ParettoParameter(string name, double value, double std, double lowerBorder, double upperBorder)
        {
            this.name = name;
            this.value = value;
            this.std = std;
            this.lowerBorder = lowerBorder;
            this.upperBorder = upperBorder;
        }

        public double getValue()
        {
            return this.value;
        }

        public string Name => name;
        public string Value => value.ToString();
        public string StandartDeviation => Math.Round(std,3).ToString();
        public string CondifenceInterval
        {
            get
            {
                return $"{Math.Round(this.lowerBorder, 3)} ; {Math.Round(this.upperBorder, 3)}";
            }
        }
    }
}
