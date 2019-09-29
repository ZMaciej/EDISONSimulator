using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes;
using DiagramDesigner.Model;
using DiagramDesigner.sNpFile;
using DiagramDesigner.ViewModel.ViewModelBases;
using Microsoft.Win32;
using Element = DiagramDesigner.Model.Element;

namespace DiagramDesigner.ViewModel
{
    public class MainVM : NotifyPropertyChangedBase
    {
        public MainVM()
        {
            Project.Elements.CollectionChanged += OnElementsCollectionChanged;
            Project.ConnectionModels.CollectionChanged += OnConnectionModelsCollectionChanged;
            Project.ComplexCharts.CollectionChanged += OnComplexChartsCollectionChanged;
            Project.PropertyChanged += OnProjectPropertyChanged;
        }

        private void OnProjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Project.ReferenceImpedance))
                OnPropertyChanged(nameof(ReferenceImpedance));
            if (e.PropertyName == nameof(Project.FromFreq))
                OnPropertyChanged(nameof(FromFreq));
            if (e.PropertyName == nameof(Project.ToFreq))
                OnPropertyChanged(nameof(ToFreq));
            if (e.PropertyName == nameof(Project.Step))
                OnPropertyChanged(nameof(Step));
        }

        private void OnComplexChartsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ComplexChart complexChart in e.NewItems)
                {
                    PlotVM.AddComplexChart(complexChart);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (ComplexChart complexChart in e.OldItems)
                {
                    PlotVM.RemoveComplexChart(complexChart);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                PlotVM.ClearComplexCharts();
            }
        }

        public MainModel Project { get; } = new MainModel();
        public ObservableCollection<ElementVM> ElementsVM { get; } = new ObservableCollection<ElementVM>();
        public ObservableCollection<ConnectionVM> ConnectionsVM { get; } = new ObservableCollection<ConnectionVM>();
        public PlotVM PlotVM { get; } = new PlotVM();
        private ICommand _simulate;
        public ICommand Simulate => _simulate ?? (_simulate = new RelayCommand(SimulateExecute));

        #region Simulation Parameters

        public string ReferenceImpedance
        {
            get => Project.ReferenceImpedance.ToString(new CultureInfo("en-US", false));
            set => Project.ReferenceImpedance = double.Parse(value, new CultureInfo("en-US", false));
        }

        public string FromFreq
        {
            get => (Project.FromFreq / 1000000000).ToString(new CultureInfo("en-US", false));
            set => Project.FromFreq = double.Parse(value, new CultureInfo("en-US", false)) * 1000000000;
        }
        public string ToFreq
        {
            get => (Project.ToFreq / 1000000000).ToString(new CultureInfo("en-US", false));
            set => Project.ToFreq = double.Parse(value, new CultureInfo("en-US", false)) * 1000000000;
        }
        public string Step
        {
            get => (Project.Step / 1000000000).ToString(new CultureInfo("en-US", false));
            set => Project.Step = double.Parse(value, new CultureInfo("en-US", false)) * 1000000000;
        }

        #endregion

        private void SimulateExecute(object parameter)
        {
            Project.Simulate();
        }

        #region Observing the ElementModel Collection

        public ObservableCollection<Element> Elements => Project.Elements;

        private void OnElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Element newElement in e.NewItems)
                {
                    ElementVM newElementVM = Creator.CreateElementVMByElement(newElement);
                    ElementsVM.Add(newElementVM);
                    newElementVM.OnMove += AutoConnectDelegate;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Element removedElement in e.OldItems)
                {
                    ElementVM removedElementVM = GetElementVMByGuid(removedElement.ID);
                    foreach (ConnectorVM connectorVM in removedElementVM.ConnectorsVM)
                    {
                        connectorVM.Detach();
                    }
                    removedElementVM.Detach();
                    removedElementVM.OnMove = null;
                    if (ElementsVM.Contains(removedElementVM)) ElementsVM.Remove(removedElementVM);
                }
            }
        }

        #endregion

        #region Observing the ConnectionModel Collection

        public ObservableCollection<ConnectionModel> ConnectionModels => Project.ConnectionModels;

        private void OnConnectionModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ConnectionModel newConnectionModel in e.NewItems)
                {
                    ConnectorVM SinkVM = GetConnectorVMByGuid(newConnectionModel.SinkConnector.ID);
                    ConnectorVM SourceVM = GetConnectorVMByGuid(newConnectionModel.SourceConnector.ID);
                    ConnectionVM newConnectionVM = new ConnectionVM(SinkVM, SourceVM, this);
                    SinkVM.ConnectionsVM.Add(newConnectionVM);
                    SourceVM.ConnectionsVM.Add(newConnectionVM);
                    newConnectionVM.ConnectionModel = newConnectionModel;
                    ConnectionsVM.Add(newConnectionVM);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ConnectionModel removedConnectionModel in e.OldItems)
                {
                    ConnectionVM removedConnectionVM = GetConnectionVMByGuid(removedConnectionModel.ID);
                    removedConnectionVM.SinkVM.ConnectionsVM.Remove(removedConnectionVM);
                    removedConnectionVM.SourceVM.ConnectionsVM.Remove(removedConnectionVM);
                    removedConnectionVM.Detach();
                    if (ConnectionsVM.Contains(removedConnectionVM)) ConnectionsVM.Remove(removedConnectionVM);
                }
            }
        }

        #endregion

        #region SelectedElement property

        /// <summary>
        /// SelectedElement property is used to show and edit Element properties in PropertyGrid
        /// </summary>
        private Object _selectedElement;
        public Object SelectedElement
        {
            get => _selectedElement;
            set
            {
                if (_selectedElement != value)
                {
                    _selectedElement = value;
                    OnPropertyChanged(nameof(SelectedElement));
                }
            }
        }
        #endregion

        #region AddElement Command

        /// <summary>
        /// Adds Element to ViewModel layer
        /// </summary>
        private ICommand _addElement;
        public ICommand AddElement => _addElement ?? (_addElement = new RelayCommand(AddElementExecute));

        private void AddElementExecute([NotNull] object newObject)
        {
            if (!(newObject is Element newElement))
                throw new Exception("new element must be type of Element");
            if (Project.AddElement is RelayCommand relayCommand)
                relayCommand.Run(newElement);
            else
                throw new Exception("AddElement Command should be type of RelayCommand");
        }

        #endregion

        #region RemoveElement Command

        /// <summary>
        /// Removes Element from ViewModel and Model layers
        /// </summary>
        private ICommand _removeElement;
        public ICommand RemoveElement => _removeElement ?? (_removeElement = new RelayCommand(RemoveElementExecute));

        private void RemoveElementExecute([NotNull]object toRemove)
        {
            if (!(toRemove is Element elementToRemove))
                throw new Exception("Element to remove must be type of Element");
            if (Project.RemoveElement is RelayCommand relayCommand)
                relayCommand.Run(elementToRemove);
            else
                throw new Exception("AddElement Command should be type of RelayCommand");
        }
        #endregion

        #region AddConnection Command

        /// <summary>
        /// Adds connection to ViewModel and Model layers
        /// </summary>
        private ICommand _addConnection;
        public ICommand AddConnection => _addConnection ?? (_addConnection = new RelayCommand(AddConnectionExecute));

        private void AddConnectionExecute([NotNull] object newObject)
        {
            if (!(newObject is ConnectionModel newConnectionModel))
                throw new Exception("new Connection must be type of ConnectionModel");
            if (Project.AddConnection is RelayCommand relayCommand)
                relayCommand.Run(newConnectionModel);
            else
                throw new Exception("AddConnection Command should be type of RelayCommand");
        }
        #endregion

        #region RemoveConnection Command

        /// <summary>
        /// Removes connection from ViewModel and Model layers
        /// </summary>
        private ICommand _removeConnection;
        public ICommand RemoveConnection => _removeConnection ?? (_removeConnection = new RelayCommand(RemoveConnectionExecute));

        private void RemoveConnectionExecute([NotNull] object toRemove)
        {
            if (!(toRemove is ConnectionModel connectionModelToRemove))
                throw new Exception("Connection to Remove must be type of ConnectionModel");
            if (Project.RemoveConnection is RelayCommand relayCommand)
                relayCommand.Run(connectionModelToRemove);
            else
                throw new Exception("RemoveConnection Command should be type of RelayCommand");
        }


        #endregion

        #region RotateElements Command

        private ICommand _rotateElements;
        public ICommand RotateElements => _rotateElements ?? (_rotateElements = new RelayCommand(RotateElementsExecute));


        private void RotateElementsExecute([NotNull] object ElementsToRotate)
        {
            if (ElementsToRotate == null)
                throw new Exception("Elements to rotate can't be null");
            if (Project.RotateElements is RelayCommand relayCommand)
                relayCommand.Run(ElementsToRotate);
            else
                throw new Exception("RotateElements Command should be type of RelayCommand");
        }
        #endregion

        #region Save & Open

        private ICommand _save;
        public ICommand Save => _save ?? (_save = new RelayCommand(SaveExecute));

        private void SaveExecute(object sender)
        {
            Project.SaveXMLFile();
        }

        private ICommand _saveAs;
        public ICommand SaveAs => _saveAs ?? (_saveAs = new RelayCommand(SaveAsExecute));

        private void SaveAsExecute(object sender)
        {
            Project.SaveAsXMLFile();
        }

        private ICommand _open;
        public ICommand Open => _open ?? (_open = new RelayCommand(OpenExecute));

        private void OpenExecute(object sender)
        {
            Project.OpenXMLFile();
        }

        public string WriteElementsAndConnectionsToString(IList<ElementVM> elementsVM, IList<ConnectionVM> connectionsVM)
        {
            List<Element> selectedElements = new List<Element>();
            foreach (ElementVM elementVM in elementsVM)
            {
                selectedElements.Add(elementVM.Element);
            }

            List<ConnectionModel> selectedConnectionModels = new List<ConnectionModel>();
            foreach (ConnectionVM connectionVM in connectionsVM)
            {
                selectedConnectionModels.Add(connectionVM.ConnectionModel);
            }

            return Project.WriteElementsAndConnectionsToString(selectedElements, selectedConnectionModels);
        }

        public void ReadElementsAndConnectionsFromString(string inputString)
        {
            Project.ReadElementsAndConnectionsFromString(inputString);
        }

        public void ReadAndShiftElementsAndConnectionsFromString(string inputString, Vector elementsShiftVector)
        {
            Project.ReadAndShiftElementsAndConnectionsFromString(inputString, elementsShiftVector);
        }

        #endregion

        #region GetByID Methods

        public ElementVM GetElementVMByGuid([NotNull] Guid ID)
        {
            if (ID == null)
                throw new ArgumentNullException(nameof(ID));
            foreach (ElementVM elementVM in ElementsVM)
            {
                if (elementVM.ID.Equals(ID))
                    return elementVM;
            }
            return null;
        }

        public ConnectorVM GetConnectorVMByGuid([NotNull] Guid ID)
        {
            if (ID == null)
                throw new ArgumentNullException(nameof(ID));
            foreach (ElementVM elementVM in ElementsVM)
            {
                foreach (ConnectorVM connectorVM in elementVM.ConnectorsVM)
                {
                    if (connectorVM.ID.Equals(ID))
                        return connectorVM;
                }
            }
            return null;
        }

        public ConnectionVM GetConnectionVMByGuid([NotNull] Guid ID)
        {
            if (ID == null)
                throw new ArgumentNullException(nameof(ID));
            foreach (ConnectionVM connectionVM in ConnectionsVM)
            {
                if (connectionVM.ID.Equals(ID))
                    return connectionVM;
            }
            return null;
        }

        #endregion

        #region Auto Connect

        public bool AutoConnectChecked { get; set; } = true;

        private ICommand _autoConnectChanged;
        public ICommand AutoConnectChanged => _autoConnectChanged ?? (_autoConnectChanged = new RelayCommand(AutoConnectChangedExecute));

        private void AutoConnectChangedExecute(object obj)
        {
            // some action on change if needed
        }

        void AutoConnectDelegate(ElementVM elementVM)
        {
            if (AutoConnectChecked)
            {
                AutoConnectElements(elementVM, ElementsVM);
            }
        }

        void AutoConnectElements(ElementVM elementToConnect, IList<ElementVM> elements)
        {
            List<ConnectorVM> elementConnectors = new List<ConnectorVM>();
            List<ConnectorVM> elementsConnectors = new List<ConnectorVM>();
            foreach (var elementVM in elements)
            {
                if (elementVM != elementToConnect)
                {
                    elementsConnectors.AddRange(elementVM.ConnectorsVM);
                }
            }
            elementConnectors.AddRange(elementToConnect.ConnectorsVM);
            AutoConnect(elementConnectors, elementsConnectors);
        }

        void AutoConnect(IList<ConnectorVM> elementConnectors, IList<ConnectorVM> elementsConnectors)
        {
            if (elementConnectors.Count > 0 && elementsConnectors.Count > 0)
            {
                if (elementConnectors.Count > 1)
                {
                    for (int i = 1; i < elementConnectors.Count; i++)
                    {
                        if (elementConnectors[0].Parent != elementConnectors[i].Parent)
                        {
                            throw new Exception("elementConnectors in AutoConnect must have same Parent Element");
                        }
                    }
                }
            }
            else return;

            ElementVM connectorsParent = elementConnectors[0].Parent;

            Dictionary<Direction, List<ConnectorVM>> connectorsOnSide = new Dictionary<Direction, List<ConnectorVM>>
            {
                {Direction.OnLeft, new List<ConnectorVM>()},
                {Direction.OnRight,new List<ConnectorVM>()},
                {Direction.OnTop, new List<ConnectorVM>()},
                {Direction.OnBottom,new List<ConnectorVM>()},
            };

            Dictionary<ConnectorOrientation, List<ConnectorVM>> elementConnectorsByOrientation = new Dictionary<ConnectorOrientation, List<ConnectorVM>>
            {
                {ConnectorOrientation.Left, new List<ConnectorVM>()},
                {ConnectorOrientation.Right, new List<ConnectorVM>()},
                {ConnectorOrientation.Top, new List<ConnectorVM>()},
                {ConnectorOrientation.Bottom, new List<ConnectorVM>()},
                {ConnectorOrientation.None, new List<ConnectorVM>()}
            };

            //categorize connectors of elementToConnect
            foreach (var connectorVm in elementConnectors)
            {
                switch (connectorVm.Orientation)
                {
                    case ConnectorOrientation.Left:
                        elementConnectorsByOrientation[ConnectorOrientation.Left].Add(connectorVm);
                        break;
                    case ConnectorOrientation.Right:
                        elementConnectorsByOrientation[ConnectorOrientation.Right].Add(connectorVm);
                        break;
                    case ConnectorOrientation.Top:
                        elementConnectorsByOrientation[ConnectorOrientation.Top].Add(connectorVm);
                        break;
                    case ConnectorOrientation.Bottom:
                        elementConnectorsByOrientation[ConnectorOrientation.Bottom].Add(connectorVm);
                        break;
                    default:
                        elementConnectorsByOrientation[ConnectorOrientation.None].Add(connectorVm);
                        break;
                }
            }

            Point elementCenter = new Point(connectorsParent.Position.X + connectorsParent.Size.Width / 2, connectorsParent.Position.Y + connectorsParent.Size.Height / 2);

            #region Categorizing Stationary Connectors by Position Relative to connectorsParent

            foreach (var elementConnector in elementsConnectors)
            {
                Vector relativeConnectorPosition = Point.Subtract(elementConnector.GlobalPosition, elementCenter);

                if (relativeConnectorPosition.Y < 0)
                {
                    if (relativeConnectorPosition.X < 0)
                    {
                        if (relativeConnectorPosition.X < relativeConnectorPosition.Y)
                        {//section 1
                            if (elementConnectorsByOrientation[ConnectorOrientation.Top].Any() && !elementConnectorsByOrientation[ConnectorOrientation.Left].Any())
                            {
                                connectorsOnSide[Direction.OnTop].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnLeft].Add(elementConnector);
                            }
                        }
                        else
                        {//section 2
                            if (!elementConnectorsByOrientation[ConnectorOrientation.Top].Any() && elementConnectorsByOrientation[ConnectorOrientation.Left].Any())
                            {
                                connectorsOnSide[Direction.OnLeft].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnTop].Add(elementConnector);
                            }
                        }
                    }
                    else
                    {
                        if (-relativeConnectorPosition.X >= relativeConnectorPosition.Y)
                        {//section 3
                            if (!elementConnectorsByOrientation[ConnectorOrientation.Top].Any() && elementConnectorsByOrientation[ConnectorOrientation.Right].Any())
                            {
                                connectorsOnSide[Direction.OnRight].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnTop].Add(elementConnector);
                            }
                        }
                        else
                        {//section 4
                            if (elementConnectorsByOrientation[ConnectorOrientation.Top].Any() && !elementConnectorsByOrientation[ConnectorOrientation.Right].Any())
                            {
                                connectorsOnSide[Direction.OnTop].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnRight].Add(elementConnector);
                            }
                        }
                    }
                }
                else
                {
                    if (relativeConnectorPosition.X >= 0)
                    {
                        if (relativeConnectorPosition.X >= relativeConnectorPosition.Y)
                        {//section 5
                            if (elementConnectorsByOrientation[ConnectorOrientation.Bottom].Any() && !elementConnectorsByOrientation[ConnectorOrientation.Right].Any())
                            {
                                connectorsOnSide[Direction.OnBottom].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnRight].Add(elementConnector);
                            }
                        }
                        else
                        {//section 6
                            if (elementConnectorsByOrientation[ConnectorOrientation.Right].Any() && !elementConnectorsByOrientation[ConnectorOrientation.Bottom].Any())
                            {
                                connectorsOnSide[Direction.OnRight].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnBottom].Add(elementConnector);
                            }
                        }
                    }
                    else
                    {
                        if (-relativeConnectorPosition.X < relativeConnectorPosition.Y)
                        {//section 7
                            if (elementConnectorsByOrientation[ConnectorOrientation.Left].Any() && !elementConnectorsByOrientation[ConnectorOrientation.Bottom].Any())
                            {
                                connectorsOnSide[Direction.OnLeft].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnBottom].Add(elementConnector);
                            }
                        }
                        else
                        {//section 8
                            if (elementConnectorsByOrientation[ConnectorOrientation.Bottom].Any() && !elementConnectorsByOrientation[ConnectorOrientation.Left].Any())
                            {
                                connectorsOnSide[Direction.OnBottom].Add(elementConnector);
                            }
                            else
                            {
                                connectorsOnSide[Direction.OnLeft].Add(elementConnector);
                            }
                        }
                    }
                }
            }

            #endregion

            Comparer<ConnectorVM> compareByXPosition = Comparer<ConnectorVM>.Create(delegate (ConnectorVM a, ConnectorVM b)
            {
                if (a.GlobalPosition.X > b.GlobalPosition.X)
                    return 1;
                else
                    return -1;
            });

            Comparer<ConnectorVM> compareByYPosition = Comparer<ConnectorVM>.Create(delegate (ConnectorVM a, ConnectorVM b)
            {
                if (a.GlobalPosition.Y > b.GlobalPosition.Y)
                    return 1;
                else
                    return -1;
            });

            ConnectNearest(elementConnectorsByOrientation[ConnectorOrientation.Left], connectorsOnSide[Direction.OnLeft], compareByYPosition);
            ConnectNearest(elementConnectorsByOrientation[ConnectorOrientation.Right], connectorsOnSide[Direction.OnRight], compareByYPosition);
            ConnectNearest(elementConnectorsByOrientation[ConnectorOrientation.Top], connectorsOnSide[Direction.OnTop], compareByXPosition);
            ConnectNearest(elementConnectorsByOrientation[ConnectorOrientation.Bottom], connectorsOnSide[Direction.OnBottom], compareByXPosition);
        }

        void ConnectNearest(IList<ConnectorVM> sideConnectors, IList<ConnectorVM> onSideConnectors, Comparer<ConnectorVM> sortingComparer)
        {
            if (sideConnectors.Count > 0 & onSideConnectors.Count > 0)
            {
                //changing IList to List to be able to sort it later
                List<ConnectorVM> side;
                if (sideConnectors is List<ConnectorVM> sideConnectorsList) { side = sideConnectorsList; }
                else { side = sideConnectors.ToList(); }

                //finding nearest connectors to connect to
                List<ConnectorVM> pickedConnectors = FindNearest(side, onSideConnectors);

                //sorting for cleaner match
                side.Sort(sortingComparer);
                pickedConnectors.Sort(sortingComparer);

                for (int i = 0; i < side.Count; i++)
                {
                    if (i < pickedConnectors.Count)
                    {
                        if (!(pickedConnectors[i].Connected && pickedConnectors[i].ConnectionsVM[0].Contains(side[i]))) //connection is not existing one
                        {
                            //disconnect current connections
                            if (side[i].Connected && RemoveConnection.CanExecute(side[i].ConnectorModel.ConnectionModels[0]))
                            {
                                RemoveConnection.Execute(side[i].ConnectorModel.ConnectionModels[0]);
                            }

                            if (pickedConnectors[i].Connected && RemoveConnection.CanExecute(pickedConnectors[i].ConnectorModel.ConnectionModels[0]))
                            {
                                RemoveConnection.Execute(pickedConnectors[i].ConnectorModel.ConnectionModels[0]);
                            }

                            ConnectionModel newConnection = new ConnectionModel(side[i].ConnectorModel, pickedConnectors[i].ConnectorModel);
                            (AddConnection as RelayCommand).Run(newConnection);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        List<ConnectorVM> FindNearest(IList<ConnectorVM> elementConnectors, IList<ConnectorVM> elementsConnectors)
        {
            List<ConnectorVM> toReturn = new List<ConnectorVM>();
            var elementsConnectorList = elementsConnectors.ToList();
            Vector meanElementConnectorPosition = new Vector(0, 0);
            foreach (var elementConnector in elementConnectors)
            {
                meanElementConnectorPosition.X += elementConnector.GlobalPosition.X;
                meanElementConnectorPosition.Y += elementConnector.GlobalPosition.Y;
            }
            meanElementConnectorPosition = Vector.Divide(meanElementConnectorPosition, elementConnectors.Count);

            if (elementsConnectorList.Count > 1)
            {
                elementsConnectorList.Sort(delegate (ConnectorVM a, ConnectorVM b)
                {
                    var vectorA = Point.Subtract(a.GlobalPosition, meanElementConnectorPosition);
                    var vectorB = Point.Subtract(b.GlobalPosition, meanElementConnectorPosition);
                    double magA = vectorA.X * vectorA.X + vectorA.Y * vectorA.Y; //no need to use square root because it is only comparison
                    double magB = vectorB.X * vectorB.X + vectorB.Y * vectorB.Y;
                    if (magA < magB)
                    {
                        return -1;
                    }
                    else if (magA == magB)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                });
            }

            for (int i = 0; i < elementConnectors.Count; i++)
            {
                if (i < elementsConnectorList.Count)
                    toReturn.Add(elementsConnectorList[i]);
            }

            return toReturn;
        }


        enum Direction
        {
            OnLeft,
            OnRight,
            OnTop,
            OnBottom,
            None
        }
        #endregion

        private ICommand _exportToSnpCommand;
        public ICommand ExportToSnpCommand => _exportToSnpCommand ?? (_exportToSnpCommand = new RelayCommand(ExportToSnpExecute));

        private void ExportToSnpExecute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "sNp File|*.sNp";
            saveFileDialog.Title = "Save an sNp File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {
                TouchstoneFile touchstoneFile = new TouchstoneFile();
                touchstoneFile.Write(Project.ResultSMatrix, Project.Frequencies, Project.ReferenceImpedance, saveFileDialog.FileName);
            }
        }

        private ICommand _exportToCsvRICommand;
        public ICommand ExportToCsvRICommand => _exportToCsvRICommand ?? (_exportToCsvRICommand = new RelayCommand(ExportToCsvRIExecute));

        private void ExportToCsvRIExecute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV File|*.csv";
            saveFileDialog.Title = "Save an CSV File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {

                var csv = new StringBuilder();
                string delimiter = "; ";


                //Names
                string temp = "Freq[GHz]";
                List<double> frequencies = Project.ComplexCharts[0].Frequencies;
                foreach (var complexChart in Project.ComplexCharts)
                {
                    temp += delimiter + complexChart.Name + "_Real";
                    temp += delimiter + complexChart.Name + "_Imag";
                }

                csv.AppendLine(temp);

                //Values
                for (var i = 0; i < frequencies.Count; i++)
                {
                    temp = frequencies[i].ToString(CultureInfo.InvariantCulture);
                    foreach (ComplexChart complexChart in Project.ComplexCharts)
                    {
                        temp += delimiter + complexChart.Values[i].Real.ToString(CultureInfo.InvariantCulture);
                        temp += delimiter + complexChart.Values[i].Imaginary.ToString(CultureInfo.InvariantCulture);
                    }

                    csv.AppendLine(temp);
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
            }
        }

        private ICommand _exportToCsvMPCommand;
        public ICommand ExportToCsvMPCommand => _exportToCsvMPCommand ?? (_exportToCsvMPCommand = new RelayCommand(ExportToCsvMPExecute));

        private void ExportToCsvMPExecute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV File|*.csv";
            saveFileDialog.Title = "Save an CSV File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {
                var csv = new StringBuilder();
                string delimiter = "; ";


                //Names
                string temp = "Freq[GHz]";
                List<double> frequencies = Project.ComplexCharts[0].Frequencies;
                foreach (var complexChart in Project.ComplexCharts)
                {
                    temp += delimiter + complexChart.Name + "_Module";
                    temp += delimiter + complexChart.Name + "_Phase";
                }

                csv.AppendLine(temp);

                //Values
                for (var i = 0; i < frequencies.Count; i++)
                {
                    temp = frequencies[i].ToString(CultureInfo.InvariantCulture);
                    foreach (ComplexChart complexChart in Project.ComplexCharts)
                    {
                        temp += delimiter + complexChart.Values[i].Magnitude.ToString(CultureInfo.InvariantCulture);
                        temp += delimiter + complexChart.Values[i].Phase.ToString(CultureInfo.InvariantCulture);
                    }

                    csv.AppendLine(temp);
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
            }
        }

        private ICommand _exportToCsvMdBPCommand;
        public ICommand ExportToCsvMdBPCommand => _exportToCsvMdBPCommand ?? (_exportToCsvMdBPCommand = new RelayCommand(ExportToCsvMdBPExecute));

        private void ExportToCsvMdBPExecute(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV File|*.csv";
            saveFileDialog.Title = "Save an CSV File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {
                var csv = new StringBuilder();
                string delimiter = "; ";


                //Names
                string temp = "Freq[GHz]";
                List<double> frequencies = Project.ComplexCharts[0].Frequencies;
                foreach (var complexChart in Project.ComplexCharts)
                {
                    temp += delimiter + complexChart.Name + "_Module[dB]";
                    temp += delimiter + complexChart.Name + "_Phase";
                }

                csv.AppendLine(temp);

                //Values
                for (var i = 0; i < frequencies.Count; i++)
                {
                    temp = frequencies[i].ToString(CultureInfo.InvariantCulture);
                    foreach (ComplexChart complexChart in Project.ComplexCharts)
                    {
                        temp += delimiter + (20*Math.Log10(complexChart.Values[i].Magnitude)).ToString(CultureInfo.InvariantCulture);
                        temp += delimiter + complexChart.Values[i].Phase.ToString(CultureInfo.InvariantCulture);
                    }

                    csv.AppendLine(temp);
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
            }
        }
    }
}