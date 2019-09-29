using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.ResistorInParallel
{
    public class ResistorInParallelVM : ElementVM
    {
        public ResistorInParallelVM([NotNull] Element resistorInParallelModel) : base(resistorInParallelModel)
        {
        }

        public ResistorInParallelVM()
            : this(Creator.CreateElementByType(ElementType.ResistorInParallel))
        {
        }

        public double Resistance
        {
            get
            {
                if (Element is LumpedElement lumpedElement)
                    return lumpedElement.R;
                else
                    throw new Exception("Model of ResistorInParallelVM should be type of LumpedElement");
            }
            set
            {
                if (Element is LumpedElement lumpedElement)
                    lumpedElement.R = value;
                else
                    throw new Exception("Model of ResistorInParallelVM should be type of LumpedElement");
            }
        }
    }
}