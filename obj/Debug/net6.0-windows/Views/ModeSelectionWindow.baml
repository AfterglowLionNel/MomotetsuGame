   M S B A M L   `   `   `  ����  MomotetsuGame-    'MomotetsuGame.Views.ModeSelectionWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$!  桃太郎電鉄風ゲーム��5   	   $	��720��5   	   $
��1280��6    � WindowStartupLocation$ CenterScreen=�5	   	   ���	#FF87CEEB.��5      � ��5      �� 5       �� 
StartPoint$	 0,0&�6    �� EndPoint$	 0,1&�6*   .��5      �  � Color$ 	#FF87CEEB��6    � Offset$ 0Q�6%   � 5      $ 	#FF4682B4��6   $ 1Q�6%   5      .+�5      �� $!��桃太郎電鉄風ゲーム��6   $��60(�5      $
��Bold"�5      
�������$��Center=�5      $	��Top=�5      $��0,50,0,0q�5       J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         ShadowDepth$ 4Q�6      Opacity$	 0.8Q�6'   5       �� 5#      $��群馬県・草津温泉��6   $��24(�5$      $
��Bold"�5%      
�������$��Right=�5'      $	��Top=�5(      $��	0,20,20,0q�5)       5*        5+      $ 2Q�6   $	 0.8Q�6'   5-      �� 50      $��Center=�6   $
��Left=�51      $��	100,0,0,0q�52      .+�55      �� $	��0,5q�57      ��55      �� *ContinueGameCommand+{�56      �� *HasSaveData+��58      �� 59      	 �� 
TargetType"	 ��6   .R�5:      � $	��400��6   $��70��6   .+�5;   	   �� ���	#FFFFB6C1
 �� RadiusX$
 35��5<   (    �� RadiusY$ 35��65   ���	#FFE91E63$��3��5>   (    J� Opacity 5?   (   �� *	IsEnabled+ �� RelativeSource �� *TemplatedParent+   BoolToOpacityConverter �� 	Converter# [  �� 5@   	    �� Orientation$ 
Horizontal=�6   $��Center=�5A   )   $��Center=�5B   )   .+�5C   
   �� $��💾 ��6   $��28(�6   �� 5D   
   $��つづきから��6   $��28(�5E   ,   $
��Bold"�5F   ,   ���	#FF8B00005G   ,   5H   
   5I   	    �� Triggers 5J      P� 5K   	    P� Property �@J�IsMouseOver P� Value$
 True��6(   . �5L   
   ��  �� Property �@�Cursor �� Value$
 Hand��6#   �� 5M   
    � z� 5N      �� 5O       �� ScaleX$	 1.1Q�6    �� ScaleY$	 1.1Q�6(    �� CenterX$	 200Q�65    �� CenterY$ 35Q�6C   5Q      5R   
   5T      5V      �� 5Y      $	��0,5q�5Z      ��5Y      �� *SinglePlayCommand+��5[      �� 5\      "	 ��6   .R�5]      � $	��400��6   $��70��6   .+�5^   	   �� ���	#FF87CEEB$
 35��5_   (   $ 35��65   ���	#FF4169E1$��3��5a   (   �� 5b   	   $ 
Horizontal=�6   $��Center=�5c   )   $��Center=�5d   )   .+�5e   
   �� $��🚂 ��6   $��28(�6   �� 5f   
   $��ひとりで桃鉄��6   $��28(�5g   ,   $
��Bold"�5h   ,   ���	#FF0000805i   ,   5j   
   5k   	    5l      P� 5m   	    �@J�IsMouseOver$
 True��6(   . �5n   
   ��  �@�Cursor$
 Hand��6#   �� 5o   
    � z� 5p      �� 5q      $	 1.1Q�6   $	 1.1Q�6(   $	 200Q�65   $ 35Q�6C   5s      5t   
   5v      5x      �� 5{      ${�False��5|      $	��0,5q�5}      ��5{      �� *MultiPlayCommand+��5~      �� 5      "	 ��6   .R�5�      � $	��400��6   $��70��6   .+�5�   	   �� ���	#FFFFA500$
 35��5�   (   $ 35��65   ���	#FFFF8C00$��3��5�   (   $	 0.5Q�5�   (   �� 5�   	   $ 
Horizontal=�6   $��Center=�5�   )   $��Center=�5�   )   .+�5�   
   �� $��👥 ��6   $��28(�6   $	 0.5Q�6-   �� 5�   
   $��みんなで桃鉄��6   $��28(�5�   ,   $
��Bold"�5�   ,   ���	#FF8B4513$	 0.5Q�5�   ,   5�   
   5�   	   5�      5�      �� 5�      ${�False��5�      $	��0,5q�5�      ��5�      �� *LocalPlayCommand+��5�      �� 5�      "	 ��6   .R�5�      � $	��400��6   $��70��6   .+�5�   	   �� ���	#FF98FB98$
 35��5�   (   $ 35��65   ���	#FF228B22$��3��5�   (   $	 0.5Q�5�   (   �� 5�   	   $ 
Horizontal=�6   $��Center=�5�   )   $��Center=�5�   )   .+�5�   
   �� $��📡 ��6   $��28(�6   $	 0.5Q�6-   �� 5�   
   $��ちかくで桃鉄��6   $��28(�5�   ,   $
��Bold"�5�   ,   ���	#FF006400$	 0.5Q�5�   ,   5�   
   5�   	   5�      5�      �� 5�      ${�False��5�      $	��0,5q�5�      ��5�      �� *OnlinePlayCommand+��5�      �� 5�      "	 ��6   .R�5�      � $	��400��6   $��70��6   .+�5�   	   �� ���	#FFEE82EE$
 35��5�   (   $ 35��65   ���	#FF8B008B$��3��5�   (   $	 0.5Q�5�   (   �� 5�   	   $ 
Horizontal=�6   $��Center=�5�   )   $��Center=�5�   )   .+�5�   
   �� $��🌐 ��6   $��28(�6   $	 0.5Q�6-   �� 5�   
   $��ネットで桃鉄��6   $��28(�5�   ,   $
��Bold"�5�   ,   ���	#FF4B0082$	 0.5Q�5�   ,   5�   
   5�   	   5�      5�      �� 5�      $	��0,5q�5�      ��5�      �� *ShowExtrasCommand+��5�      �� 5�      "	 ��6   .R�5�      � $	��400��6   $��70��6   .+�5�   	   �� ���	#FFFFFFE0$
 35��5�   (   $ 35��65   ���	#FFFFD700$��3��5�   (   �� 5�   	   $ 
Horizontal=�6   $��Center=�5�   )   $��Center=�5�   )   .+�5�   
   �� $��🎁 ��6   $��28(�6   �� 5�   
   $��	おまけ��6   $��28(�5�   ,   $
��Bold"�5�   ,   ���	#FFFF8C005�   ,   5�   
   5�   	    5�      P� 5�   	    �@J�IsMouseOver$
 True��6(   . �5�   
   ��  �@�Cursor$
 Hand��6#   �� 5�   
    � z� 5�      �� 5�      $	 1.1Q�6   $	 1.1Q�6(   $	 200Q�65   $ 35Q�6C   5�      5�   
   5�      5�      �� 5�      $	��0,5q�5�      ��5�      �� *ShowTutorialCommand+��5�      �� 5�      "	 ��6   .R�5�      � $	��400��6   $��70��6   .+�5�   	   �� ���	#FFE0FFFF$
 35��5�   (   $ 35��65   ���	#FF00CED1$��3��5�   (   �� 5�   	   $ 
Horizontal=�6   $��Center=�5�   )   $��Center=�5�   )   .+�5�   
   �� $
��❓ ��6   $��28(�6   �� 5�   
   $��遊びかた��6   $��28(�5�   ,   $
��Bold"�5�   ,   ���	#FF008B8B5�   ,   5�   
   5   	    5     P� 5  	    �@J�IsMouseOver$
 True��6(   . �5  
   ��  �@�Cursor$
 Hand��6#   �� 5  
    � z� 5     �� 5     $	 1.1Q�6   $	 1.1Q�6(   $	 200Q�65   $ 35Q�6C   5     5	  
   5     5     5     5      � 	Resources 5       BoolToVisibilityConverter �� 
TargetType �� Triggers��%�   &	       &	       �� 5     6!   �� 5     " ��6)    5     P� 5      � {�$ False��6#   . �5     ��  �@J�Opacity$	 0.5Q�6!   5     5     5     5     