using System;
using System.Globalization;
using System.Numerics;
using System.Xml;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.Model;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.BlockTypes.Port
{
    public class PortModel : Element
    {
        public PortModel()
        {
            Type = ElementType.Port;
            Name = "Port";
            CreateConnector(1, 0.5, ConnectorOrientation.Right);

            _dimension = 1;
            S = Matrix<Complex>.Build.Dense(_dimension, _dimension);
        }


        private int _number = Int32.MaxValue;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        private double _portImpedance = 50;
        public double PortImpedance
        {
            get => _portImpedance;
            set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(PortImpedance, value, nameof(PortImpedance), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _portImpedance = value;
                OnPropertyChanged(nameof(PortImpedance));
            }
        }

        private Complex _prevReferenceImpedance;
        private double _prevResistance;

        public override Boolean UpdateScatteringMatrix(double frequency, Complex referenceImpedance)
        {
            if (!_prevReferenceImpedance.Equals(referenceImpedance) || !_prevResistance.Equals(_portImpedance))
            {
                S[0, 0] = (_portImpedance - referenceImpedance) / (referenceImpedance + _portImpedance);
                _prevReferenceImpedance = referenceImpedance;
                _prevResistance = _portImpedance;
                return true; //returns true when the ScatteringMatrix has been updated
            }
            return false; //returns false when the ScatteringMatrix stays the same
        }

        protected override void ReadUniqueProperties(XmlReader reader)
        {
            if (reader.Read() && reader.Name == "PortImpedance") PortImpedance = double.Parse(reader.ReadString());
            else throw new Exception("Impedance of PortModel is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "Number") Number = int.Parse(reader.ReadString());
            else throw new Exception("Number of PortModel is missing or it is in the wrong place");
        }

        protected override void WriteUniqueProperties(XmlWriter writer)
        {
            writer.WriteElementString("PortImpedance", PortImpedance.ToString(new CultureInfo("en-US", false)));
            writer.WriteElementString("Number",Number.ToString(new CultureInfo("en-US", false)));
        }
    }
}