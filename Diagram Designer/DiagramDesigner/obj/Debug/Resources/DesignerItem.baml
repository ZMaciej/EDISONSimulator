   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5      J FDiagramDesigner, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:DiagramDesignerDiagramDesigner 5      C&clr-namespace:DiagramDesigner.ControlsDiagramDesigner.Controls 5      o<clr-namespace:DiagramDesigner.BlockTypes.BlockTypeConverters.DiagramDesigner.BlockTypes.BlockTypeConverters 5      G(clr-namespace:DiagramDesigner.ConvertersDiagramDesigner.Converters 5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   DesignerItemContextMenu.   (System.Windows.Input.ApplicationCommands
     Cut m� Command m� Icon    Copy    Paste    DeleteR  LDiagramDesigner.BlockTypes.BlockTypeConverters.BlockTypeToPathStyleConverter ! BlockTypeToPathStyleConverter^  XDiagramDesigner.BlockTypes.BlockTypeConverters.BlockTypeToContentPresenterStyleConverter - )BlockTypeToContentPresenterStyleConverterO  IDiagramDesigner.Converters.OrientationToLabelHorizontalAlignmentConverter 2 .OrientationToLabelHorizontalAlignmentConverterM  GDiagramDesigner.Converters.OrientationToLabelVerticalAlignmentConverter 0 ,OrientationToLabelVerticalAlignmentConverterA  ;DiagramDesigner.Converters.PortNumberToLabelMarginConverter $  PortNumberToLabelMarginConverter  DiagramDesigner.Connector �� 
TargetType �� Property �� Value4  .DiagramDesigner.Controls.RelativePositionPanel	 �� RelativeSource
 �� Path �� 
TargetType ��Name J� IsHitTestVisible �� Mode �� AncestorType �� 	Converter �� Mode `� 	Converter �� Triggers �� Value �� Binding �� 
TargetName  ConnectorDecoratorTemplate  ResizeCornerTemplate*  $DiagramDesigner.Controls.ResizeThumb  ResizeBarStyle 	 ResizeDecoratorTemplate J� Opacity J� SnapsToDevicePixels � Cursor(	  "DiagramDesigner.Controls.DragThumb"
  DiagramDesigner.DesignerItem 
 Description � DataContext � ContextMenu F�Name ��Name 	 Name ��Name  P� Property! P� Value�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      .sys&clr-namespace:System;assembly=mscorlib  5      #sclr-namespace:DiagramDesigner  5      ,c&clr-namespace:DiagramDesigner.Controls  5      TblockTypeConverters<clr-namespace:DiagramDesigner.BlockTypes.BlockTypeConverters  5      7
converters(clr-namespace:DiagramDesigner.Converters  5      %�  &	        &	 �    &	 �    &	 �    &	 �    &	 �    '  �    7  7  7  &	 �    &	 	    &	 �	    &		 �	    7  7  7  7  7  7  7  7  '	  �    7  &	
 �    '
  K    7   7  7  7 	 7  7 
 �� 5	      .@�5
   
   m� $	��Cut��6   # Z  6     5      �� 5      $��Images/Cut.png��6   $��16��60   5      m� 5   
   $
��Copy��6   # Z 6!    5      �� 5      $��Images/Copy.png��6   $��16��61   5      m� 5   
   $��Rotate��6    6#   �� *Rotate+ 5      �� 5      $��Images/Redo.png��6   $��16��61   5      m� 5   
   $��Paste��6   # Z 6"    5      �� 5      $��Images/Paste.png��6   $��16��62   5      m� 5   
   $��Delete��6   # Z 6#    5      �� 5       $��Images/Delete.png��6   $��16��63   5"      5#        5%      68     5&      6D     5'      6@     5(      6>     5)      62   �� 5,      "  6   .�5-   
   ��  � ��$ 8��6"   �� 5.   
    � ��$ 8��6#   �� 5/   
    �@�Cursor$ Cross��6#   �� 50   
    �@J�SnapsToDevicePixels . 60   �� 51   
    �@ RelativePosition 6E   �� 	 �� *Self+$
 ViewModel.Position��� 52   
    �@ Orientation 6(   �� 	 �� *Self+$
 ViewModel.Orientation��� 53   
   
 �@ ID 6   �� 	 �� *Self+$
 ViewModel.ID��� 54   
    � �� 55      �� 56      "  6"   .R�57      � .+�59      �� 
������ $��-2q�67   �� 5:      $ 	Rectangle��6$   
�������$��1��6G   ���	#AA0000806[   �� 5;      $��10(�5>   $    .  ��5;   $   �� 	 �� $ FindAncestor=�$ s:ConnectorL�$
 ViewModel.Number���5<   $   �� 	 �� $ FindAncestor=�$ s:ConnectorL�$
 ViewModel.Orientation�8   $ OneWay=���5=   $   �� 	 �� $ FindAncestor=�$ s:ConnectorL�$
 ViewModel.Orientation�8  $ OneWay=���5@      `� 5A   "   8  .1�5B   &   �� $
 ViewModel.Number�6|   $ OneWay=�6�   	 6.   �� $ FindAncestor=�$ s:ConnectorL�6�   �� 5C   &   $
 ViewModel.Orientation�6|   $ OneWay=�6�   	 6.   �� $ FindAncestor=�$ s:ConnectorL�6�   5D   #   5F      5G       5H      �� 5I      $
 true��6x    6&   �� 	 �� *Self+$
 ViewModel.Connected�.a�5J      �� $ 	Rectangle��6%    � �� �#A6A6BA6L   5K      5M      5O      5P      �� 5S      " ��69   .R�5T   
     $��-4q�6"   5U      �� 5X      "  63   .R�5Y   
   � .+�5[      �� 
������ $��-2q�6+   �� 5\      
���   �$��1��6'   ���#eef6;   5]      5^      �� 5_      " ��6#   .�5`   
   ��  � ��
 �   �5a      5b      �� 5e      " ��66   .R�5f   
   � $	 0.5Q�6    . .+�5g      �� $��1��6   $��Stretch=�6"   $
��Left=�6>   $��-4 0 0 0q�6Y   8��  6k   �� 5h      $��1��6   $	��Top=�6#   $��Stretch=�6;   $��0 -4 0 0q�6Y   8�� 6k   �� 5i      $��1��6   $��Stretch=�6"   $��Right=�6>   $��0 0 -4 0q�6Z   8�� 6l   �� 5j      $��1��6   $��Bottom=�6#   $��Stretch=�6>   $��0 0 0 -4q�6\   8�� 6n     5k      $��6��6   $��6��6&   $ SizeNWSE��61   $��	-6 -6 0 0q�6C   $	��Top=�5l      $
��Left=�65   8�� 6P     5m      $��6��6   $��6��6&   $ SizeNESW��61   $��	0 -6 -6 0q�6C   $	��Top=�5n      $��Right=�65   8�� 6Q     5o      $��6��6   $��6��6&   $ SizeNESW��61   $��	-6 0 0 -6q�6C   $��Bottom=�5p      $
��Left=�68   8�� 6S     5q      $��6��6   $��6��6&   $ SizeNWSE��61   $��	0 0 -6 -6q�6C   $��Bottom=�5r      $��Right=�68   8�� 6T   5s      5t      �� 5w      " 	 6   .�5x   
   ��  � �� 5y      �� 5z      " 	 6"   .R�5{      F� 
������  . 
������ ��6   �� 	 �� *FindAncestor+" 
 $
 DataContext.Type�8   6�   5|      5~      5      �� 5�      " ��6*   .R�5�   
   �� $��0 5 0 0q�6   $��Bottom=�69   $��Center=�6T   $��15��6q   
������ 
������ ��6#   �� *Name+6�   5�      �� 5�      " 
 6   .�5�   
   ��  � ��$ 20��6%   �� 5�   
    � ��$ 20��6&   �� 5�   
    �@J�SnapsToDevicePixels . 60   �� 5�   
    �@��Left 6(   �� *LeftOnCanvas+$ TwoWay=��� 5�   
    �@��Top 6'   �� *Top+$ TwoWay=��� 5�   
    �@
 CanvasWidth 6(   �� *WidthOnCanvas+$ OneWayToSource=��� 5�   
    �@
 
Connectors 6'   �� *ConnectorsVM+�� 5�   
   
 �@
 ID 6   �� *ID+�� 5�   
    � �� 5�      �� 5�      " 
 6"   .R�5�      ��  6!   �� 	 �� *FindAncestor+" 
 $
 DataContext�.+�5�      � 8   ��6V   �� *Width+$ TwoWay=���6{   �� *Height+$ TwoWay=�.+�5�      �� 
������ $��-4q�6;   F� 5�      $ PART_SelectionMarker��6#   $��-5q�6A   
������$	 0.3Q�5�   1   $y�Hidden=�6?   
��������5�      �� *Type+8  �� 5�      $ PART_ContentPresenter��6/   $��Stretch=�5�   +   $��Stretch=�5�   +   ��5�   +   �� ��5�   +   �� *Type+8  5�      � 5�       6#   �� 	 �� *TemplatedParent+.+�5�   "   	  $ PART_DragThumb��6.   �� 5�   "   $ PART_ResizeDecorator��6*   $y�	Collapsed=�5�   &   8�� 5�   &   �� 5�   "   $ PART_ConnectorDecorator��6*   8�� 6K   5�      5�      �� 5�      $ PART_Description��6"   8�� 5�   "   5�       5�      P� 5�        �@J�IsMouseOver! . . �5�      �� $ PART_ResizeDecorator��6%    � y�$ Visible=�6]   5�      �� 5�      $
 True��6&    63   �� 	 �� *Self+$
 
IsSelected�.a�5�      �� $ PART_SelectionMarker��6%    � y�$ Visible=�6]   5�      5�      5�      5�      5�      