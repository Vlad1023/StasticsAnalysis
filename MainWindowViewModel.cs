using EMPI_Proj.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.Threading.Tasks;
using System.IO;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using SciChart.Examples.ExternalDependencies.Data;
using System.Windows.Controls;
using Extreme.Statistics;
using Extreme.Mathematics;

namespace EMPI_Proj
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private IEnumerable<double> rawData;
        private IEnumerable<DataFirstPoint> dataFirstPoints;
        private IEnumerable<DataSecondPoint> dataSecondPoints;
        private IEnumerable<DataThirdPointViewModel> dataParameters;

        private IEnumerable<AnomalyPointViewModel> anomalies;
        private DistributionIdentifier distributionIdentifier;

        private PlotModel scatterModel;
        private double errorForAnomalies = 0.01;

        private Visibility _isFirstPointVisible = Visibility.Visible;
        private Visibility _isSecondPointVisible = Visibility.Collapsed;
        private Visibility _isThirdPointVisible = Visibility.Collapsed;
        private Visibility _isFourthPointVisible = Visibility.Collapsed;
        private Visibility _isFivthPointVisible = Visibility.Collapsed;
        private Visibility _isSixPointVisible = Visibility.Collapsed;
        private PlotModel histogramModel;
        private string numberOfClasses;
        private string width;
        private string fileName = "Null";
        private double currentLengthOfClass;

        public MainWindowViewModel()
        {
            LoadDataCommand = new RelayCommand(loadDate);
            OpenFirstPointDataCommand = new RelayCommand(LoadFirstPoint);
            OpenSecondPointDataCommand = new RelayCommand(LoadSecondPoint);
            OpenThirdPointDataCommand = new RelayCommand(LoadThirdPoint);
            OpenFourthPointDataCommand = new RelayCommand(LoadFourthPoint);
            OpenFifthPointDataCommand = new RelayCommand(LoadFifthPoint);
            DeleteAnomaliesDataCommand = new RelayCommand(DeleteAnomalies);
            OpenSixPointDataCommand = new RelayCommand(LoadSixPoint);
            HandleRawDataProcess();

        }
        public ICommand LoadDataCommand { get; set; }
        public ICommand OpenFirstPointDataCommand { get; set; }
        public ICommand OpenSecondPointDataCommand { get; set; }
        public ICommand OpenThirdPointDataCommand { get; set; }
        public ICommand OpenFourthPointDataCommand { get; set; }
        public ICommand OpenFifthPointDataCommand { get; set; }
        public ICommand DeleteAnomaliesDataCommand { get; set; }
        public ICommand OpenSixPointDataCommand { get; set; }
        public IEnumerable<DataFirstPoint> DataFirstPoints
        {
            get { return dataFirstPoints; }
            private set
            {
                dataFirstPoints = value.OrderBy(el => el.Value).ToList();
                if (dataFirstPoints.Count() > 0)
                {
                    var DataSecondPointsWrapper = new DataSecondPointWrapper(Convert.ToInt32(numberOfClasses), dataFirstPoints);
                    this.currentLengthOfClass = DataSecondPointsWrapper.LengthOfClass;

                    DataSecondPoints = DataSecondPointsWrapper.DataSecondPoints;
                    DataParameters = new DataThirdPoint(dataFirstPoints).DataThirdPoints;

                    var dataFourthPoint = new DataFourthPoint(dataFirstPoints, errorForAnomalies);

                    AnomalyPointsViewModel = dataFourthPoint.Anomalies;
                    ScatterAllDataModel = dataFourthPoint.ScatterPlotOfData;

                    DistributionIdentifier = new DistributionIdentifier(dataFirstPoints, rawData, this.histogramModel, this.currentLengthOfClass);
                }
                OnPropertyChanged("DataFirstPoints");
            }
        }

        public string FileName
        {
            get { return fileName; }
            private set
            {
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        public PlotModel HistogramModel
        {
            get { return histogramModel; }
            private set
            {
                histogramModel = value;
                histogramModel.InvalidatePlot(true);
                OnPropertyChanged("HistogramModel");
            }
        }

        public IEnumerable<DataSecondPoint> DataSecondPoints
        {
            get { return dataSecondPoints; }
            private set
            {
                dataSecondPoints = value;
                this.numberOfClasses = dataSecondPoints.Count().ToString();
                HistogramModel = CreateClassesChart();
                OnPropertyChanged("DataSecondPoints");
            }
        }

        public IEnumerable<DataThirdPointViewModel> DataParameters
        {
            get { return dataParameters; }
            private set
            {
                dataParameters = value;
                OnPropertyChanged("DataParameters");
            }
        }

        public PlotModel ScatterAllDataModel
        {
            get { return scatterModel; }
            private set
            {
                scatterModel = value;
                scatterModel.InvalidatePlot(true);
                OnPropertyChanged("ScatterAllDataModel");
            }
        }

        public IEnumerable<AnomalyPointViewModel> AnomalyPointsViewModel
        {
            get { return anomalies; }
            private set
            {
                anomalies = value;
                OnPropertyChanged("AnomalyPointsViewModel");
            }
        }

        public DistributionIdentifier DistributionIdentifier
        {
            get { return distributionIdentifier; }
            private set
            {
                distributionIdentifier = value;
                OnPropertyChanged("DistributionIdentifier");
            }
        }

        public double ErrorForAnomalies
        {
            get { return errorForAnomalies; }
            set
            {
                errorForAnomalies = value;
                if (errorForAnomalies <= 0)
                {
                    errorForAnomalies = 0.01;
                }
                var dataFourthPoint = new DataFourthPoint(dataFirstPoints, errorForAnomalies);

                AnomalyPointsViewModel = dataFourthPoint.Anomalies;
                ScatterAllDataModel = dataFourthPoint.ScatterPlotOfData;
                OnPropertyChanged("ErrorForAnomalies");
            }
        }

        public PlotModel CreateClassesChart()
        {
            var plotModel = new PlotModel();
            var xAxis = new OxyPlot.Axes.LinearAxis
            { Title = "classRange", Position = AxisPosition.Bottom, LabelFormatter = (param) => Math.Round(param, 3).ToString() };
            plotModel.Axes.Add(xAxis);

            var yAxis = new OxyPlot.Axes.LinearAxis
            { Title = "RelativeFreq", Position = AxisPosition.Left };
            plotModel.Axes.Add(yAxis);

            var barSeries = new RectangleBarSeries();
            //var random = new Random();
            foreach (var item in this.dataSecondPoints)
            {
                //barSeries.Items.Add(new RectangleBarItem(item.lowBorder, item.RelativeFrequency, item.upperBorder, item.RelativeFrequency));
                barSeries.Items.Add(new RectangleBarItem(item.lowBorder, 0, item.upperBorder, item.RelativeFrequency));
            }
            plotModel.Series.Add(barSeries);
            AddKDEToPlotModel(plotModel);
            return plotModel;
        }

        public void DeleteAnomalies()
        {
            if (this.AnomalyPointsViewModel != null)
            {
                  var rawData = DataFirstPoints.ElementAt(0).GetRawDataDistinct();

                HandleRawDataProcess(rawData.Where(p => !AnomalyPointsViewModel.Any(p2 => Convert.ToDouble(p2.Value) == p)).ToList());
            }
        }

        private void AddKDEToPlotModel(PlotModel plotModel)
        {
            var widthDouble = Convert.ToDouble(this.width);
            var lineSeries = new OxyPlot.Series.LineSeries();
            var allData = this.dataFirstPoints.Select(el => el.Value).ToList();
            var allDataVector = Extreme.Mathematics.Vector.Create((IList<double>)allData, ArrayMutability.MutableValues);
            var bwSilverman = KernelDensity.EstimateBandwidth(allDataVector, KernelDensity.GaussianKernel,
                KernelDensityBandwidthEstimator.Scott);
            for (int i = 0; i < this.dataFirstPoints.Count(); i++)
            {
                var currentElement = this.dataFirstPoints.ElementAt(i);
                var currentPoint = currentElement.Value;
                if (widthDouble <= 0)
                    this.width = bwSilverman.ToString();
                //var density = KernelDensity.Estimate(allDataVector, KernelDensity.UniformKernel, value: currentPoint, bandwidth: widthDouble > 0 ? widthDouble : bwSilverman);
                var density = KernelDensity.Estimate(allDataVector, KernelDensity.GaussianKernel, currentPoint, bandwidth: widthDouble <= 0 ? bwSilverman : widthDouble);
                lineSeries.Points.Add(new DataPoint(currentPoint, density * this.currentLengthOfClass));
            }
            plotModel.Series.Add(lineSeries);
        }
        public string NumberOfClasses
        {
            get { return this.numberOfClasses; }
            set
            {
                this.numberOfClasses = value;
                int numberOfClassesInt;
                if (this.numberOfClasses != String.Empty && Convert.ToInt32(this.numberOfClasses) > 0)
                {
                    numberOfClassesInt = Convert.ToInt32(this.numberOfClasses);
                    this.DataSecondPoints = new DataSecondPointWrapper(numberOfClassesInt, this.dataFirstPoints).DataSecondPoints;
                }
                else
                {
                    this.DataSecondPoints = new DataSecondPointWrapper(0, this.dataFirstPoints).DataSecondPoints;
                    this.numberOfClasses = this.DataSecondPoints.Count().ToString();
                }
                OnPropertyChanged("NumberOfClasses");
            }
        }

        public string Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                this.NumberOfClasses = this.numberOfClasses;
                OnPropertyChanged("Width");
            }
        }

        public Visibility IsFirstPointVisible
        {
            get { return _isFirstPointVisible; }
            set
            {
                _isFirstPointVisible = value;
                OnPropertyChanged("IsFirstPointVisible");
            }
        }

        public Visibility IsSecondPointVisible
        {
            get { return _isSecondPointVisible; }
            set
            {
                _isSecondPointVisible = value;
                OnPropertyChanged("IsSecondPointVisible");
            }
        }

        public Visibility IsThirdPointVisible
        {
            get { return _isThirdPointVisible; }
            set
            {
                _isThirdPointVisible = value;
                OnPropertyChanged("IsThirdPointVisible");
            }
        }

        public Visibility IsFourthPointVisible
        {
            get { return _isFourthPointVisible; }
            set
            {
                _isFourthPointVisible = value;
                OnPropertyChanged("IsFourthPointVisible");
            }
        }

        public Visibility IsFivthPointVisible
        {
            get { return _isFivthPointVisible; }
            set
            {
                _isFivthPointVisible = value;
                OnPropertyChanged("IsFivthPointVisible");
            }
        }

        public Visibility IsSixPointVisible
        {
            get { return _isSixPointVisible; }
            set
            {
                _isSixPointVisible = value;
                OnPropertyChanged("IsSixPointVisible");
            }
        }

        public void LoadFirstPoint()
        {
            IsFirstPointVisible = Visibility.Visible;
            IsSecondPointVisible = Visibility.Collapsed;
            IsThirdPointVisible = Visibility.Collapsed;
            IsFourthPointVisible = Visibility.Collapsed;
            IsFivthPointVisible = Visibility.Collapsed;
            IsSixPointVisible = Visibility.Collapsed;
        }

        public void LoadSecondPoint()
        {
            IsFirstPointVisible = Visibility.Collapsed;
            IsSecondPointVisible = Visibility.Visible;
            IsThirdPointVisible = Visibility.Collapsed;
            IsFourthPointVisible = Visibility.Collapsed;
            IsFivthPointVisible = Visibility.Collapsed;
            IsSixPointVisible = Visibility.Collapsed;
        }

        public void LoadThirdPoint()
        {
            IsFirstPointVisible = Visibility.Collapsed;
            IsSecondPointVisible = Visibility.Collapsed;
            IsThirdPointVisible = Visibility.Visible;
            this.NumberOfClasses = "0";
            this.Width = "0";
            IsFourthPointVisible = Visibility.Collapsed;
            IsFivthPointVisible = Visibility.Collapsed;
            IsSixPointVisible = Visibility.Collapsed;
        }

        public void LoadFourthPoint()
        {
            IsFirstPointVisible = Visibility.Collapsed;
            IsSecondPointVisible = Visibility.Collapsed;
            IsThirdPointVisible = Visibility.Collapsed;
            IsFourthPointVisible = Visibility.Visible;
            IsFivthPointVisible = Visibility.Collapsed;
            IsSixPointVisible = Visibility.Collapsed;
        }

        public void LoadFifthPoint()
        {
            IsFirstPointVisible = Visibility.Collapsed;
            IsSecondPointVisible = Visibility.Collapsed;
            IsThirdPointVisible = Visibility.Collapsed;
            IsFourthPointVisible = Visibility.Collapsed;
            IsFivthPointVisible = Visibility.Visible;
            IsSixPointVisible = Visibility.Collapsed;
        }

        public void LoadSixPoint()
        {
            IsFirstPointVisible = Visibility.Collapsed;
            IsSecondPointVisible = Visibility.Collapsed;
            IsThirdPointVisible = Visibility.Collapsed;
            IsFourthPointVisible = Visibility.Collapsed;
            IsFivthPointVisible = Visibility.Collapsed;
            IsSixPointVisible = Visibility.Visible;
        }

        private void HandleRawDataProcess()
        {
            var OpenFileDialog = new OpenFileDialog();
            this.width = "0";
            this.numberOfClasses = "0";
            if (OpenFileDialog.ShowDialog() == true)
            {
                var rawData = proceedFile(OpenFileDialog).OrderBy(el => el).ToList();
                this.rawData = rawData;
                var rawDataDistinct = rawData.Distinct().ToList();
                var dataFirstPoints = new List<DataFirstPoint>();
                for (int index = 0; index < rawDataDistinct.Count; index++)
                {
                    var previousEmpFunctionResult = dataFirstPoints.Count() == 0 ?
                        0 :
                        dataFirstPoints.ElementAt(index - 1).EmpFunctionResult;
                    var currentPoint = new DataFirstPoint(rawData, rawDataDistinct, previousEmpFunctionResult, index);
                    dataFirstPoints.Add(currentPoint);
                }
                this.DataFirstPoints = dataFirstPoints;
            }
        }

        private void HandleRawDataProcess(IEnumerable<double> rawData)
        {
            var OpenFileDialog = new OpenFileDialog();
            this.rawData = rawData;
            this.width = "0";
            this.numberOfClasses = "0";
            var rawDataDistinct = rawData.Distinct().ToList();
            var dataFirstPoints = new List<DataFirstPoint>();
            for (int index = 0; index < rawDataDistinct.Count(); index++)
            {
                var previousEmpFunctionResult = dataFirstPoints.Count() == 0 ?
                    0 :
                    dataFirstPoints.ElementAt(index - 1).EmpFunctionResult;
                var currentPoint = new DataFirstPoint(rawData, rawDataDistinct, previousEmpFunctionResult, index);
                dataFirstPoints.Add(currentPoint);
            }
            this.DataFirstPoints = dataFirstPoints;
        }

        private void loadDate()
        {
            HandleRawDataProcess();
            LoadFirstPoint();
        }

        //private IEnumerable<double> proceedFile(OpenFileDialog OpenFileDialog)
        //{
        //    var rawDataStrings = File.ReadAllLines(OpenFileDialog.FileName).ToList();
        //    return rawDataStrings.Select(s => s.Replace('.', ',')).Select(el => double.Parse(el));
        //}
        private IEnumerable<double> proceedFile(OpenFileDialog OpenFileDialog)
        {
            List<double> result = new List<double>();
            var fullPath = OpenFileDialog.FileName;
            FileName = fullPath;
            var rawDataStrings = File.ReadAllLines(fullPath).ToList();
            if (rawDataStrings.ElementAt(0).Split(" ").Length > 0)
            {
                List<string> listforStringsFromDAT = new List<string>();
                foreach (var item in rawDataStrings)
                {
                    var splitedValues = item.Split(" ");
                    foreach (var subitem in splitedValues)
                    {
                        listforStringsFromDAT.Add(subitem);
                    }
                }
                return listforStringsFromDAT.Select(s => s.Replace('.', ',')).Select(el => double.Parse(el));
            }
            return rawDataStrings.Select(s => s.Replace('.', ',')).Select(el => double.Parse(el));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
