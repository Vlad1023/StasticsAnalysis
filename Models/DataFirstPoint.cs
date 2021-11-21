using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EMPI_Proj.Models
{
    public class DataFirstPoint : INotifyPropertyChanged
    {
        private readonly IEnumerable<double> rawDataDistinct;
        private readonly int index;
        private double value;
        private double frequency;
        private double relativeFrequency;
        private double empFunctionResult;

        public int Index
        {
            get { return index; }
        }

        public IEnumerable<double>  GetRawDataDistinct()
        {
            return rawDataDistinct;
        }

        public double Value
        {
            get { return value; }
            private set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }
        public double Frequency
        {
            get { return Math.Round(frequency,3); }
            private set
            {
                frequency = value;
                OnPropertyChanged("Frequency");
            }
        }
        public double RelativeFrequency
        {
            get { return Math.Round(relativeFrequency,3); }
            private set
            {
                relativeFrequency = value;
                OnPropertyChanged("RelativeFrequency");
            }
        }

        public double RelFreq()
        {
            return relativeFrequency;
        }

        public double EmpFunctionResult
        {
            get { return Math.Round(empFunctionResult,3); }
            private set
            {
                empFunctionResult = value;
                OnPropertyChanged("EmpFunctionResult");
            }
        }

        public DataFirstPoint(IEnumerable<double> rawData, IEnumerable<double> rawDataDistinct,  double previousEmpFunctionResult, int index)
        {
            this.rawDataDistinct = rawDataDistinct;
            this.index = index;
            this.value = rawDataDistinct.ElementAt(index);
            this.calculateInfo(previousEmpFunctionResult, rawData, rawDataDistinct);
        }

        private void calculateInfo(double previousEmpFunctionResult, IEnumerable<double> rawData, IEnumerable<double> rawDataDistinct)
        {
            var currentElement = rawDataDistinct.ElementAt(this.index);
            this.Frequency = rawData.Count(el => el == currentElement);
            this.RelativeFrequency = this.Frequency / (double)rawData.Count();
            this.EmpFunctionResult = this.RelativeFrequency + previousEmpFunctionResult;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}