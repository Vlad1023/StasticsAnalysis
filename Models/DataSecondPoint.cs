using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EMPI_Proj.Models
{
    class Class
    {
        public double? lowBorder { get; set; }
        public double? upperBorder { get; set; }
    }
    public class DataSecondPointWrapper
    {
        private IEnumerable<DataSecondPoint> dataSecondPoints;
        private readonly IEnumerable<DataFirstPoint> dataFirstPoints;
        private readonly int MParametr;
        private readonly double minValue;
        private readonly double lengthOfClass;
        private IEnumerable<Class> Classes;
        public DataSecondPointWrapper(int NumberOfIntervals, IEnumerable<DataFirstPoint> dataFirstPoints)
        {
            this.dataFirstPoints = dataFirstPoints;
            if (NumberOfIntervals <= 0)
                this.MParametr = Convert.ToInt32(1 + 3.32 * Math.Log10(dataFirstPoints.Count() - 1));
            else
                this.MParametr = NumberOfIntervals;
            this.minValue = this.dataFirstPoints.Select(el => el.Value).Min();
            this.lengthOfClass = (this.dataFirstPoints.Select(el => el.Value).Max() - this.dataFirstPoints.Select(el => el.Value).Min()) / this.MParametr;
            calculateBorders();
            calculateDataSecondPoints();
        }

        public double LengthOfClass => lengthOfClass;

        public IEnumerable<DataSecondPoint> DataSecondPoints => dataSecondPoints;

        private void calculateDataSecondPoints()
        {
            var dataSecondPoints = new List<DataSecondPoint>();
            double currentEmpFunctionResult = 0;
            for (int i = 0; i < this.Classes.Count(); i++)
            {
                double classFrequency;
                double relativeClassFrequency;
                Func<DataFirstPoint, bool> condition;

                var currentElement = this.Classes.ElementAt(i);

                if (i == this.Classes.Count() - 1)
                {
                    condition = el => el.Value >= currentElement.lowBorder && el.Value <= currentElement.upperBorder;
                }
                else
                {
                    condition = el => el.Value >= currentElement.lowBorder && el.Value < currentElement.upperBorder;
                }
                if (this.dataFirstPoints.Where(condition).Any())
                {
                    classFrequency = this.dataFirstPoints
                     .Where(condition)
                     .Select(el => el.Frequency)
                         .Aggregate((x, y) => x + y);
                    //relativeClassFrequency = this.dataFirstPoints
                    //  .Where(condition)
                    //  .Select(el => el.RelFreq())
                    //     .Aggregate((x, y) => x + y);
                    relativeClassFrequency = this.dataFirstPoints
                      .Where(condition)
                      .Select(el => el.RelFreq())
                         .Aggregate((x, y) => x + y);
                    currentEmpFunctionResult += relativeClassFrequency;
                }
                else
                {
                    classFrequency = 0;
                    relativeClassFrequency = 0;
                }


                var curDataSecondPont = new DataSecondPoint(i, this.dataFirstPoints.Count(), currentElement.lowBorder.Value, currentElement.upperBorder.Value,
                    classFrequency, relativeClassFrequency, currentEmpFunctionResult);
                dataSecondPoints.Add(curDataSecondPont);
            }
            this.dataSecondPoints = dataSecondPoints;
        }

        private void calculateBorders()
        {
            var classes = new List<Class>();
            Class curClass = new Class();
            for (int i = 0; i < MParametr; i++)
            {
                curClass = new Class { lowBorder = curClass.upperBorder ?? this.minValue };
                var upperBorder = this.minValue + this.lengthOfClass * (i + 1);
                if (upperBorder > this.dataFirstPoints.Max(el => el.Value))
                    curClass.upperBorder = this.dataFirstPoints.Max(el => el.Value);
                else
                    curClass.upperBorder = upperBorder;
                classes.Add(curClass);
            }
            this.Classes = classes;
        }
    }
    public class DataSecondPoint
    {
        public readonly double lowBorder;
        public readonly double upperBorder;


        private readonly int totalNumber;
        private readonly int index;
        private readonly double frequency;
        private readonly double relativeFrequency;
        private readonly double empFunctionResult;

        public DataSecondPoint(int index, int totalNumber, double lowBorder, double upperBorder, double frequency, double relativeFrequency, double empFunctionResult)
        {
            this.index = index;
            this.lowBorder = lowBorder;
            this.upperBorder = upperBorder;
            this.frequency = frequency;
            this.totalNumber = totalNumber;
            this.relativeFrequency = relativeFrequency;
            this.empFunctionResult = empFunctionResult;
        }

        public int Index
        {
            get { return index; }
        }

        public string Values => String.Format($"({Math.Round(lowBorder,3)} ; {Math.Round(upperBorder,3)})");

        public double Frequency
        {
            get { return Math.Round(frequency,3); }
        }

        public double RelativeFrequency
        {
            get { return Math.Round(relativeFrequency,3); }
        }

        public double EmpFunctionResult
        {
            get { return Math.Round(empFunctionResult,3); }
        }
    }
}
