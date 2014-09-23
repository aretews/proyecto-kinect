using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace motion
{
    class ElementCategory
    {
        public ElementCategory()
        {
            lab_cat = new Label();




        } //-------------------------------------------------------


        public void setNameLabel(string s, int posX, int posY, int width_label)
        {

            //lab_cat.FontFamily = SystemFonts.
            lab_cat.FontStyle = FontStyles.Italic;
            lab_cat.FontWeight = FontWeights.Bold;
            lab_cat.Foreground = new SolidColorBrush(Colors.White);
            lab_cat.FontSize = 30;
            lab_cat.Background = new SolidColorBrush(Colors.CadetBlue);
            lab_cat.HorizontalAlignment = HorizontalAlignment.Left;
            lab_cat.VerticalAlignment = VerticalAlignment.Top;

            lab_cat.Margin = new Thickness(posX, posY, 0, 0);


            Random random = new Random();
            int randomNumber = random.Next(0, 100);
            lab_cat.Name = "label" + randomNumber;
            lab_cat.TabIndex = 0;
            lab_cat.Content = s;
            lab_cat.Width = width_label;




        }//-------------------------------------------------------




        public Label getLabel()
        {
            return lab_cat;
        }



        //variables
        Label lab_cat;
    }
}
