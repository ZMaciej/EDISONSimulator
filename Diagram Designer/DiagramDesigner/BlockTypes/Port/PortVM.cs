using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.Port
{
    public class PortVM : ElementVM
    {
        public PortVM([NotNull] Element portModel):base(portModel)
        {
        }

        public PortVM()
            : this(new PortModel())
        {
        }

        public int Number
        {
            get
            {
                if (Element is PortModel portModel)
                    return portModel.Number;
                else
                    throw new Exception("Model of PortVM should be type of PortModel");
            }
            set
            {
                if (Element is PortModel portModel)
                    portModel.Number = value;
                else
                    throw new Exception("Model of PortVM should be type of PortModel");
            }
        }

        public double Resistance
        {
            get
            {
                if (Element is PortModel portModel)
                    return portModel.PortImpedance;
                else
                    throw new Exception("Model of PortVM should be type of PortModel");
            }
            set
            {
                if (Element is PortModel portModel)
                    portModel.PortImpedance = value;
                else
                    throw new Exception("Model of PortVM should be type of PortModel");
            }
        }
    }
}