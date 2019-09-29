using System.Collections.Generic;
using System.Windows.Input;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel.ViewModelBases;
using OxyPlot;
using OxyPlot.Series;

namespace DiagramDesigner.ViewModel
{
    public class PlotCheckItem : NotifyPropertyChangedBase
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        private readonly PlotModel _plotModel;
        private readonly LineSeries _lineSeries = new LineSeries();
        public List<DataPoint> Points { get; set; }
        private readonly DataPointListMaker _dataPointListMaker;
        public ComplexChart ComplexChart { get; }

        public PlotCheckItem(bool isSelected, PlotModel plotModel, ComplexChart complexChart, DataPointListType type)
        {
            Name = complexChart.Name;
            _lineSeries.Title = Name;
            IsSelected = isSelected;
            this._plotModel = plotModel;
            _dataPointListMaker = new DataPointListMaker(complexChart, type);
            ComplexChart = complexChart;
            CheckBoxChanged(isSelected);
        }

        public void Detach()
        {
            _plotModel.Series.Remove(_lineSeries);
            RefreshPlot();
        }

        public void UdpateType(DataPointListType type)
        {
            _dataPointListMaker.MakeDataPointList(type);
            Points = _dataPointListMaker.DataPointList;
            _lineSeries.ItemsSource = Points;
            RefreshPlot();
        }

        private ICommand _changed;
        public ICommand Changed => _changed ?? (_changed = new RelayCommand(CheckBoxChanged));

        private void CheckBoxChanged(object parameter)
        {
            if (IsSelected)
            {
                Points = _dataPointListMaker.DataPointList;
                if (_lineSeries.ItemsSource != Points)
                    _lineSeries.ItemsSource = Points;
                _plotModel.Series.Add(_lineSeries);
            }
            else
            {
                _plotModel.Series.Remove(_lineSeries);
            }
            RefreshPlot();
        }

        public void ShowWithoutRefresh()
        {
            Points = _dataPointListMaker.DataPointList;
            if (_lineSeries.ItemsSource != Points)
            {
                _lineSeries.ItemsSource = Points;
            }
            if (!_plotModel.Series.Contains(_lineSeries))
            {
                _plotModel.Series.Add(_lineSeries);
            }

            IsSelected = true;
            OnPropertyChanged(nameof(IsSelected));
        }

        public void HideWithoutRefresh()
        {
            _plotModel.Series.Remove(_lineSeries);
            IsSelected = false;
            OnPropertyChanged(nameof(IsSelected));
        }

        private void RefreshPlot()
        {
            _plotModel.InvalidatePlot(true);
        }
    }
}