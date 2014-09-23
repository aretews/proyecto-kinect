using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace motion
{
    class Link
    {
        //metodos
        public int getOrderScreen()
        {
            return orderScreen;
        }//---------------------------------------------------------

        public void setOrderScreen(int order)
        {
            this.orderScreen = order;
        }//---------------------------------------------------------

        public void setLink(string link)
        {
            this.link = link;
        }//---------------------------------------------------------

        public string getLink()
        {
            return link;
        }//---------------------------------------------------------



        //variables
        private int orderScreen;
        private string link;
    }
}
