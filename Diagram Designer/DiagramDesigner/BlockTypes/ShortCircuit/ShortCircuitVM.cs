using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.ShortCircuit
{
    public class ShortCircuitVM : ElementVM
    {
        public ShortCircuitVM([NotNull] Element shortCircuitModel) :base(shortCircuitModel)
        {
        }

        public ShortCircuitVM()
            : this(new ShortCircuitModel())
        {
        }
    }
}