using System;
using DiagramDesigner.Annotations;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.InductorInParallel
{
    public class InductorInParallelVM : ElementVM
    {
        public InductorInParallelVM([NotNull] Element inductorInParallelModel) :base(inductorInParallelModel)
        {
        }

        public InductorInParallelVM()
            : this(Creator.CreateElementByType(ElementType.InductorInParallel))
        {
        }

        public double Impedance
        {
            get
            {
                if (Element is LumpedElement lumpedElement)
                    return lumpedElement.L;
                else
                    throw new Exception("Model of InductorInParallelVM should be type of LumpedElement");
            }
            set
            {
                if (Element is LumpedElement lumpedElement)
                    lumpedElement.L = value;
                else
                    throw new Exception("Model of InductorInParallelVM should be type of LumpedElement");
            }
        }
    }
}