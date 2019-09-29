using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Windows.Input;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel.ViewModelBases;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using LinearAxis = OxyPlot.Axes.LinearAxis;

namespace DiagramDesigner.ViewModel
{
    public class PlotVM
    {
        public ObservableCollection<PlotCheckItem> PlotsToShow { get; set; }
        public ObservableCollection<TypeRadioButton> TypeRadioButtons { get; set; }
        private TypeRadioButton _currentlySelectedTypeRadioButton;
        public LinearAxis YAxis { get; set; }
        public PlotModel PlotModel { get; set; } = new PlotModel
        {
            LegendPosition = LegendPosition.TopRight
        };

        public PlotVM()
        {
            PlotsToShow = new ObservableCollection<PlotCheckItem>();
            TypeRadioButtons = new ObservableCollection<TypeRadioButton>
            {
                new TypeRadioButton(DataPointListType.Module_dB,"Module [dB]",UpdateType,true),
                new TypeRadioButton(DataPointListType.Real,"Real",UpdateType,false),
                new TypeRadioButton(DataPointListType.Imaginary,"Imaginary",UpdateType,false),
                new TypeRadioButton(DataPointListType.Module,"Module",UpdateType,false),
                new TypeRadioButton(DataPointListType.Phase,"Phase",UpdateType,false),
            };

            PlotModel.Axes.Add(new LinearAxis
            {
                Title = "Frequency [GHz]",
                Position = AxisPosition.Bottom
            });

            _currentlySelectedTypeRadioButton = GetSelectedType(TypeRadioButtons);
            YAxis = new LinearAxis
            {
                Title = _currentlySelectedTypeRadioButton.Name,
                Position = AxisPosition.Left
            };
            PlotModel.Axes.Add(YAxis);
        }

        private ICommand _exportToPngCommand;
        public ICommand ExportToPngCommand => _exportToPngCommand ?? (_exportToPngCommand = new RelayCommand(ExportToPngExecute));

        private void ExportToPngExecute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "png File|*.png";
            saveFileDialog.Title = "Save an png File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {
                PngExporter.Export(PlotModel, saveFileDialog.FileName, 1000, 700, OxyColors.White);
            }
        }

        private ICommand _selectAllChanged;
        public ICommand SelectAllChanged => _selectAllChanged ?? (_selectAllChanged = new RelayCommand(SelectAllChangedExecute));

        private void SelectAllChangedExecute(object parameter)
        {
            if (parameter is bool isChecked)
            {
                if (isChecked)
                {
                    foreach (var plotCheckItem in PlotsToShow)
                    {
                        plotCheckItem.ShowWithoutRefresh();
                    }
                    PlotModel.InvalidatePlot(true); //Refresh
                }
                else
                {
                    foreach (var plotCheckItem in PlotsToShow)
                    {
                        plotCheckItem.HideWithoutRefresh();
                    }
                    PlotModel.InvalidatePlot(true); //Refresh
                }
            }
            else
            {
                throw new Exception("Parameter of SelectAllChangedExecute should be bool type");
            }
        }

        public void AddComplexChart(ComplexChart complexChart)
        {
            PlotsToShow.Add(new PlotCheckItem(true, PlotModel, complexChart, _currentlySelectedTypeRadioButton.Type));
        }

        public void RemoveComplexChart(ComplexChart complexChart)
        {
            foreach (PlotCheckItem plotCheckItem in PlotsToShow)
            {
                if (plotCheckItem.ComplexChart == complexChart)
                {
                    plotCheckItem.Detach();
                    PlotsToShow.Remove(plotCheckItem);
                    break;
                }
            }
        }

        public void ClearComplexCharts()
        {
            int plotCount = PlotsToShow.Count;
            for (int i = 0; i < plotCount; i++)
            {
                PlotCheckItem plotCheckItem = PlotsToShow.Last();
                plotCheckItem.Detach();
                PlotsToShow.Remove(plotCheckItem);
            }
        }

        public void UpdateType(TypeRadioButton typeRadioButton)
        {
            _currentlySelectedTypeRadioButton = typeRadioButton;
            foreach (PlotCheckItem plotCheckItem in PlotsToShow)
            {
                plotCheckItem.UdpateType(typeRadioButton.Type);
            }
            YAxis.Title = typeRadioButton.Name;
            PlotModel.InvalidatePlot(true); //refresh plot
        }

        TypeRadioButton GetSelectedType(IList<TypeRadioButton> typeRadioButtons)
        {
            foreach (TypeRadioButton typeRadioButton in typeRadioButtons)
            {
                if (typeRadioButton.IsChecked)
                    return typeRadioButton;
            }

            return null;
        }
    }
}