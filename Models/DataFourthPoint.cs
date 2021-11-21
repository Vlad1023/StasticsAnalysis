using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace EMPI_Proj.Models
{
    public class DataFourthPoint
    {
        private readonly double avgValue;
        private readonly double lengthOfDataset;
        private double lowBorder;
        private double upperBorder;
        public PlotModel ScatterPlotOfData { get; private set; }
        public IEnumerable<AnomalyPointViewModel> Anomalies { get; private set; }
        public DataFourthPoint(IEnumerable<DataFirstPoint> dataFirstPoints, double error)
        {
            this.avgValue = dataFirstPoints.Select(el => el.Value).Average();
            this.lengthOfDataset = dataFirstPoints.Count();

            this.Anomalies = CalculateAnomalies(dataFirstPoints, error);
            var normalPoints = dataFirstPoints.Where(p => !Anomalies.Any(p2 => Convert.ToInt32(p2.Index) == p.Index)).ToList();
            if (this.Anomalies.Any())
                this.ScatterPlotOfData = CalculateScatterModel(normalPoints, this.Anomalies);
            else
                this.ScatterPlotOfData = CalculateScatterModel(normalPoints);
        }

        private IEnumerable<AnomalyPointViewModel> CalculateAnomalies(IEnumerable<DataFirstPoint> dataFirstPoints, double error)
        {
            var quantil = MathHelper.GetQuantilOfNormalDistribution(1 - (error / 2.0));
            lowBorder = avgValue - quantil * StandartDeviationOfDataset(dataFirstPoints);
            upperBorder = avgValue + quantil * StandartDeviationOfDataset(dataFirstPoints);

            List<AnomalyPointViewModel> anomalies = new List<AnomalyPointViewModel>();

            for (int i = 0; i < dataFirstPoints.Count(); i++)
            {
                if (dataFirstPoints.ElementAt(i).Value > upperBorder || dataFirstPoints.ElementAt(i).Value < lowBorder)
                    anomalies.Add(new AnomalyPointViewModel(i, dataFirstPoints.ElementAt(i).Value));
            }
            return anomalies;
        }

        private PlotModel CalculateScatterModel(IEnumerable<DataFirstPoint> normalPoints, IEnumerable<AnomalyPointViewModel> anomalies = null)
        {
            PlotModel scatterModel = new PlotModel();
            var xAxis = new OxyPlot.Axes.LinearAxis
            { Title = "index", Position = AxisPosition.Bottom, LabelFormatter = (param) => param.ToString() };
            scatterModel.Axes.Add(xAxis);

            var yAxis = new OxyPlot.Axes.LinearAxis
            { Title = "value", Position = AxisPosition.Left};
            scatterModel.Axes.Add(yAxis);

            ScatterSeries normalScatterSeries = new ScatterSeries() { MarkerFill = OxyColor.Parse("#0000FF") };

            for (int i = 0; i < normalPoints.Count(); i++)
            {
                normalScatterSeries.Points.Add(new ScatterPoint(i, normalPoints.ElementAt(i).Value, size: 2));
            }

            scatterModel.Series.Add(normalScatterSeries);

            if(anomalies != null)
            {
                ScatterSeries anomaliesScatterSeries = new ScatterSeries() { MarkerFill = OxyColor.Parse("#FF0000") };

                foreach (var item in anomalies)
                {
                    anomaliesScatterSeries.Points.Add(new ScatterPoint(Convert.ToInt32(item.Index), Convert.ToDouble(item.Value), size: 2));
                }
                scatterModel.Series.Add(anomaliesScatterSeries);
            }

            LineAnnotation LineUpper = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                StrokeThickness = 1,
                Color = OxyColors.Green,
                Y = upperBorder
            };
            LineAnnotation LineLow = new LineAnnotation()
            {
                Type = LineAnnotationType.Horizontal,
                StrokeThickness = 1,
                Color = OxyColors.Green,
                Y = lowBorder
            };
            scatterModel.Annotations.Add(LineUpper);

            scatterModel.Annotations.Add(LineLow);
            return scatterModel;
        }

        private double StandartDeviationOfDataset(IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            double upperPart = 0;
            foreach (var item in dataFirstPoints)
            {
                upperPart += Math.Pow(item.Value - this.avgValue, 2);
            }
            return Math.Sqrt(upperPart / (this.lengthOfDataset - 1));
        }

    }
    public class AnomalyPointViewModel
    {
        private readonly int index;
        private readonly double value;
        public AnomalyPointViewModel(int index, double value)
        {
            this.index = index;
            this.value = value;
        }
        public string Index => index.ToString();
        public string Value => value.ToString();
    }
}
