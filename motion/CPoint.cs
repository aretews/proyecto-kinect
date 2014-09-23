using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motion
{
    class CPoint
    {
        //properties
        private double PosX = 0.0;
        private double PosY = 0.0;

        //constructors
        public CPoint() { }

        public CPoint(double positionX, double positionY) {
            PosX = positionX;
            PosY = positionY;
        }

        //getters 
        public double getPosX() {
            return PosX;
        }//--------------------------------------
        public double getPosY(){
            return PosY;
        }//--------------------------------------
        //setters

        public void setPosX(double positionX) {
            PosX = positionX;
        }//--------------------------------------

        public void setPosY(double positionY) {
            PosY = positionY;
        }//--------------------------------------

        


    }
}
