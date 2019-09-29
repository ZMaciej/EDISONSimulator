using System;
using System.Collections.Generic;
using DiagramDesigner.Model;
using OxyPlot;

namespace DiagramDesigner.ViewModel
{
    public class DataPointListMaker
    {
        private ComplexChart _complexChart;
        public List<DataPoint> DataPointList { get; set; }

        public DataPointListMaker(ComplexChart complexChart, DataPointListType Type)
        {
            _complexChart = complexChart;
            DataPointList = new List<DataPoint>();
            MakeDataPointList(Type);
        }

        public void MakeDataPointList(DataPointListType Type)
        {
            switch (Type)
            {
                case DataPointListType.Module:
                    DataPointList = new List<DataPoint>();
                    for (int i = 0; i < _complexChart.Values.Count; i++)
                    {
                        DataPointList.Add(new DataPoint(_complexChart.Frequencies[i], _complexChart.Values[i].Magnitude));
                    }
                    break;
                case DataPointListType.Module_dB:
                    DataPointList = new List<DataPoint>();
                    for (int i = 0; i < _complexChart.Values.Count; i++)
                    {
                        DataPointList.Add(new DataPoint(_complexChart.Frequencies[i], 20 * Math.Log10(_complexChart.Values[i].Magnitude)));
                    }
                    break;
                case DataPointListType.Phase:
                    DataPointList = new List<DataPoint>();
                    for (int i = 0; i < _complexChart.Values.Count; i++)
                    {
                        DataPointList.Add(new DataPoint(_complexChart.Frequencies[i], _complexChart.Values[i].Phase));
                    }
                    break;
                case DataPointListType.Real:
                    DataPointList = new List<DataPoint>();
                    for (int i = 0; i < _complexChart.Values.Count; i++)
                    {
                        DataPointList.Add(new DataPoint(_complexChart.Frequencies[i], _complexChart.Values[i].Real));
                    }
                    break;
                case DataPointListType.Imaginary:
                    DataPointList = new List<DataPoint>();
                    for (int i = 0; i < _complexChart.Values.Count; i++)
                    {
                        DataPointList.Add(new DataPoint(_complexChart.Frequencies[i], _complexChart.Values[i].Imaginary));
                    }
                    break;
                default:
                    DataPointList = new List<DataPoint>();
                    for (int i = 0; i < _complexChart.Values.Count; i++)
                    {
                        DataPointList.Add(new DataPoint(_complexChart.Frequencies[i], 20 * Math.Log10(_complexChart.Values[i].Magnitude)));
                    }
                    break;
            }
        }
    }
}