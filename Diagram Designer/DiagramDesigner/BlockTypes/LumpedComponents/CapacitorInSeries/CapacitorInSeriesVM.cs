using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.CapacitorInSeries
{
    public class CapacitorInSeriesVM : ElementVM
    {
        public CapacitorInSeriesVM([NotNull] Element capacitorInSeriesModel) :base(capacitorInSeriesModel)
        {
        }

        public CapacitorInSeriesVM()
            : this(Creator.CreateElementByType(ElementType.CapacitorInSeries))
        {
        }

        public double Capacitance
        {
            get
            {
                if (Element is LumpedElement lumpedElement)
                    return lumpedElement.C;
                else
                    throw new Exception("Model of CapacitorInSeriesVM should be type of LumpedElement");
            }
            set
            {
                if (Element is LumpedElement lumpedElement)
                    lumpedElement.C = value;
                else
                    throw new Exception("Model of CapacitorInSeriesVM should be type of LumpedElement");
            }
        }
    }
}