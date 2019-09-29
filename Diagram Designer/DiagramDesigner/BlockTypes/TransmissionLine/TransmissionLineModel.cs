using System;
using System.Globalization;
using System.Numerics;
using System.Xml;
using DiagramDesigner.CommandManagement.Commands;
using DiagramDesigner.Model;
using MathNet.Numerics.LinearAlgebra;

namespace DiagramDesigner.BlockTypes.TransmissionLine
{
    public class TransmissionLineModel : Element
    {
        public TransmissionLineModel()
        {
            Type = ElementType.TransmissionLine;
            Name = "Transmission Line";
            CreateConnector(0, 0.5, ConnectorOrientation.Left);
            CreateConnector(1, 0.5, ConnectorOrientation.Right);

            _dimension = 2;
            S = Matrix<Complex>.Build.Dense(_dimension, _dimension);
        }

        private double _length = 1;
        public double Length
        {
            get => _length;
            set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(Length, value, nameof(Length), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _length = value;
                OnPropertyChanged(nameof(Length));
            }
        }

        private double _attenuationFactor = 0;
        public double AttenuationFactor
        {
            get => _attenuationFactor;
            set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(AttenuationFactor, value, nameof(AttenuationFactor), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _attenuationFactor = value;
                OnPropertyChanged(nameof(AttenuationFactor));
            }
        }

        private Complex _impedance = 1;
        public Complex Impedance
        {
            get => _impedance;
            set
            {
                if (MainModelCommandManager != null)
                {
                    PropertyChangedCommand propertyChangedCommand =
                        new PropertyChangedCommand(Impedance, value, nameof(Impedance), this);
                    MainModelCommandManager.AddToList(propertyChangedCommand);
                }
                _impedance = value;
                OnPropertyChanged(nameof(Impedance));
            }
        }

        public override Boolean UpdateScatteringMatrix(double frequency, Complex referenceImpedance)
        {
            //equations from http://qucs.sourceforge.net/tech/node61.html
            double omega = frequency * 2 * Math.PI;
            double c0 = 299792458; //vacuum light velocity
            Complex r = (Impedance - referenceImpedance) / (Impedance + referenceImpedance);
            Complex p = Complex.Exp(-Complex.ImaginaryOne * omega * (_length / c0) - _attenuationFactor * _length);

            Complex rSquared = r * r;
            Complex pSquared = p * p;

            S[0, 0] = (r*(1-pSquared))/(1-rSquared*pSquared);
            S[1, 1] = S[0, 0];

            S[0, 1] = (p * (1 - rSquared)) / (1 - rSquared * pSquared);
            S[1, 0] = S[0, 1];
            return true; //returns true when the ScatteringMatrix has been updated
        }

        protected override void ReadUniqueProperties(XmlReader reader)
        {
            double tempImpedanceReal;
            if (reader.Read() && reader.Name == "Length")
                Length = double.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
            else
                throw new Exception("Length of TransmissionLineModel is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "AttenuationFactor")
                AttenuationFactor = double.Parse(reader.ReadString(),CultureInfo.InvariantCulture);
            else
                throw new Exception("AttenuationFactor of TransmissionLineModel is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "ImpedanceRe")
                tempImpedanceReal = double.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
            else
                throw new Exception("ImpedanceRe of TransmissionLineModel is missing or it is in the wrong place");
            if (reader.Read() && reader.Name == "ImpedanceIm")
                Impedance = new Complex(tempImpedanceReal,double.Parse(reader.ReadString(), CultureInfo.InvariantCulture));
            else
                throw new Exception("ImpedanceIm of TransmissionLineModel is missing or it is in the wrong place");
        }

        protected override void WriteUniqueProperties(XmlWriter writer)
        {
            writer.WriteElementString("Length", Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("AttenuationFactor", AttenuationFactor.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ImpedanceRe", Impedance.Real.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("ImpedanceIm", Impedance.Imaginary.ToString(CultureInfo.InvariantCulture));
        }
    }
}