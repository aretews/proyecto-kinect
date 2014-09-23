using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace motion
{
    class CInterface
    {

        public List<Label> drawInPanel(Grid p, List<string> catergorys)
        {

            int MaxWith = 912;
            int HeighRow = 55;
            int margin = 10;
            int width_label = 0;

            int posX = 6;
            int posY = 53;

            //agregar los elementos a la interfaz
            List<Label> labels = new List<Label>();

            foreach (string s in catergorys)
            {
                ElementCategory element = new ElementCategory();
                //calcular el ancho basado en el numero de letras de la palabra
                width_label = s.Length * 20;

                //intenta calcular si la palabra cabe en el mismo renglon
                int ds = MaxWith - (posX + width_label + margin);

                if (ds <= 0)
                {
                    posY = posY + HeighRow;
                    posX = 6;
                }
                //asignar valores de ancho y posicion
                element.setNameLabel(s, posX, posY, width_label);
                //agregar la etiqueta
                p.Children.Add(element.getLabel());
                labels.Add(element.getLabel());//guardar las etiquetas para poder manipularlas mas adelante

                posX = posX + width_label + margin;

            }
            

            return labels;


        }
    }
}
