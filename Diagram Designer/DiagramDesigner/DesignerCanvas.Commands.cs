using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using DiagramDesigner.CommandManagement.Commands;
using Microsoft.Win32;
using DiagramDesigner.Controls;
using DiagramDesigner.Model;
using DiagramDesigner.ViewModel;
using DiagramDesigner.ViewModel.ViewModelBases;

namespace DiagramDesigner
{
    public partial class DesignerCanvas
    {
        public static RoutedCommand SelectAll = new RoutedCommand();
        public static RoutedCommand Rotate = new RoutedCommand();

        public DesignerCanvas()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, Open_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, SaveAs_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Print, Print_Executed));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_Enabled));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_Enabled));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_Enabled));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, Delete_Executed, Delete_Enabled));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, Undo_Executed, Undo_Enabled));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, Redo_Executed, Redo_Enabled));
            this.CommandBindings.Add(new CommandBinding(DesignerCanvas.SelectAll, SelectAll_Executed));
            this.CommandBindings.Add(new CommandBinding(DesignerCanvas.Rotate, Rotate_Executed, Rotate_Enabled));
            SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            Rotate.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));

#if DEBUG
            this.CommandBindings.Add(new CommandBinding(DesignerCanvas.Debug, Debug_Executed));
            Debug.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
#endif

            this.AllowDrop = true;
            Clipboard.Clear();
            this.LayoutTransform = ScaleCanvas;
        }

#if DEBUG
        #region DebugCommand

        public static RoutedCommand Debug = new RoutedCommand();

        private void Debug_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selection = SelectionService.CurrentSelection;
            var canvas = this.Children;
            var vm = this.DataContext;
            var model = (vm as MainVM).Project;
            var commandsHistory = model.MyCommandManager;
            System.Diagnostics.Debugger.Break();
        }
        #endregion
#endif
        
        #region New Command

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectionService.SelectAll();
            DeleteCurrentSelection();
        }

        #endregion

        #region Open Command

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM)
            {
                (mainVM.Open as RelayCommand).Run(this);
            }
        }

        #endregion

        #region Save Command

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM)
            {
                (mainVM.Save as RelayCommand).Run(this);
            }
        }

        #endregion

        #region SaveAs Command

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM)
            {
                (mainVM.SaveAs as RelayCommand).Run(this);
            }
        }

        #endregion

        #region Print Command

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectionService.ClearSelection();

            PrintDialog printDialog = new PrintDialog();

            if (true == printDialog.ShowDialog())
            {
                printDialog.PrintVisual(this, "WPF Diagram");
            }
        }

        #endregion

        #region Copy Command

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopyCurrentSelection();
        }

        private void Copy_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectionService.CurrentSelection.Any();
        }

        #endregion

        #region Paste Command

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM)
                mainVM.Project.PasteXMLStringFromClipboard();
        }

        private void Paste_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        #endregion

        #region Delete Command

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteCurrentSelection();
        }

        private void Delete_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SelectionService.CurrentSelection.Any();
        }

        #endregion

        #region Cut Command

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopyCurrentSelection();
            DeleteCurrentSelection();
        }

        private void Cut_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectionService.CurrentSelection.Any();
        }

        #endregion

        #region SelectAll Command

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectionService.SelectAll();
        }

        #endregion

        #region Undo Command
        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM)
            {
                mainVM.Project.MyCommandManager.Undo();
            }
        }

        private void Undo_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM && mainVM.Project.MyCommandManager.CanUndo)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        #endregion

        #region Redo Command
        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM)
            {
                mainVM.Project.MyCommandManager.Redo();
            }
        }

        private void Redo_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is MainVM mainVM && mainVM.Project.MyCommandManager.CanRedo)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }
        #endregion

        #region Helper Methods

        private XElement LoadSerializedDataFromFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    return XElement.Load(openFile.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        void SaveFile(XElement xElement)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (saveFile.ShowDialog() == true)
            {
                try
                {
                    xElement.Save(saveFile.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private XElement LoadSerializedDataFromClipBoard()
        {
            if (Clipboard.ContainsData(DataFormats.Xaml))
            {
                String clipboardData = Clipboard.GetData(DataFormats.Xaml) as String;

                if (String.IsNullOrEmpty(clipboardData))
                    return null;
                try
                {
                    return XElement.Load(new StringReader(clipboardData));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        private void CopyCurrentSelection()
        {
            if (this.DataContext is MainVM mainVM)
            {
                IEnumerable<Connection> connectionSelection = SelectionService.CurrentSelection.OfType<Connection>();
                IEnumerable<ConnectionModel> connectionModels = from connection in connectionSelection
                    where (connection.DataContext is ConnectionVM)
                    select (connection.DataContext as ConnectionVM).ConnectionModel;

                IEnumerable<DesignerItem> itemSelection = SelectionService.CurrentSelection.OfType<DesignerItem>();
                IEnumerable<Element> elements = from designerItem in itemSelection
                    where (designerItem.DataContext is ElementVM)
                    select (designerItem.DataContext as ElementVM).Element;

                mainVM.Project.CopyXMLStringToClipboard(elements,connectionModels);
            }
        }

        private void DeleteCurrentSelection()
        {
            IEnumerable<Connection> connectionSelection = SelectionService.CurrentSelection.OfType<Connection>();
            IEnumerable<ConnectionModel> connectionModels = from connection in connectionSelection
                where (connection.DataContext is ConnectionVM)
                select (connection.DataContext as ConnectionVM).ConnectionModel;

            IEnumerable<DesignerItem> itemSelection = SelectionService.CurrentSelection.OfType<DesignerItem>();
            IEnumerable<Element> elements = from designerItem in itemSelection
                where (designerItem.DataContext is ElementVM)
                select (designerItem.DataContext as ElementVM).Element;

            if (this.DataContext is MainVM MainVM)
            {
                DeleteCommand newDeleteCommand = new DeleteCommand(elements,connectionModels,MainVM.Project);
                MainVM.Project.MyCommandManager.AddToList(newDeleteCommand);
                newDeleteCommand.Execute();
            }
            
            UpdateZIndex();
        }

        private void UpdateZIndex()
        {
            List<UIElement> ordered = (from UIElement item in this.Children
                                       orderby Canvas.GetZIndex(item as UIElement)
                                       select item as UIElement).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                Canvas.SetZIndex(ordered[i], i);
            }
        }

        private static Rect GetBoundingRectangle(IEnumerable<DesignerItem> items)
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach (DesignerItem item in items)
            {
                x1 = Math.Min(Canvas.GetLeft(item), x1);
                y1 = Math.Min(Canvas.GetTop(item), y1);

                x2 = Math.Max(Canvas.GetLeft(item) + item.ActualWidth, x2);
                y2 = Math.Max(Canvas.GetTop(item) + item.ActualHeight, y2);
            }

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        public void GetConnectors(DependencyObject parent, List<Connector> connectors)
        {
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is Connector)
                {
                    connectors.Add(child as Connector);
                }
                else
                    GetConnectors(child, connectors);
            }
        }

        private Connector GetConnector(Guid itemID, String connectorName)
        {
            DesignerItem designerItem = (from item in this.Children.OfType<DesignerItem>()
                                         where item.ID == itemID
                                         select item).FirstOrDefault();

            Control connectorDecorator = designerItem.Template.FindName("PART_ConnectorDecorator", designerItem) as Control;
            connectorDecorator.ApplyTemplate();

            return connectorDecorator.Template.FindName(connectorName, connectorDecorator) as Connector;
        }

        #endregion

        #region Rotate Command

        private void Rotate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selectedItems = from item in SelectionService.CurrentSelection.OfType<DesignerItem>()
                                where item.ParentID == Guid.Empty
                                select item;
            List<Element> selectedElements = new List<Element>();
            foreach (DesignerItem item in selectedItems)
            {
                if(item.DataContext is ElementVM elementVM)
                selectedElements.Add(elementVM.Element);
            }

            ((this.DataContext as MainVM).RotateElements as RelayCommand).Run(selectedElements);
        }

        private void Rotate_Enabled(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SelectionService.CurrentSelection.Count() > 0;
        }

        #endregion

        #region Zoom Command

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            //scroll zooming related to mouse position
            if (this.Parent is ScrollViewer)
            {
                double scale = 1.1;
                if (e.Delta < 0)
                    scale = 1 / scale;

                this.ScaleCanvas.ScaleX *= scale;
                this.ScaleCanvas.ScaleY *= scale;

                //It prevents being not able to reach 100% zoom after few rescales
                //Situation could be posible becouse 1/1.1 is number that must be rounded. With in long term using value would drift away and it would be hard to reach 100% again
                //Code set 100% zoom every time it is nearby
                if (this.ScaleCanvas.ScaleX < 1.1 && this.ScaleCanvas.ScaleX > 1)
                {
                    this.ScaleCanvas.ScaleX = 1;
                    this.ScaleCanvas.ScaleY = 1;
                }

                ScrollViewer parent;
                parent = this.Parent as ScrollViewer;

                Point mousePosition = Mouse.GetPosition(parent);

                double desiredViewportHeight = mousePosition.Y;
                double newVerticalOffset = ((parent.VerticalOffset + desiredViewportHeight) * scale - desiredViewportHeight);

                double desiredViewportWidth = mousePosition.X;
                double newHorizontalOffset = ((parent.HorizontalOffset + desiredViewportWidth) * scale - desiredViewportWidth);

                parent.ScrollToHorizontalOffset(newHorizontalOffset);
                parent.ScrollToVerticalOffset(newVerticalOffset);
            }
        }

        #endregion
    }
}