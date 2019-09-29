using System;
using System.Globalization;
using System.Numerics;
using System.Xml;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.Model;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.BlockTypes.LumpedComponents
{
    public class LumpedElement : Element
    {
        private Complex _z;
        private Complex _y;
        protected double _r = 50;
        protected double _l = 1e-9;
        protected double _c = 1e-12;

        private ConnectionType _connectionType;
        private LumpedComponentType _lumpedComponentType;

        public LumpedElement(ElementType elementType, string elementName)
        {
            SetComponentAndConnectionType(elementType);
            _dimension = 2;
            S = Matrix<Complex>.Build.Dense(_dimension, _dimension);
            Type = elementType;
            Name = elementName;
            CreateConnector(0, 0.5, ConnectorOrientation.Left); //left connector
            CreateConnector(1, 0.5, ConnectorOrientation.Right); //right connector
        }

        public double R
        {
            get => _r;
            set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(R, value, nameof(R), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _r = value;
                OnPropertyChanged(nameof(R));
            }
        }

        public double L
        {
            get => _l;
            set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(L, value, nameof(L), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _l = value;
                OnPropertyChanged(nameof(L));
            }
        }

        public double C { get => _c; set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(C, value, nameof(C), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _c = value;
                OnPropertyChanged(nameof(C));
            }
        }

        public override Boolean UpdateScatteringMatrix(double frequency, Complex referenceImpedance)
        {
            double omega = frequency * 2 * Math.PI;
            if (_connectionType == ConnectionType.Series)
            {
                if (_lumpedComponentType == LumpedComponentType.Resistor)
                    _z = _r;
                else if (_lumpedComponentType == LumpedComponentType.Capacitor)
                    _z = new Complex(0, -1 / (omega * _c));
                else if (_lumpedComponentType == LumpedComponentType.Inductor)
                    _z = new Complex(0, omega * _l);

                Complex Ds = _z + 2 * referenceImpedance;

                S[0, 0] = _z / Ds;
                S[0, 1] = (2 * referenceImpedance) / Ds;
                S[1, 0] = ScatteringMatrix[0, 1];
                S[1, 1] = ScatteringMatrix[0, 0];
            }
            else
            if (_connectionType == ConnectionType.Parallel)
            {
                if (_lumpedComponentType == LumpedComponentType.Resistor)
                    _y = 1/_r;
                else if (_lumpedComponentType == LumpedComponentType.Capacitor)
                    _y = new Complex(0, omega * _c);
                else if (_lumpedComponentType == LumpedComponentType.Inductor)
                    _y = new Complex(0, -1 / (omega * _l));

                Complex referenceAdmittance = 1 / referenceImpedance;
                Complex Ds = _y + 2 * referenceAdmittance;

                S[0, 0] = -_y / Ds;
                S[0, 1] = (2 * referenceAdmittance) / Ds;
                S[1, 0] = ScatteringMatrix[0, 1];
                S[1, 1] = ScatteringMatrix[0, 0];
            }

            return true; //returns true when the ScatteringMatrix has been updated
        }

        void SetComponentAndConnectionType(ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.ResistorInSeries:
                    _lumpedComponentType = LumpedComponentType.Resistor;
                    _connectionType = ConnectionType.Series;
                    break;
                case ElementType.ResistorInParallel:
                    _lumpedComponentType = LumpedComponentType.Resistor;
                    _connectionType = ConnectionType.Parallel;
                    break;
                case ElementType.InductorInSeries:
                    _lumpedComponentType = LumpedComponentType.Inductor;
                    _connectionType = ConnectionType.Series;
                    break;
                case ElementType.InductorInParallel:
                    _lumpedComponentType = LumpedComponentType.Inductor;
                    _connectionType = ConnectionType.Parallel;
                    break;
                case ElementType.CapacitorInSeries:
                    _lumpedComponentType = LumpedComponentType.Capacitor;
                    _connectionType = ConnectionType.Series;
                    break;
                case ElementType.CapacitorInParallel:
                    _lumpedComponentType = LumpedComponentType.Capacitor;
                    _connectionType = ConnectionType.Parallel;
                    break;
                default: throw new Exception("LumpedElement cannot be set as " + elementType);
            }
        }

        protected override void ReadUniqueProperties(XmlReader reader)
        {
            switch (_lumpedComponentType)
            {
                case LumpedComponentType.Resistor:
                    if (reader.Read() && reader.Name == "R") R = double.Parse(reader.ReadString(), new CultureInfo("en-US", false));
                    else throw new Exception("Resistance of Element is missing or it is in the wrong place");
                    break;
                case LumpedComponentType.Capacitor:
                    if (reader.Read() && reader.Name == "C") C = double.Parse(reader.ReadString(), new CultureInfo("en-US", false));
                    else throw new Exception("Capacitance of Element is missing or it is in the wrong place");
                    break;
                case LumpedComponentType.Inductor:
                    if (reader.Read() && reader.Name == "L") L = double.Parse(reader.ReadString(), new CultureInfo("en-US", false));
                    else throw new Exception("Inductance of Element is missing or it is in the wrong place");
                    break;
                default:
                    throw new Exception("The lumpedComponentType is unknown");
            }
        }

        protected override void WriteUniqueProperties(XmlWriter writer)
        {
            switch (_lumpedComponentType)
            {
                case LumpedComponentType.Resistor:
                    writer.WriteElementString("R", R.ToString(new CultureInfo("en-US", false)));
                    break;
                case LumpedComponentType.Capacitor:
                    writer.WriteElementString("C", C.ToString(new CultureInfo("en-US", false)));
                    break;
                case LumpedComponentType.Inductor:
                    writer.WriteElementString("L", L.ToString(new CultureInfo("en-US", false)));
                    break;
                default:
                    throw new Exception("The lumpedComponentType is unknown");
            }
        }
    }

    public enum ConnectionType
    {
        Series,
        Parallel
    }

    public enum LumpedComponentType
    {
        Resistor,
        Capacitor,
        Inductor
    }
}