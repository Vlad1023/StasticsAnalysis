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
        private readonly IEnumerable<double> rawData;
        private readonly IEnumerable<ParettoParameter> distributionParams;
        private readonly PlotModel modelForRenewedDensityFunction;
        private readonly PlotModel modelForRenewedDistributionFunction;
        private readonly PlotModel modelForProbabilityPaper;
        private readonly OutcomeViewModel modelForDistributionInfoResult;
        public DistributionIdentifier(IEnumerable<DataFirstPoint> dataFirstPoints, IEnumerable<double> rawData, PlotModel histogramModel, double lengthOfClass)
        {
            this.dataFirstPoints = dataFirstPoints;
            this.rawData = rawData;
            this.nParameter = dataFirstPoints.Count() - 1;
            this.lengthOfClass = lengthOfClass;

            this.distributionParams = CalculateParams();
            this.modelForRenewedDensityFunction = CalculateModelForRenewedDensityFunction(histogramModel);
            this.modelForRenewedDistributionFunction = CalculateModelForRenewedDistributionFunction();
            this.modelForProbabilityPaper = CalculateModelForProbabilityPaper();
            this.modelForDistributionInfoResult = CalculateProbability();
        }

        public IEnumerable<ParettoParameter> DistributionParams => distributionParams;
        public PlotModel ModelForRenewedDensityFunction => modelForRenewedDensityFunction;
        public PlotModel ModelForRenewedDistributionFunction => modelForRenewedDistributionFunction;
        public PlotModel ModelForProbabilityPaper => modelForProbabilityPaper;
        public OutcomeViewModel DistributionResult => modelForDistributionInfoResult; 

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
            plotModel.Title = "Restored density function";
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
            plotModel.Title = "Restored distribution function";
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

        private PlotModel CalculateModelForProbabilityPaper()
        {
            PlotModel plotModel = new PlotModel();
            plotModel.Title = "Probability Paper";

            var xAxis = new OxyPlot.Axes.LinearAxis
            { Title = "t", Position = AxisPosition.Bottom, LabelFormatter = (param) => Math.Round(param, 3).ToString() };
            plotModel.Axes.Add(xAxis);

            var yAxis = new OxyPlot.Axes.LinearAxis
            { Title = "z", Position = AxisPosition.Left };
            plotModel.Axes.Add(yAxis);

            var scatterSeries = new ScatterSeries();

            for (int i = 0; i < this.dataFirstPoints.Count() - 1; i++)
            {
                scatterSeries.Points.Add(new ScatterPoint(getValueOnTAxis(this.dataFirstPoints.ElementAt(i).Value), getValueOnZAxis(this.dataFirstPoints.ElementAt(i).EmpFunctionResult), 1));
            }
            plotModel.Series.Add(scatterSeries);

            var linearSeries = new LineSeries();
            linearSeries.Title = "Restored distribution function";

            linearSeries.Points.Add(new DataPoint(getValueOnTAxis(this.dataFirstPoints.ElementAt(0).Value), 
                                    getValueOnZAxis(distributionFuncForParetto(this.dataFirstPoints.ElementAt(0).Value))));
            linearSeries.Points.Add(new DataPoint(getValueOnTAxis(this.dataFirstPoints.ElementAt(this.dataFirstPoints.Count() - 2).Value), 
                                    getValueOnZAxis(distributionFuncForParetto(this.dataFirstPoints.ElementAt(this.dataFirstPoints.Count() - 2).Value))));
            plotModel.Series.Add(linearSeries);

            return plotModel;
        }

        private OutcomeViewModel CalculateProbability()
        {
            var z = calculateZ();
            var kz = calculateKZ(z);

            var p = 1.0 - kz;

            if(p >= 0.5)
            {
                return new OutcomeViewModel(z, kz, p, "This distribution is probably the Paretto distribution");
            }
            return new OutcomeViewModel(z, kz, p, "This distribution is probably not the Paretto distribution");
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
            return ((this.nParameter - 1.0) / this.nParameter) * aBiasedParameter;
        }

        private double calculateK(double aParameter, double kBiased)
        {
            // return (1.0 - (1.0 / (this.nParameter - 1) * aParameter) ) * kBiased;
            return kBiased;
        }

        private double calculateStdForA()
        {
            return (double)(this.nParameter - 1) / this.nParameter;
        }
        private double calculateStdForK(double kBiased)
        {
            return kBiased * ((double)(1) / (this.nParameter - 1));
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

        private double getValueOnTAxis(double x)
        {
            return -Math.Log(x);
        }

        private double getValueOnZAxis(double empValue)
        {
            return Math.Log(1.0 - empValue);
        }

        private double calculateZ()
        {
            var dnPlustListOfValues = new List<double>();
            for (int i = 0; i < this.dataFirstPoints.Count(); i++)
            {
                var currentElement = this.dataFirstPoints.ElementAt(i);
                dnPlustListOfValues.Add(Math.Abs(currentElement.EmpFunctionResult - this.distributionFuncForParetto(currentElement.Value)));
            }
            var dnPlus = dnPlustListOfValues.Max();

            var dnMinusListOfValues = new List<double>();
            for (int i = 1; i < this.dataFirstPoints.Count(); i++)
            {
                var currentElement = this.dataFirstPoints.ElementAt(i);
                var previousElement = this.dataFirstPoints.ElementAt(i - 1);
                dnMinusListOfValues.Add(Math.Abs(previousElement.EmpFunctionResult - this.distributionFuncForParetto(currentElement.Value)));
            }
            var dnMinus = dnMinusListOfValues.Max();

            var dValuesList = new List<double> { dnMinus, dnPlus };
            return Math.Sqrt(this.rawData.Count() - 1) * dValuesList.Max();
        }

        private double calculateKZ(double z)
        {
            var rightParSum = 0.0;
            for (int i = 1; i < 30; i++)
            {
                rightParSum += Math.Pow(-1.0, i) * Math.Exp(-2.0 * i * i * z * z);
            }
            return 1.0 + 2.0 * rightParSum;
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

    public class OutcomeViewModel
    {
        private readonly double zValue;
        private readonly double kzValue;
        private readonly double pValue;
        private readonly string result;

        public OutcomeViewModel(double zValue, double kzValue, double pValue, string result)
        {
            this.zValue = zValue;
            this.kzValue = kzValue;
            this.pValue = pValue;
            this.result = result;
        }

        public string ZValue => $"z value - {Math.Round(this.zValue, 3)}";
        public string KZValue => $"K(z) value - {Math.Round(this.kzValue, 3)}";
        public string PValue => $"p value - {Math.Round(this.pValue, 3)}";
        public string Result => result;
    }
}
