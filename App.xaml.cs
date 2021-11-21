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
            SciChartSurface.SetRuntimeLicenseKey("V/HU/EegcKubvkRGaOOV3S1u3Sa8QmeHLHQr297mKHNbydw84yLYcDK99WFGk+uG7wIZEjsbK65wk0VpCGo0+r5UippkCwewYbMVNA7FLUZ1Lvc4Xf+Dhbvuj+wdhHbXhGTgkjhZXs+XuGLS8K8KDxDz6FzvEJ49lmrQIj6K9zhfTDu6DR0qijZa8YTv0tscost3hNRaPPoK5y6mBRbJRMFaD3ydt/toH8geVXqE+hWQP0srqSkzHioidS81SjIsj2tbXyhU8E5YRLocBYrIDc6pl9EyVOJzz+N7/d1rAZF+UXNeVg3JGbwph4gM+Z66PAKt6XtmRfPlydDfm9oiIbOzgBZ24mO/nh5LxDW+jJSmMNc1HQh8drhB5blDl7BoDSzYi/dhaITTRT/ImwzIkQWN9QmFKxfmYmDjjSriRmtgCMbRIVqgT29gLgw39RcH9auh+PFRtDuZ6oMhr4OVH9122/tTko/W9ZpqoN8g4Cb2AX5rdYsAqPCgP4yN1DzHQdIISmHyU/Nb2rbzg37Z/9I758OLlZSipRLUK+sTGGkRPJNrqgT64LktD8B4wxXyXBU=");
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
