using System.Windows;
using DiagramDesigner.BlockTypes.CapacitorInParallel;
using DiagramDesigner.BlockTypes.CapacitorInSeries;
using DiagramDesigner.BlockTypes.ImportedComponent;
using DiagramDesigner.BlockTypes.InductorInParallel;
using DiagramDesigner.BlockTypes.InductorInSeries;
using DiagramDesigner.BlockTypes.LumpedComponents;
using DiagramDesigner.BlockTypes.OpenCircuit;
using DiagramDesigner.BlockTypes.Port;
using DiagramDesigner.BlockTypes.ResistorInParallel;
using DiagramDesigner.BlockTypes.ResistorInSeries;
using DiagramDesigner.BlockTypes.ShortCircuit;
using DiagramDesigner.BlockTypes.TransmissionLine;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner.BlockTypes
{
    public static class Creator
    {
        public static Element CreateElementByType(ElementType type, double width, double height, double top, double left)
        {
            Element toReturn;
            switch (type)
            {
                case ElementType.Element:
                    toReturn = new Element();
                    break;
                case ElementType.Port:
                    toReturn = new PortModel();
                    break;
                case ElementType.ResistorInSeries:
                    toReturn = new LumpedElement(ElementType.ResistorInSeries, ElementType.ResistorInSeries.ToString());
                    break;
                case ElementType.ResistorInParallel:
                    toReturn = new LumpedElement(ElementType.ResistorInParallel, ElementType.ResistorInParallel.ToString());
                    break;
                case ElementType.InductorInSeries:
                    toReturn = new LumpedElement(ElementType.InductorInSeries, ElementType.InductorInSeries.ToString());
                    break;
                case ElementType.InductorInParallel:
                    toReturn = new LumpedElement(ElementType.InductorInParallel, ElementType.InductorInParallel.ToString());
                    break;
                case ElementType.CapacitorInSeries:
                    toReturn = new LumpedElement(ElementType.CapacitorInSeries, ElementType.CapacitorInSeries.ToString());
                    break;
                case ElementType.CapacitorInParallel:
                    toReturn = new LumpedElement(ElementType.CapacitorInParallel, ElementType.CapacitorInParallel.ToString());
                    break;
                case ElementType.OpenCircuit:
                    toReturn = new OpenCircuitModel();
                    break;
                case ElementType.ShortCircuit:
                    toReturn = new ShortCircuitModel();
                    break;
                case ElementType.TransmissionLine:
                    toReturn = new TransmissionLineModel();
                    break;
                case ElementType.ImportedComponent:
                    toReturn = new ImportedComponentModel();
                    break;
                default:
                    toReturn = new Element();
                    break;
            }

            toReturn.Position = new Point(left, top);
            toReturn.Size = new Size(width, height);

            return toReturn;
        }

        public static Element CreateElementByType(ElementType type)
        {
            return CreateElementByType(type, 60, 50, 10, 10);
        }

        public static ElementVM CreateElementVMByElement(Element element)
        {
            ElementVM toReturn;
            switch (element.Type)
            {
                case ElementType.Element:
                    toReturn = new ElementVM(element);
                    break;
                case ElementType.Port:
                    toReturn = new PortVM(element);
                    break;
                case ElementType.ResistorInSeries:
                    toReturn = new ResistorInSeriesVM(element);
                    break;
                case ElementType.ResistorInParallel:
                    toReturn = new ResistorInParallelVM(element);
                    break;
                case ElementType.InductorInSeries:
                    toReturn = new InductorInSeriesVM(element);
                    break;
                case ElementType.InductorInParallel:
                    toReturn = new InductorInParallelVM(element);
                    break;
                case ElementType.CapacitorInSeries:
                    toReturn = new CapacitorInSeriesVM(element);
                    break;
                case ElementType.CapacitorInParallel:
                    toReturn = new CapacitorInParallelVM(element);
                    break;
                case ElementType.OpenCircuit:
                    toReturn = new OpenCircuitVM(element);
                    break;
                case ElementType.ShortCircuit:
                    toReturn = new ShortCircuitVM(element);
                    break;
                case ElementType.TransmissionLine:
                    toReturn = new TransmissionLineVM(element);
                    break;
                case ElementType.ImportedComponent:
                    toReturn = new ImportedComponentVM(element);
                    break;
                default:
                    toReturn = new ElementVM(element);
                    break;
            }
            return toReturn;
        }
    }
}