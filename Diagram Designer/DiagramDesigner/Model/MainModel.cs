using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.BlockTypes.Port;
using DiagramDesigner.CommandManagement;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.ViewModel.ViewModelBases;
using Microsoft.Win32;

namespace DiagramDesigner.Model
{
    public class MainModel : NotifyPropertyChangedBase
    {
        public ObservableCollection<Element> Elements { get; } = new ObservableCollection<Element>();

        public ObservableCollection<ConnectionModel> ConnectionModels { get; } = new ObservableCollection<ConnectionModel>();

        public MyCommandManager MyCommandManager { get; } = new MyCommandManager();

        private List<ElementsChain> _listOfChains;
        public ObservableCollection<ComplexChart> ComplexCharts { get; } = new ObservableCollection<ComplexChart>();
        public List<List<List<Complex>>> ResultSMatrix { get; set; } //Two dimensional matrix of complex lists
        public List<double> Frequencies { get; set; }
        public double ReferenceImpedance { get; set; } = 50;
        public double FromFreq { get; set; } = 1e9;
        public double ToFreq { get; set; } = 15e9;
        public double Step { get; set; } = 0.1e9;

        public void Simulate()
        {
            //LoadExampleScheme();

            _listOfChains = CreateListsFromElements(Elements);
            RenumberPorts(_listOfChains);
            int portCount = 0;
            foreach (ElementsChain elementsChain in _listOfChains)
            {
                portCount += elementsChain.Ports.Count;
            }

            #region Initializing ResultSMatrix

            ResultSMatrix = new List<List<List<Complex>>>();
            for (int i = 0; i < portCount; i++)
            {
                ResultSMatrix.Add(new List<List<Complex>>()); //initializing first dimension
                for (int j = 0; j < portCount; j++)
                {
                    ResultSMatrix[i].Add(new List<Complex>()); //initializing second dimension
                }
            }

            #endregion

            #region Solving and renumbering result ports

            foreach (ElementsChain elementsChain in _listOfChains)
            {
                elementsChain.Solve(FromFreq, ToFreq, Step, ReferenceImpedance);
                Frequencies = elementsChain.Frequencies;
                //changing elementsChain indexes to Port indexes
                for (int i = 0; i < elementsChain.Ports.Count; i++)
                {
                    for (int j = 0; j < elementsChain.Ports.Count; j++)
                    {
                        //portIndex-1 because ports are numbered from 1
                        ResultSMatrix[elementsChain.Ports[i].Number - 1][elementsChain.Ports[j].Number - 1] = elementsChain.SPlotsMatrix[i][j];
                    }
                }
            }

            #endregion

            #region Generating plots

            ComplexCharts.Clear();

            for (int i = 0; i < ResultSMatrix.Count; i++)
            {
                for (int j = 0; j < ResultSMatrix[i].Count; j++)
                {
                    if (ResultSMatrix[i][j].Count > 0)
                    {
                        ComplexCharts.Add(new ComplexChart(ResultSMatrix[i][j], Frequencies, $"S({i + 1},{j + 1})"));
                    }
                }
            }

            #endregion
        }

        void LoadExampleScheme()
        {
            //creating scheme from Model layer example
            PortModel port1 = new PortModel();
            port1.Position = new Point(30, 100);
            port1.Size = new Size(50, 40);
            port1.PortImpedance = 1;
            (AddElement as RelayCommand).Run(port1);

            LumpedElement inductor = new LumpedElement(ElementType.InductorInParallel, ElementType.InductorInParallel.ToString());
            inductor.Position = new Point(150, 100);
            inductor.Size = new Size(50, 40);
            inductor.L = 0.00001f;
            (AddElement as RelayCommand).Run(inductor);

            ConnectionModel conmod = new ConnectionModel(port1.Connectors[0], inductor.Connectors[0]);
            (AddConnection as RelayCommand).Run(conmod);

            LumpedElement capacitor = new LumpedElement(ElementType.CapacitorInParallel, ElementType.CapacitorInParallel.ToString());
            capacitor.Position = new Point(300, 100);
            capacitor.Size = new Size(50, 40);
            capacitor.C = 0.001f;
            (AddElement as RelayCommand).Run(capacitor);

            conmod = new ConnectionModel(inductor.Connectors[1], capacitor.Connectors[0]);
            (AddConnection as RelayCommand).Run(conmod);

            PortModel port2 = new PortModel();
            port2.Position = new Point(420, 100);
            port2.Size = new Size(50, 40);
            port2.PortImpedance = 1;
            (AddElement as RelayCommand).Run(port2);
            port2.Rotation = 2;

            conmod = new ConnectionModel(capacitor.Connectors[1], port2.Connectors[0]);
            (AddConnection as RelayCommand).Run(conmod);
        }

        public List<ElementsChain> CreateListsFromElements(IList<Element> InputElements)
        {
            List<ElementsChain> validElementChains = new List<ElementsChain>();
            ElementsChain currentChain = new ElementsChain();
            List<Element> notAnalyzedElements = new List<Element>();
            List<PortModel> ports = new List<PortModel>();

            foreach (Element element in InputElements)
            {
                if (element is PortModel portModel)
                {
                    ports.Add(portModel);
                }

                element.Chain = null;
                element.IndexInChain = 0;
            }

            foreach (Element element in InputElements)
            {
                notAnalyzedElements.Add(element);
            }

            List<Element> neighborsToCheck = new List<Element>();
            while (ports.Count > 0) //for each port
            {
                bool currentChainIsValid = true;

                currentChain.ConnectorsSum.Add(0); // 0 at the first place of array, because the sum of the connectors preceding the element with index 0 is 0

                //pick one port element and try to create chain from it
                neighborsToCheck.Add(ports[0]);
                currentChain.Elements.Add(ports[0]);
                currentChain.ConnectorsSum.Add(ports[0].Connectors.Count);
                currentChain.PortsIndexes.Add(currentChain.ConnectorsSum[currentChain.Elements.IndexOf(ports[0])], ports[0]);
                currentChain.Ports.Add(ports[0]);

                notAnalyzedElements.Remove(ports[0]);
                ports.Remove(ports[0]);


                while (neighborsToCheck.Count > 0 && currentChainIsValid) //while at the ends of the currentChain there are an unChecked elements and each connector is connected
                {
                    if (neighborsToCheck[0].Connectors.Count > 0)
                    {
                        foreach (ConnectorModel elementConnector in neighborsToCheck[0].Connectors)
                        {
                            if (!elementConnector.Connected)
                            {
                                currentChainIsValid = false;
                                break;
                            }

                            foreach (ConnectionModel connectionModel in elementConnector.ConnectionModels) //should be one
                            {
                                ConnectorModel elementToAddConnector;
                                ConnectorModel currentlyCheckedElementConnector;
                                Element elementToAdd; //element connected to neighborsToCheck[0]

                                if (connectionModel.SinkConnector == elementConnector)
                                {
                                    elementToAdd = connectionModel.SourceConnector.Parent;

                                    //this is needed later to determine the coordinates of the connection in W matrix:
                                    elementToAddConnector = connectionModel.SourceConnector;
                                    currentlyCheckedElementConnector = connectionModel.SinkConnector;
                                }
                                else
                                {
                                    elementToAdd = connectionModel.SinkConnector.Parent;

                                    //this is needed later to determine the coordinates of the connection in W matrix:
                                    elementToAddConnector = connectionModel.SinkConnector;
                                    currentlyCheckedElementConnector = connectionModel.SourceConnector;
                                }

                                if (!currentChain.Elements.Contains(elementToAdd) && notAnalyzedElements.Contains(elementToAdd)) //if element is accepted as a new ChainElement
                                {
                                    neighborsToCheck.Add(elementToAdd);
                                    currentChain.Elements.Add(elementToAdd);
                                    notAnalyzedElements.Remove(elementToAdd);
                                    int newElementIndex = currentChain.Elements.IndexOf(elementToAdd);
                                    if (newElementIndex > 0)
                                    {
                                        currentChain.ConnectorsSum.Add(currentChain.ConnectorsSum[newElementIndex] +
                                            elementToAdd.Connectors.Count);
                                    }
                                    else
                                    {
                                        throw new Exception(
                                            "Index of new analyzed Element in Chain should be greater than 0");
                                    }
                                    if (elementToAdd is PortModel portModel)
                                    {
                                        currentChain.PortsIndexes.Add(currentChain.ConnectorsSum[currentChain.Elements.IndexOf(elementToAdd)], portModel); //assuming that port model has only one connector
                                        currentChain.Ports.Add(portModel);
                                        ports.Remove(portModel);
                                    }

                                    int firstConnectionIndex =
                                        currentChain.ConnectorsSum[newElementIndex] +
                                        elementToAdd.Connectors.IndexOf(elementToAddConnector);
                                    int secondConnectionIndex =
                                        currentChain.ConnectorsSum[currentChain.Elements.IndexOf(neighborsToCheck[0])] +
                                        neighborsToCheck[0].Connectors.IndexOf(currentlyCheckedElementConnector);

                                    if (firstConnectionIndex < secondConnectionIndex) //Key is a smaller index
                                    {
                                        currentChain.ConnectionList.Add(firstConnectionIndex,
                                            secondConnectionIndex);
                                    }
                                    else
                                    {
                                        currentChain.ConnectionList.Add(secondConnectionIndex,
                                            firstConnectionIndex);
                                    }
                                }
                            }
                        }
                    }
                    neighborsToCheck.Remove(neighborsToCheck[0]);
                }

                if (currentChainIsValid)
                {
                    validElementChains.Add(currentChain);

                    for (int i = 0; i < currentChain.Elements.Count; i++)
                    {
                        currentChain.Elements.ElementAt(i).Chain = currentChain;
                        currentChain.Elements.ElementAt(i).IndexInChain = i;
                    }
                    currentChain = new ElementsChain();
                }
                else
                {
                    neighborsToCheck.Clear();
                    currentChain = new ElementsChain();
                }
            }

            return validElementChains;
        }

        #region Save & Open

        private string _lastFileLocation = "";
        public void SaveXMLFile()
        {
            if (_lastFileLocation == "")
                SaveAsXMLFile();
            else
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(_lastFileLocation, settings);
                WriteElementsAndConnections(writer, Elements, ConnectionModels);
            }
        }

        public void SaveAsXMLFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML File|*.xml";
            saveFileDialog.Title = "Save an XML File";
            saveFileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.  
            if (saveFileDialog.FileName != "")
            {
                _lastFileLocation = saveFileDialog.FileName;
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create(saveFileDialog.FileName, settings);
                WriteElementsAndConnections(writer, Elements, ConnectionModels);
            }
        }

        public void OpenXMLFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files|*.xml";
            openFileDialog.Title = "Open XML File";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                DeleteCommand newDeleteCommand = new DeleteCommand(Elements, ConnectionModels, this);
                MyCommandManager.AddToList(newDeleteCommand);
                newDeleteCommand.Execute();

                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.IgnoreWhitespace = true;
                XmlReader reader = XmlReader.Create(openFileDialog.FileName, readerSettings);
                ReadElementsAndConnections(reader);
            }
        }

        #endregion

        #region Copy & Paste

        private int _shiftValue = 0;

        public void CopyXMLStringToClipboard(IEnumerable<Element> elements, IEnumerable<ConnectionModel> connections)
        {
            string format = "EdisonScheme";
            Clipboard.Clear();

            // Set data to clipboard
            Clipboard.SetData(format, WriteElementsAndConnectionsToString(elements, connections));
            _shiftValue = 0;
        }

        public void PasteXMLStringFromClipboard()
        {
            string format = "EdisonScheme";
            // Get data from clipboard
            if (Clipboard.ContainsData(format))
            {
                _shiftValue += 15;
                ReadAndShiftElementsAndConnectionsFromString((string)Clipboard.GetData(format),
                    new Vector(_shiftValue, _shiftValue));
            }
            else
            {
                //incorrect format
            }
        }

        #endregion

        #region Write Elements and Connections

        public string WriteElementsAndConnectionsToString(IEnumerable<Element> elementsToCopy, IEnumerable<ConnectionModel> connectionModelsToCopy)
        {
            StringWriter stringwriter = new StringWriter();
            XmlWriter writer = XmlWriter.Create(stringwriter);
            WriteElementsAndConnections(writer, elementsToCopy, connectionModelsToCopy);
            return stringwriter.ToString();
        }

        public void WriteElementsAndConnections(XmlWriter writer, IEnumerable<Element> elementsToWrite, IEnumerable<ConnectionModel> connectionModelsToWrite)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("Root");
            
            writer.WriteStartElement("SimulationProperties");
            writer.WriteElementString("ReferenceImpedance", ReferenceImpedance.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("FromFreq", FromFreq.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ToFreq", ToFreq.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Step", Step.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            
            foreach (Element element in elementsToWrite)
            {
                element.WriteXml(writer);
            }
            foreach (ConnectionModel connectionModel in connectionModelsToWrite)
            {
                //checking if each connection is connected on both sides to anyone of elements from the list, and then saving a it
                if (GetConnectorModelByGuid(connectionModel.SinkConnector.ID, elementsToWrite) != null && GetConnectorModelByGuid(connectionModel.SourceConnector.ID, elementsToWrite) != null)
                    connectionModel.WriteXml(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }


        #endregion

        #region Read Elements and Connections

        public void ReadElementsAndConnectionsFromString(string stringXml)
        {
            ReadAndShiftElementsAndConnectionsFromString(stringXml, new Vector(0, 0));
        }

        public void ReadAndShiftElementsAndConnectionsFromString(string stringXml, Vector elementsShiftVector)
        {
            XmlReader reader = XmlReader.Create(new StringReader(stringXml));
            ReadAndShiftElementsAndConnections(reader, elementsShiftVector);
        }

        public void ReadElementsAndConnections(XmlReader reader)
        {
            ReadAndShiftElementsAndConnections(reader, new Vector(0, 0));
        }

        public void ReadAndShiftElementsAndConnections(XmlReader reader, Vector elementsShiftVector)
        {

            List<Element> ReadElements = new List<Element>();
            List<ConnectionModel> ReadConnectionModels = new List<ConnectionModel>();
            try
            {
                reader.MoveToContent();
                reader.Read();
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "SimulationProperties")
                    {
                        if (reader.Read() && reader.Name == "ReferenceImpedance")
                        {
                            ReferenceImpedance = double.Parse(reader.ReadString());
                            OnPropertyChanged(nameof(ReferenceImpedance));
                        }
                        else throw new Exception("ReferenceImpedance is missing or it is in the wrong place");
                        if (reader.Read() && reader.Name == "FromFreq")
                        {
                            FromFreq = double.Parse(reader.ReadString());
                            OnPropertyChanged(nameof(FromFreq));
                        }
                        else throw new Exception("FromFreq is missing or it is in the wrong place");
                        if (reader.Read() && reader.Name == "ToFreq")
                        {
                            ToFreq = double.Parse(reader.ReadString());
                            OnPropertyChanged(nameof(ToFreq));
                        }
                        else throw new Exception("ToFreq is missing or it is in the wrong place");
                        if (reader.Read() && reader.Name == "Step")
                        {
                            Step = double.Parse(reader.ReadString());
                            OnPropertyChanged(nameof(Step));
                        }
                        else throw new Exception("Step is missing or it is in the wrong place");
                    }
                }

                do
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Element":
                                Element readElement = ReadElement(reader);
                                readElement.Position = Point.Add(readElement.Position, elementsShiftVector);
                                ReadElements.Add(readElement);
                                break;
                            case "Connection":
                                ReadConnectionModels.Add(ReadConnectionModel(reader, ReadElements));
                                break;
                        }
                    }
                } while (reader.Read());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

            //changing ID's that every Element, connector and connection have unique
            foreach (Element readElement in ReadElements)
            {
                readElement.ID = Guid.NewGuid();
                foreach (ConnectorModel connectorModel in readElement.Connectors)
                {
                    connectorModel.ID = Guid.NewGuid();
                }
            }
            foreach (ConnectionModel readConnectionModel in ReadConnectionModels)
            {
                readConnectionModel.ID = Guid.NewGuid();
            }

            PasteCommand newPasteCommand = new PasteCommand(ReadElements, ReadConnectionModels, this);
            MyCommandManager.AddToList(newPasteCommand);
            newPasteCommand.Execute();
            RenumberPorts(Elements);
        }

        public Element ReadElement(XmlReader reader)
        {
            if (reader.Read() && reader.NodeType == XmlNodeType.Element && reader.Name == "Type")
            {
                if (reader.Read() && Enum.TryParse(reader.Value, out ElementType type))
                {
                    reader.Read();
                    Element newElement = Creator.CreateElementByType(type);
                    newElement.ReadXml(reader);
                    return newElement;
                }
                else
                    throw new Exception(reader.Value + " is unknown Type of Element");
            }
            else
                throw new Exception("The Type of Element is not saved in the right place");
        }

        public ConnectionModel ReadConnectionModel(XmlReader reader, List<Element> readElements)
        {
            ConnectorModel Sink;
            ConnectorModel Source;
            Guid ConnectionID;
            if (reader.Read() && reader.Name == "ID")
            {
                ConnectionID = Guid.Parse(reader.ReadString());
                if (reader.Read() && reader.Name == "SourceID")
                {
                    Source = GetConnectorModelByGuid(Guid.Parse(reader.ReadString()), readElements);
                    if (reader.Read() && reader.Name == "SinkID")
                    {
                        Sink = GetConnectorModelByGuid(Guid.Parse(reader.ReadString()), readElements);
                        if (Sink != null && Source != null)
                        {
                            ConnectionModel newConnectionModel = new ConnectionModel(Sink, Source, ConnectionID);
                            Sink.ConnectionModels.Add(newConnectionModel);
                            Source.ConnectionModels.Add(newConnectionModel);
                            Sink.Connected = true;
                            Source.Connected = true;
                            return newConnectionModel;
                        }
                    }
                }
            }
            throw new Exception("Can not find the correct connection");
        }

        #endregion

        #region AddElement Command

        /// <summary>
        /// Adds Element to Model layer
        /// </summary>
        private ICommand _addElement;
        public ICommand AddElement => _addElement ?? (_addElement = new RelayCommand(AddElementExecute));

        private void AddElementExecute([NotNull] object newObject)
        {
            if (newObject == null)
                throw new Exception("Element to add can't be null");
            if (newObject is Element element)
            {
                AddElementCommand addElementCommand = new AddElementCommand(element, this);
                addElementCommand.Execute();
                MyCommandManager.AddToList(addElementCommand);
                if (element is PortModel)
                {
                    RenumberPorts(Elements);
                }
            }
        }

        #endregion

        #region RemoveElement Command

        /// <summary>
        /// Removes Element from Model layer
        /// </summary>
        private ICommand _removeElement;
        public ICommand RemoveElement => _removeElement ?? (_removeElement = new RelayCommand(RemoveElementExecute));

        private void RemoveElementExecute([NotNull]object toRemove)
        {
            if (toRemove == null)
                throw new Exception("Element to remove can't be null");
            if (toRemove is Element toRemoveElementVM)
            {
                RemoveElementCommand removeElementCommand = new RemoveElementCommand(toRemoveElementVM, this);
                removeElementCommand.Execute();
                MyCommandManager.AddToList(removeElementCommand);
            }
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
            if (newObject == null)
                throw new Exception("Connection to add can't be null");
            if (newObject is ConnectionModel connectionModel)
            {
                AddConnectionCommand addConnectionCommand = new AddConnectionCommand(connectionModel, this);
                addConnectionCommand.Execute();
                MyCommandManager.AddToList(addConnectionCommand);
            }
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
            if (toRemove == null)
                throw new Exception("Connection to Remove can't be null");
            if (toRemove is ConnectionModel connectionModel)
            {
                RemoveConnectionCommand removeConnectionCommand = new RemoveConnectionCommand(connectionModel, this);
                removeConnectionCommand.Execute();
                MyCommandManager.AddToList(removeConnectionCommand);
            }
        }

        #endregion

        #region RotateElements Command

        private ICommand _rotateElements;
        public ICommand RotateElements => _rotateElements ?? (_rotateElements = new RelayCommand(RotateElementsExecute));


        private void RotateElementsExecute([NotNull] object ElementsToRotate)
        {
            if (ElementsToRotate == null)
                throw new Exception("List of elements to rotate can't be null");
            if (ElementsToRotate is List<Element> elementsToRotate)
            {
                GroupRotationCommand groupRotationCommand = new GroupRotationCommand(elementsToRotate);
                groupRotationCommand.Execute();
                MyCommandManager.AddToList(groupRotationCommand);
            }
        }
        #endregion

        #region GetByID Methods

        public Element GetElementByGuid([NotNull] Guid ID)
        {
            if (ID == null)
                throw new ArgumentNullException(nameof(ID));
            foreach (Element element in Elements)
            {
                if (element.ID.Equals(ID))
                    return element;
            }
            return null;
        }

        public ConnectorModel GetConnectorModelByGuid([NotNull] Guid ID, [NotNull] IEnumerable<Element> elements)
        {
            if (ID == null)
                throw new ArgumentNullException(nameof(ID));
            foreach (Element element in elements)
            {
                foreach (ConnectorModel connector in element.Connectors)
                {
                    if (connector.ID.Equals(ID))
                        return connector;
                }
            }
            return null;
        }

        public ConnectorModel GetConnectorModelByGuid([NotNull] Guid ID)
        {
            return GetConnectorModelByGuid(ID, Elements);
        }

        public ConnectionModel GetConnectionModelByGuid([NotNull] Guid ID)
        {
            if (ID == null)
                throw new ArgumentNullException(nameof(ID));
            foreach (ConnectionModel connectionModel in ConnectionModels)
            {
                if (connectionModel.ID.Equals(ID))
                    return connectionModel;
            }
            return null;
        }

        #endregion


        void RenumberPorts(List<ElementsChain> elementsChains)
        {
            List<Element> elements = new List<Element>();
            foreach (ElementsChain elementsChain in elementsChains)
            {
                elements.AddRange(elementsChain.Elements);
            }
            RenumberPorts(elements);
        }

        void RenumberPorts(IList<Element> elements)
        {
            List<PortModel> portModels = new List<PortModel>();

            //get only Ports
            foreach (Element element in elements)
            {
                if (element is PortModel port)
                {
                    portModels.Add(port);
                }
            }

            //sort
            portModels.Sort(delegate (PortModel x, PortModel y)
            {
                if (x.Number > y.Number)
                    return 1;
                if (x.Number < y.Number)
                    return -1;
                return 0;
            });

            //revalue
            for (int i = 0; i < portModels.Count; i++)
            {
                portModels[i].Number = i + 1; //so it starts from 1, not from 0
            }
        }
    }
}