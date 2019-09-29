using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DiagramDesigner.BlockTypes;
using DiagramDesigner.CommandManagement;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.ViewModel.ViewModelBases;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.Model
{
    public class Element : NotifyPropertyChangedBase, IXmlSerializable, IChainable
    {
        protected int _dimension;
        public int Dimension
        {
            get => _dimension;
        }
        protected Matrix<Complex> S;

        public Matrix<Complex> ScatteringMatrix
        {
            get => S;
        }

        public Element()
        {
            ID = Guid.NewGuid();
            Name = "Element";
            Type = ElementType.Element;
            Rotation = 0;
        }

        public ObservableCollection<ConnectorModel> Connectors { get; } = new ObservableCollection<ConnectorModel>();

        #region ID Property

        private Guid _id = Guid.NewGuid();
        public Guid ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        #endregion

        #region Name Property

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (MainModelCommandManager != null) //if MainVMCommandManager is null it means that ElementVM is not added to list yet
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(Name, value, nameof(Name), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        #endregion

        #region Position Property
        /// <summary>
        /// Position of Top Left Corner
        /// </summary>

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        #endregion

        #region Size Property

        private Size _size;

        public Size Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        #endregion

        #region Type Property
        private ElementType _type;
        public ElementType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }
        #endregion

        #region Rotation Property

        private int _rotation;

        public int Rotation
        {
            get => _rotation;
            set
            {
                _rotation = ((value % 4) + 4) % 4; //this operation gives us certainty that _rotation whatever is in the value always be in range of 0-3
                OnPropertyChanged(nameof(Rotation));
            }
        }

        #endregion

        public MyCommandManager MainModelCommandManager { get; set; } //when it is null it means that the element is not added to list of elements
        protected void CreateConnector(double horizontalPosition, double verticalPosition, ConnectorOrientation connectorOrientation)
        {
            ConnectorModel newConnectorModel = new ConnectorModel(this)
            {
                Position = new Point(horizontalPosition, verticalPosition),
                Orientation = connectorOrientation
            };
            Connectors.Add(newConnectorModel);
            newConnectorModel.Number = Connectors.Count - 1;
        }

        #region XML Read Write

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            //in this step, Type should already be read
            ReadBaseProperties(reader);
            ReadUniqueProperties(reader);
            while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Element")) //go to the end of Element
                reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Element");
            WriteBaseProperties(writer);
            WriteUniqueProperties(writer);
            writer.WriteEndElement();
        }

        private void ReadBaseProperties(XmlReader reader)
        {
            int readConnectorsCount = 0;
            ConnectorModel connectorModel = new ConnectorModel(this);
            while (reader.Read() && reader.NodeType == XmlNodeType.Element && reader.Name == "Connector") //while it is the beginning of the Connector
            {
                if (readConnectorsCount < Connectors.Count)
                {
                    connectorModel = Connectors[readConnectorsCount];
                    readConnectorsCount++;
                }
                else
                {
                    connectorModel = new ConnectorModel(this);
                    Connectors.Add(connectorModel);
                    connectorModel.Number = Connectors.Count - 1;
                    readConnectorsCount++;
                }

                if (reader.Read() && reader.Name == "ID") connectorModel.ID = Guid.Parse(reader.ReadString());
                else throw new Exception("ID of Connector is missing or it is in the wrong place");
                if (reader.Read() && reader.Name == "Position") connectorModel.Position = Point.Parse(reader.ReadString());
                else throw new Exception("Position of Connector is missing or it is in the wrong place");
                if (reader.Read() && reader.Name == "Orientation" && Enum.TryParse(reader.ReadString(), out ConnectorOrientation orientation))
                {
                    connectorModel.Orientation = orientation;
                }
                else
                    throw new Exception(reader.ReadString() + " is unknown Orientation of Connector");
                while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Connector")) //go to end of Connector
                    reader.Read();
            }

            if (reader.Name == "ID") ID = Guid.Parse(reader.ReadString());
            else throw new Exception("ID of Element is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "Name") Name = reader.ReadString();
            else throw new Exception("Name of Element is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "Position") Position = Point.Parse(reader.ReadString());
            else throw new Exception("Position of Element is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "Size") Size = System.Windows.Size.Parse(reader.ReadString());
            else throw new Exception("Size of Element is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "Rotation") Rotation = Int32.Parse(reader.ReadString());
            else throw new Exception("Rotation of Element is missing or it is in the wrong place");
        }

        private void WriteBaseProperties(XmlWriter writer)
        {
            writer.WriteElementString("Type", Type.ToString());
            foreach (ConnectorModel connectorModel in Connectors)
            {
                writer.WriteStartElement("Connector");
                writer.WriteElementString("ID", connectorModel.ID.ToString());
                writer.WriteElementString("Position", connectorModel.Position.ToString(new CultureInfo("en-US", false)));
                writer.WriteElementString("Orientation", connectorModel.Orientation.ToString());
                writer.WriteEndElement();
            }
            writer.WriteElementString("ID", ID.ToString());
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Position", Position.ToString(new CultureInfo("en-US", false)));
            writer.WriteElementString("Size", Size.ToString(new CultureInfo("en-US", false)));
            writer.WriteElementString("Rotation", Rotation.ToString());
        }

        protected virtual void ReadUniqueProperties(XmlReader reader)
        {
        }

        protected virtual void WriteUniqueProperties(XmlWriter writer)
        {
        }

        public virtual Boolean UpdateScatteringMatrix(double frequency, Complex referenceImpedance)
        {
            return false; //returns true when the ScatteringMatrix has been updated
        }

        public List<Element> NeighboringElements()
        {
            List<Element> toReturn = new List<Element>();
            foreach (ConnectorModel connectorModel in Connectors)
            {
                if (connectorModel.Connected)
                {
                    foreach (ConnectionModel connectionModel in connectorModel.ConnectionModels)
                    {
                        Element neighboringElement;

                        if (connectionModel.SinkConnector == connectorModel)
                        {
                            neighboringElement = connectionModel.SourceConnector.Parent;
                        }
                        else
                        {
                            neighboringElement = connectionModel.SinkConnector.Parent;
                        }
                        toReturn.Add(neighboringElement);
                    }
                }
            }
            return toReturn;
        }

        #endregion

        public ElementsChain Chain { get; set; }
        public int IndexInChain { get; set; }
    }
}