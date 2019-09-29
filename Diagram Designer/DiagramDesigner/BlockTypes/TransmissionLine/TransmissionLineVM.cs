using System;
using System.Numerics;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.TransmissionLine
{
    public class TransmissionLineVM : ElementVM
    {
        public TransmissionLineVM([NotNull] Element transmissionLineModel) :base(transmissionLineModel)
        {
        }

        public TransmissionLineVM()
            : this(new TransmissionLineModel())
        {
        }

        public double Length
        {
            get
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    return transmissionLineModel.Length;
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
            set
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    transmissionLineModel.Length = value;
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
        }
        public double AttenuationFactor
        {
            get
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    return transmissionLineModel.AttenuationFactor;
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
            set
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    transmissionLineModel.AttenuationFactor = value;
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
        }
        public double ImpedanceReal
        {
            get
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    return transmissionLineModel.Impedance.Real;
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
            set
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    transmissionLineModel.Impedance = new Complex(value, transmissionLineModel.Impedance.Imaginary);
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
        }
        public double ImpedanceImaginary
        {
            get
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    return transmissionLineModel.Impedance.Imaginary;
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
            set
            {
                if (Element is TransmissionLineModel transmissionLineModel)
                    transmissionLineModel.Impedance = new Complex(transmissionLineModel.Impedance.Real,value);
                else
                    throw new Exception("Model of TransmissionLineVM should be type of TransmissionLineModel");
            }
        }
    }
}