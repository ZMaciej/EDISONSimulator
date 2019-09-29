using DiagramDesigner.Annotations;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes.OpenCircuit
{
    public class OpenCircuitVM : ElementVM
    {
        public OpenCircuitVM([NotNull] Element openCircuitModel):base(openCircuitModel)
        {
        }

        public OpenCircuitVM()
            : this(new OpenCircuitModel())
        {
        }
    }
}