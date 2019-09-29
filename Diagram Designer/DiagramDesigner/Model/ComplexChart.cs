using System.Collections.Generic;
using System.Numerics;

namespace DiagramDesigner.Model
{
    public class ComplexChart
    {
        public List<Complex> Values { get; }
        public List<double> Frequencies { get; }
        public string Name { get; }

        public ComplexChart(List<Complex> values, List<double> frequencies, string name)
        {
            Values = values;
            Frequencies = frequencies;
            Name = name;
        }
    }
}