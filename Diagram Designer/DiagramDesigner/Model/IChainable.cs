using DiagramDesigner.Annotations;

namespace DiagramDesigner.Model
{
    public interface IChainable
    {
        [CanBeNull] ElementsChain Chain { get; set; } //if Null then element does not belong to any chain
        int IndexInChain { get; set; }
    }
}