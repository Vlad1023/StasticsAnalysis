using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SciChart.Charting.Visuals;
using SciChart.Examples.ExternalDependencies.Controls.ExceptionView;

namespace EMPI_Proj
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
           //DispatcherUnhandledException += App_DispatcherUnhandledException;

            InitializeComponent();
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("9ljUOSSE7uWHQVtpsmr0XmjDA1AtaAY7ic8kwDkM6b6Hy7NMU8wIk7lWs0RwmIAAVTtT9/dTiBpqD8AV98LbHfcRq6miiECXlXDR3Qze37dOge03OsB4nCBdZXfc6F3nLaMgqH8IXPzukL1ChBpYXcM4YmIwjLzeK9OSIf6zX5/Eru6V8RBQGfsjeUQ7QoU4vZkyeY0BqSBp7FdG3Ans/jTV4Fd0BY6TtHFgSRDKNT1Jt613N9LDzZ7GP0QgaqEexF0O4hzCiuXYSqcqoAKn+rvcfAwaVtaOKT8g3g9XhnKCn0z0w6tNkrxgMtrKcTobEE7jR2S/fC/d6KDFxtP9AhJ9YpcfCTnnxwusUa4l5ABAgct1NJhddHeY1M3Tglm44rTY6cK0j41/xYfiA5WlFsRQN3QvxBWdlwzarFHVSJnQtYTcvvjlJw5/0Ev4EemHToQUvb7HeSTb5HU7szqzefK931NPJAR7WcIVHj8BhAqaxHfvaSkbz64L0hJLK/kRX9F+GSDNuKpQ4D6U6Z68GSRkRf8DUcCs+7IGrtcpPwVSZEjG0dmCsG0sY1Nyc1dvNq2AetR3QZ8l");
        }

        //private void App_DispatcherUnhandledException(object sender,
        //    System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    var exceptionView = new ExceptionView(e.Exception)
        //    {
        //        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        //    };
        //    exceptionView.ShowDialog();

        //    e.Handled = true;
        //}
    }
}
