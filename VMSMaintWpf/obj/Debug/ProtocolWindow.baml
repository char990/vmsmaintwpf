   M S B A M L   `   `   `  ����F  BVMSMaintWpf, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:VMSMaintWpfVMSMaintWpf  5   	        VMSMaintWpf.ProtocolWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:VMSMaintWpf     5   	   -   5          � Title$  Protocol Stream Data��5   	   $	��480��6&   $	��640��63   .��5
      �  � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��24 �6   .+�5   
   �� -   5         ��Name$ 
tbProtocol��5      $��0��6   " �� VerticalScrollBarVisibility$ Visible=�63    �� AcceptsReturn .  �� TextWrapping$
 Wrap=�6n   �� 5   
   $��1��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�5      �� -   5         ��Name$ 
chkPtToEnd��5      $��Center=�6+   .��6G   Scroll to end6V   �� 5      -   5         ��Name$ btnPtClearData��5      $��Center=�6-   $��50,0,0,0q�6H   .��6w   
Clear Data6�   5      5      5      