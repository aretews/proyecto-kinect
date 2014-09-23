using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace motion
{
    class Set
    {
        public Set()
        {

            set = new List<Link>();
        }
        public void setNumberOfSet(int numberSet)
        {
            this.numberSet = numberSet;
        }
        public int getNumberOfSet()
        {
            return numberSet;
        }

        public void setLink(Link link)
        {
            //cada que agrega un link hace un ordenamiento
            set.Add(link);
        }
        public List<Link> getSet()
        {
            return set;
        }


        //variables
        private int numberSet;

        List<Link> set;
    }
}
