using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using SciChart.Examples.ExternalDependencies.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EMPI_Proj
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            mainWindowViewModel = (MainWindowViewModel)DataContext;
            mainWindowViewModel.PropertyChanged += (sender, e) => BuildGraphFirstPoint(sender, e);
        }
        private void BuildGraphFirstPoint(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName.Equals("IsSecondPointVisible") && mainWindowViewModel.IsSecondPointVisible == Visibility.Visible)
            {
                DoubleSeries data = new DoubleSeries();
                var empFunctionResult = this.mainWindowViewModel.DataFirstPoints.Select(s => s.EmpFunctionResult);
                var values = this.mainWindowViewModel.DataFirstPoints.Select(s => s.Value);

                for (int i = 0; i < values.Count(); i++)
                {
                    data.Add(new XYPoint { X = this.mainWindowViewModel.DataFirstPoints.ElementAt(i).Value, Y = empFunctionResult.ElementAt(i) });
                }
                var dataSeries = new XyDataSeries<double, double>();
                // Append data to series. SciChart automatically redraws
                dataSeries.Append(data.XData, data.YData);

                lineRenderSeries.DataSeries = dataSeries;

                // We set visible ranges only to zoom in to series to show Digital Line
                //sciChart.XAxis.VisibleRange = new DoubleRange(1, 1.25);
                //sciChart.YAxis.VisibleRange = new DoubleRange(2.3, 3.3);
            }

        }

    }
}
