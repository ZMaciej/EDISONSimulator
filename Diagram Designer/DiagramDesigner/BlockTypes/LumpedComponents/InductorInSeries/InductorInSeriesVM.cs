using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.InductorInSeries
{
    public class InductorInSeriesVM : ElementVM
    {
        public InductorInSeriesVM([NotNull] Element inductorInSeriesModel) :base(inductorInSeriesModel)
        {
        }

        public InductorInSeriesVM()
            : this(Creator.CreateElementByType(ElementType.InductorInSeries))
        {
        }

        public double Impedance
        {
            get
            {
                if (Element is LumpedElement lumpedElement)
                    return lumpedElement.L;
                else
                    throw new Exception("Model of InductorInSeriesVM should be type of LumpedElement");
            }
            set
            {
                if (Element is LumpedElement lumpedElement)
                    lumpedElement.L = value;
                else
                    throw new Exception("Model of InductorInSeriesVM should be type of LumpedElement");
            }
        }
    }
}