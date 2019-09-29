using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.ResistorInSeries
{
    public class ResistorInSeriesVM : ElementVM
    {
        public ResistorInSeriesVM([NotNull] Element resistorInSeriesModel) :base(resistorInSeriesModel)
        {
        }

        public ResistorInSeriesVM()
            : this(Creator.CreateElementByType(ElementType.ResistorInSeries))
        {
        }

        public double Resistance
        {
            get
            {
                if (Element is LumpedElement lumpedElement)
                    return lumpedElement.R;
                else
                    throw new Exception("Model of ResistorInSeriesVM should be type of LumpedElement");
            }
            set
            {
                if (Element is LumpedElement lumpedElement)
                    lumpedElement.R = value;
                else
                    throw new Exception("Model of ResistorInSeriesVM should be type of LumpedElement");
            }
        }
    }
}