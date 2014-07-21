using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace KinectFinalProyect
{
    class CoreBrowser
    {
        private Process processHandler;
        private int numberScreen;
        private int posX;
        private int posY;
        private int height ;
        private int width;
        private Boolean active;
        private int id;

        public CoreBrowser() {
            processHandler = new Process();
        }

        public Process getProcess() {
            return this.processHandler;
        }
        public void setProcess(Process process){
            this.processHandler = process;
        }

        public int getNumberScreen() {
            return this.numberScreen;
        }

        public void setNumberScreen(int numberScreen) {
            this.numberScreen = numberScreen;
        }

        public void setLinkBrowser(string link) {
            processHandler.StartInfo.FileName = link;
            
        }
        public void setArguments(string arguments) {
            processHandler.StartInfo.Arguments = arguments;
        }
        public void startBrowser() {
            processHandler.Start();
        }
        public int getPosX() {
            return this.posX;
        }
        public void setPox(int posX) {
            this.posX = posX;
        }
        public int getPosY() {
            return this.posY;
        }

        public void setPosY(int posY) {
            this.posY = posY;
        }

        public int getHeight() {
            return height;
        }

        public void setHeight(int height){

            this.height = height;
        }

        public int getWidth() {
            return width;
        }
        public int setWidth(int width) {
            return width;
        }

        public Boolean isActive() {
            return active;

        } 
        public void setActive(Boolean active){
            this.active = active;
        }
        public int getNumberScreens(){
            
            return 0;
        }
        public int getId() {
            return id;
        }
        public void setId(int id) {
            this.id = id;
        }

        public Boolean KillBrowser(int number,List<CoreBrowser> list) {
            int count = 1;
            // si se pone 0 como argumento eliminara todos los procesos creados
            
             foreach (CoreBrowser browser in list)
                {
                   
                   if (count == number)
                    {
                       list.Remove(browser);
                       browser.getProcess().Kill();
                       return true;
                    }
                    count += 1;
                }

              return false;
        
        }

        public Boolean KillAllProcess(List<CoreBrowser> list) {
            foreach (CoreBrowser browser in list) {
                browser.getProcess().Kill();

            }
            list.Clear();
            return true;

        
        }

    }
}
