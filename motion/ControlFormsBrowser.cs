using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Awesomium.Windows.Controls;

namespace motion
{
    class ControlFormsBrowser
    {
        //métodos
        public ControlFormsBrowser()
        {

        }//---------------------------------------------------------
        public ControlFormsBrowser(int numberScreen)
        {
            this.numberScreen = numberScreen;
        }//---------------------------------------------------------
        public int getNumScreen()
        {
            return this.numberScreen;
        }
        public void setBrowser(WebControl browser)
        {
            this.browser = browser;
        }//---------------------------------------------------------
        public WebControl getBrowser()
        {
            return browser;
        }//---------------------------------------------------------
        public void UpdateURLBrowser(string url)
        {
            try
            {
                browser.Source = new Uri(url);

                //si esta cargando la pagina y truena cachar el error
                if (browser.IsLoading)
                {

                    if (browser.IsLoaded)
                    {
                        Console.WriteLine("cargo el navegadore exitosamente");
                    }
                    else if (browser.IsCrashed)
                    {
                        Console.WriteLine("trono el navegador");
                    }

                }

            }
            catch (Exception ue)
            {
                Console.WriteLine("uri invalida");
                //mostrar una url que si sea valida momentaneamente indicando que la página no puede ser visualizada
                browser.Source = new Uri(defaultPage);


            }
        }//---------------------------------------------------------

        public void setForm(Window window)
        {
            this.window = window;
        }//---------------------------------------------------------
        public Window getForm()
        {
            return window;
        }//---------------------------------------------------------

        public void setDefaultLink()
        {

        }//---------------------------------------------------------



        //variables
        private int numberScreen = 0;
        private WebControl browser;
        private Window window;
        private string defaultPage = "http://aretemotion.appspot.com/arete";
    }
}
