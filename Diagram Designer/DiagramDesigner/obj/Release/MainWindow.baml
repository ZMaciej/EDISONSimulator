   M S B A M L   `   `   `  ���� J  FDiagramDesigner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:DiagramDesignerDiagramDesigner  C&clr-namespace:DiagramDesigner.ControlsDiagramDesigner.Controls  E'clr-namespace:DiagramDesigner.ViewModelDiagramDesigner.ViewModel       DiagramDesigner.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     +sclr-namespace:DiagramDesigner     4c&clr-namespace:DiagramDesigner.Controls     R NOxyPlot.Wpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf  0vm'clr-namespace:DiagramDesigner.ViewModel  X TXceed.Wpf.Toolkit, Version=3.5.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit    � WindowStartupLocation$  CenterScreen=� � Title$ EDISON Simulator��$	��800��$
��1200��$y�Visible=�
������� � 	Resources    DesignerCanvasContextMenu.  (System.Windows.Input.ApplicationCommands   Paste m� Command m� Icon$   DiagramDesigner.DesignerCanvas   	SelectAll&    DiagramDesigner.ViewModel.MainVM 
 MainVm��%�   &	        &	 �     �� .@�m� $��Paste��# Z  �� $ ��Resources/Images/Paste.png��m� $��
Select All��# Z  �� $$��Resources/Images/SelectAll.png��  .���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* �.+�o� $��0��  MenuContent#��[ ���#f3f3f3�� $��1��.+��� $	��Top=�|�.  $��2�����#dddddd$��Stretch=�$��0 0 0 0q���  �� Orientation$ 
Horizontal=����#f3f3f3	 � DataContext#	 [ .+��� $��3 0 0 3q�$��Center=�.+��� $��80��$��3 2 0 2q�.���� 
 �� TextWrapping$

 Wrap=�$��Reference _Impedance:��.���� $��60������ *ReferenceImpedance+ �� Mode$ TwoWay=��� |�.  $��2�����#dddddd$��Stretch=�$��3 0 0 0q��� .+��� $��Frequencies [GHz]:��$��Center=�$��3 2 0 0q��� $ 
Horizontal=�.+��� $��3 0 0 3q�.+��� $��From:��$��3 2 0 2q��� $��60������ *
FromFreq+$ TwoWay=��� $��3 0 0 3q�.+��� $	��To:��$��3 2 0 2q��� $��60������ *ToFreq+$ TwoWay=��� $��3 0 0 3q�.+��� $��Step:��$��3 2 0 2q��� $��60������ *Step+$ TwoWay=��� |�.  $��2�����#dddddd$��Stretch=�$��3 0 0 0q��� $��3 3 0 3q����#f3f3f3
������ ���� *
Simulate+.���� $ 
Horizontal=�.+��� $ ��Resources/Images/Start.png��$��16��$��2q��� $��Simulate��$��2q��� |�.  $��2�����#dddddd$��Stretch=�$��3 0 0 0q��� $��Center=�$��Center=�$��3 0 0 0q� d� 	IsChecked �� *AutoConnectChecked+$ TwoWay=����� *AutoConnectChanged+���� *	IsChecked+ �� RelativeSource �� *Self+.���� .+��� $��Center=�Auto�� $��Center=�	Connect�� |�.  $��2�����#dddddd$��Stretch=�$��3 0 0 0q��� $	��Top=�|�.  $��2�����#dddddd$��Stretch=�$��0 0 0 0q�� $��2��$��2�� � ColumnDefinitions �� $	��265 ��� .+��� $��0��.+��� $
��Auto=�.���� $��5q�.+�9� $��Ports�� 	 Ports#��[  9� 
IsExpanded . 9� $��Transmission Lines��  TransmissionLines#��[  . 9� $��Lumped Components��  LumpedComponents#��[  . �� $
��Auto=�.��1  +Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid  -     Name$ PropertyGrid��$��5q�#	 [    SelectedObject �� *SelectedElement+�� |�.  $��2�����#f3f3f3$��Stretch=�$��Right=��� $��1��$��1,3,1,1q�.@��� $��Diagram��.���� $
��Auto=�$
��Auto=�.��  -     Name$ 
MyDesigner��|�. #	 [   WindowBackgroundBrush#��[ $��10q� � ContextMenu# [   � FocusVisualStyle Z�    Elements �� *
ElementsVM+   Connections �� *ConnectionsVM+�� $
��Plot��#	 [ .���  �� $
��Auto ��� .+�o� .@�m� $��Save As��.@�m� $	��PNG�� �� *PlotVM.ExportToPngCommand+m� $	��sNp�� �� *ExportToSnpCommand+m� $	��CSV��.@�m� $��Module[dB] Phase�� �� *ExportToCsvMdBPCommand+m� $��Module Phase�� �� *ExportToCsvMPCommand+m� $��Real Imaginary�� �� *ExportToCsvRICommand+� $��3q�$��1�� �� �� $
��Auto �.+�  OxyPlot.Wpf.PlotView  $��0��   Model �� *PlotVM.PlotModel+]� $��1��.+��� $	��Top=�$ Vertical=�$	��Top=�#	 [ .+��� $��0q�
������ ���� *PlotVM.TypeRadioButtons+����  �� 
TargetType" ��.���  �� Property �@��
IsSelected �� Value ��  �� Path$ 	IsChecked�$ TwoWay=���  � �� ��  �� 
TargetType" ��.R��� ���� .R�	� ���� *Name+ �� $ 
IsSelected�$ TwoWay=� �� *FindAncestor+ �� AncestorType" ������ *	Changed+���� *	IsChecked+ �� *Self+�� |�.  $��2�����#f3f3f3$��Bottom=�$��Stretch=�$��0 2 0 2q��� $��
Select All������ *PlotVM.SelectAllChanged+���� *	IsChecked+ �� *Self+�� $��Bottom=�$
��Auto=�.���� ���� *PlotVM.PlotsToShow+���� .R��� ���� *Name+ �� *
IsSelected+���� *	Changed+���� *	IsChecked+ �� *Self+