using System;
using System.Collections.Generic;
using System.Linq;
using DiagramDesigner.ViewModel;

namespace DiagramDesigner
{
    internal class SelectionService
    {
        private DesignerCanvas designerCanvas;
        private Object EmptyObject = new Object();

        private List<ISelectable> currentSelection;
        internal List<ISelectable> CurrentSelection
        {
            get
            {
                if (currentSelection == null)
                    currentSelection = new List<ISelectable>();
                return currentSelection;
            }
        }

        //setting selected item to use in propertyGrid
        private void SetSelectedItem()
        {
            if (currentSelection.Count() == 1 && currentSelection[0] is DesignerItem selectedItem)
            {
                (designerCanvas.DataContext as MainVM).SelectedElement = selectedItem.DataContext as ElementVM;
            }
            else
            {
                (designerCanvas.DataContext as MainVM).SelectedElement = EmptyObject;
            }
        }

        public SelectionService(DesignerCanvas canvas)
        {
            this.designerCanvas = canvas;
        }

        internal void SelectItem(ISelectable item)
        {
            this.ClearSelection();
            this.AddToSelection(item);
        }

        internal void AddToSelection(ISelectable item)
        {
            if (!item.IsSelected)
            {
                item.IsSelected = true;
                CurrentSelection.Add(item);
                SetSelectedItem();
            }
        }

        internal void RemoveFromSelection(ISelectable item)
        {
            if (item.IsSelected)
            {
                item.IsSelected = false;
                CurrentSelection.Remove(item);
                SetSelectedItem();
            }
        }

        internal void ClearSelection()
        {
            for (int i = CurrentSelection.Count - 1; i >= 0; i--)
                RemoveFromSelection(CurrentSelection[i]);
        }

        internal void SelectAll()
        {
            var list = designerCanvas.Children.OfType<ISelectable>();
            for (int i = list.Count() - 1; i >= 0; i--)
                AddToSelection(list.ElementAt(i));
        }
    }
}
