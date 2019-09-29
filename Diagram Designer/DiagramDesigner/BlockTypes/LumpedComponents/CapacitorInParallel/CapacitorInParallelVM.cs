using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.CapacitorInParallel
{
    public class CapacitorInParallelVM : ElementVM
    {
        public CapacitorInParallelVM([NotNull] Element capacitorInParallelModel) :base(capacitorInParallelModel)
        {
        }

        public CapacitorInParallelVM()
            : this(Creator.CreateElementByType(ElementType.CapacitorInParallel))
        {
        }

        public double Capacitance
        {
            get
            {
                if (Element is LumpedElement lumpedElement)
                    return lumpedElement.C;
                else
                    throw new Exception("Model of CapacitorInParallelVM should be type of LumpedElement");
            }
            set
            {
                if (Element is LumpedElement lumpedElement)
                    lumpedElement.C = value;
                else
                    throw new Exception("Model of CapacitorInParallelVM should be type of LumpedElement");
            }
        }
    }
}