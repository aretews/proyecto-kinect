using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Awesomium.Core;
using System.Xml;
using System.IO;
using Awesomium.Windows.Controls;
using System.Windows.Forms;


namespace motion
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            //inicializar el core de awesomium
            while (WebCore.IsInitialized == false) {
                WebCore.Initialize(WebConfig.Default, true);
            }

            

            InitializeComponent();

            //maximizar pantalla con una transparencia 
            MaximizeWindow();

            //crear contador
            timer.Tick += new EventHandler(TimerEventRunning);
            timer.Interval = timeToValidate;
            timerTexFeedBack.Tick += new EventHandler(TimerEventFadeOut);
            timerTexFeedBack.Interval = velocityFade;


            //instancias de clases
            cs = new CSpeech();
            conn = new Connection();
             ck = new CKinect();


        }//--------------------------------------------------------------------

        /* ==================================*\ 
         *      MÉTODOS DE INTERFAZ          *
         *                                   *
        \* ==================================*/                         

        public void MaximizeWindow() {
            //maximizar pantalla
            Topmost = true;
            this.Activate();
            this.WindowState = System.Windows.WindowState.Maximized;
           
        }//---------------------------------------------------------


        public Boolean loadXMLanguage() {
            try {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load("lenguaje.xml");
                XmlNodeList xLanguages = xDoc.GetElementsByTagName("languages");
                XmlNodeList xLanguage = ((XmlElement)xLanguages[0]).GetElementsByTagName("language");
                foreach (XmlElement language in xLanguage)
                {
                    if (language.GetAttribute("value") == defaultLanguage)
                    {
                       VOICE_GREETING  = language.GetElementsByTagName("greeting")[0].InnerText;
                       VOICE_NEXT = language.GetElementsByTagName("command-next")[0].InnerText;
                       VOICE_PREVIOUS = language.GetElementsByTagName("command-previous")[0].InnerText;
                       VOICE_LAST_LEFT = language.GetElementsByTagName("command-last-set-left")[0].InnerText;
                       VOICE_LAST_RIGHT = language.GetElementsByTagName("command-last-set-right")[0].InnerText;
                       VOICE_CLOSE = language.GetElementsByTagName("close")[0].InnerText;
                    }
                    
                }
                return true;
            }catch(FileNotFoundException e){
                return false;
            }

        }//---------------------------------------------------------------------

        private bool loadClock(bool run)
        {
            timer.Enabled = true;
            if (run == true)
            {
                timer.Start();
                return true;
            }
            else
            {
                timer.Stop();
                
                return false;
            }


        }//---------------------------------------------------------

        private void TimerEventRunning(object sender, EventArgs e)
        {
            //mostrar avance del contador
            if (firstBegin)
            {
                //actualizar nuevamnente desde la base de datos
                //el movimiento sera para el que tenga las angulos validados

                if (trackingPositionRight.Count > 10 && (trackingPositionRight.Count > trackingPositionLeft.Count)) //mover a la derecha
                {
                    updateBrowserWithSet("right");
                }
                else if (trackingPositionLeft.Count > 10 && (trackingPositionLeft.Count > trackingPositionRight.Count))
                { //mover a la izquierda
                    updateBrowserWithSet("left");
                }

                trackingPositionRight.Clear();
                trackingPositionLeft.Clear();
               
            }


        }//-----------------------------------------------------------------------
        private void TimerEventFadeOut(object sender, EventArgs e)
        {
            //desaparecer el texto en caso de que contenga algo
            if (lb_feedbackMov.Opacity <= 0)
            {//si esta totalmente desvanecido quita el texto

                lb_feedbackMov.Content = "";
                lb_feedbackMov.Opacity = 1;//devuelve la opacidad al 100%;
                timerTexFeedBack.Stop();

            }
            else
            {
                lb_feedbackMov.Opacity = lb_feedbackMov.Opacity - .1; // al valor actual le disminuye la opacidad               
            }

        }//-----------------------------------------------------------------

        private Boolean InitialConfig()
        {
            String[] dataCompany = conn.getDataCompany();//obtner los datos basico de la empresa
            try
            {
                numberScreens = int.Parse((dataCompany[2].Split(new char[] { '=', ';' }))[1]);
                numberScreens = 2; //***********************************************************************************

                if (InitializeInBlank)
                {
                    createForms(numberScreens);
                    return true;
                }
                else {
                    //obtener el primer set
                    List<String> list = conn.getCategories();
                    if (list != null && list.Count > 0)
                    {
                        //mostrar en la interfaz las categorías 
                        CInterface i = new CInterface();
                        labelsInterface = i.drawInPanel(panel_status_category, list);


                        string category = list[0];

                        sets = conn.getLinksCompany(category);
                        if (sets.Count > 0)
                        {
                            //abrir el primer set 
                            createForms(numberScreens);
                            return true;

                        }
                        else { //si no hay un solo set no puede mostrar las pantallas con los links y pondria los default
                            InitializeInBlank = true;
                            //abrir el primer set 
                            createForms(numberScreens);
                            return true;
                            
                        }
                        
                    }
                    else { 
                        //si no hay categorias no puede inicializar el programa
                        return false;
                    }
                }
               
              
               
            }
            catch (Exception e)
            {               
                return false;
            }


        }//---------------------------------------------------------

        private void createForms(int numberForms)
        {
            //crear el almacen de referncias de los navegadores
            browserReferences = new List<ControlFormsBrowser>();
            //obtener la lista del primer set

            //crear un numero finito instancias de form
            for (int i = 1; i <= numberForms; i++)
            {

                //crea el navegador y lo incluye dentro del form
                WebControl browser = new WebControl();

                browser.ViewType = WebViewType.Window;

                if (browser.IsLive)
                {
                   
                }
                else
                {
                    
                }

                Window w = new Window();
                Grid g = new Grid();
                w.Content = g;
                g.Children.Add(browser);

                //abrir el form en el monitor asignado
                openFormWindow(i, w);

                //guarda la referencia del form para controlarlo mas adelante
                ControlFormsBrowser cb = new ControlFormsBrowser(i); //guarda el numero del navegador
                cb.setForm(w);
                cb.setBrowser(browser);
                browserReferences.Add(cb);

            }

            //abrir los browsers
            openBrowsers();


        }//---------------------------------------------------------

        public void openFormWindow(int i, Window w)
        {
            //obtener pantallas

            Screen[] screens = Screen.AllScreens;

            System.Drawing.Rectangle bounds = screens[i - 1].Bounds;

            w.Top = bounds.Top;
            w.Left = bounds.Left;
            w.Width = bounds.Width;
            w.Height = bounds.Height;


        }//---------------------------------------------------------



        public void openBrowsers()
        {
            String[] links =null;
            int count = 0;

            if (WebCore.IsInitialized)
            {
                if (InitializeInBlank) // iniciar con la pagina default
                {
                    foreach (ControlFormsBrowser cb in browserReferences)
                    {
                        cb.getForm().Show();
                        if (cb.getBrowser().IsLive)
                        {
                            cb.UpdateURLBrowser(defaultPage);
                        }
                            
                    }
                }
                else { //cargar los link del primer set
                    links = setToLinkArray(0);
                    
                    foreach (ControlFormsBrowser cb in browserReferences)
                    {
                        cb.getForm().Show();
                        if (cb.getBrowser().IsLive)
                        {
                            cb.UpdateURLBrowser(links[count]);
                        }
                        count += 1;
                    }
                    firstBegin = true;
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("navegadores no inicializados intentar nuevamente!");
            }

        }//---------------------------------------------------------


        public String[] setToLinkArray(int number) {

            String[] linksReturn = { defaultPage, defaultPage, defaultPage, defaultPage, defaultPage, defaultPage, defaultPage, defaultPage };
            Set s = sets[number];
            int count = 0;
            foreach (Link l in s.getSet())
            {
                linksReturn[count] = l.getLink();
                count += 1;
            }
            return linksReturn;

        } //--------------------------------------------------------------


        public void updateBrowserWithSet(string direction)
        {
            int newValue = 0;
            if (direction.Equals("right"))
            {
                newValue = actualSet + 1;
                if (newValue >= 0 && newValue < sets.Count)
                {
                    setFeedBackText("Siguiente");
                    cs.generateVoice(VOICE_NEXT);
                    String[] links = setToLinkArray(newValue);
                    int count = 0;
                    foreach (ControlFormsBrowser cb in browserReferences)
                    {
                        cb.UpdateURLBrowser(links[count]);
                        count += 1;
                    }
                    actualSet = newValue;
                    //desaparece el texto 
                    timerTexFeedBack.Start();
                }
                else
                {
                    setFeedBackText("último set");
                    cs.generateVoice(VOICE_LAST_RIGHT);
                    //desaparece el texto 
                    timerTexFeedBack.Start();
                }

            }
            else
            {
                newValue = actualSet - 1;
                if (newValue >= 0 && newValue < sets.Count)
                {
                    setFeedBackText("Anterior");
                    cs.generateVoice("set anterior");
                    String[] links = setToLinkArray(newValue);
                    int count = 0;
                    foreach (ControlFormsBrowser cb in browserReferences)
                    {
                        cb.UpdateURLBrowser(links[count]);
                        count += 1;
                    }

                    actualSet = newValue;
                    //desaparece el texto 
                    timerTexFeedBack.Start();
                }
                else
                {
                    setFeedBackText("último set");
                    cs.generateVoice(VOICE_LAST_LEFT);
                    //desaparece el texto 
                    timerTexFeedBack.Start();
                }
            }


        }//---------------------------------------------------------

        public void setFeedBackText(String text)
        {
            lb_feedbackMov.Content = text;
        }//-------------------------------------------------

        public void closeForms()
        {
            if (browserReferences != null)
            {
                if (WebCore.IsInitialized)
                {
                    foreach (ControlFormsBrowser f in browserReferences)
                    {
                        //obtener cada form y cerrar la aplicacion
                        f.getForm().Close();
                    }
                }
            }

        }//---------------------------------------------------------

        public Boolean loadKinect() {

            ck.InitializeKinect(streamVideo, streamSkeleton, trackingPositionRight, trackingPositionLeft, loaderGesture,this);

            return true;
        }
        public Boolean loadSpeechRecognition() {

            return true;
        }//-----------------------------------------------------------------------

        public void removeLabelsPanel()
        {
            foreach (System.Windows.Controls.Label l in labelsInterface)
            {
                panel_status_category.Children.Remove(l);
            }
        }//---------------------------------------------------

        public void closeProcessAwesomium() {
        
        
        }


        //acciones para los botones de iniciar aplicación y de cerrar aplicación
        private void open_Click(object sender, RoutedEventArgs e)
        {
            if (open.Content.Equals("Activar"))
            {
                //cargar lenguaje
                if (loadXMLanguage())
                { //cargar el lenguaje
                    cs.generateVoice(VOICE_GREETING);
                    if (loadClock(true) == true && InitialConfig() == true && loadKinect()==true && loadSpeechRecognition()==true)
                    {
                        //cambiar el estatus del boton 
                        open.Content = "Desactivar";
                    }

                }
                else
                {
                    System.Windows.MessageBox.Show("No se encuentra el archivo lenguaje.xml");
                }
            }
            else {               
                open.Content = "Activar";
                //mensaje de despedida
                cs.generateVoice(VOICE_CLOSE);
                loadClock(false);
                closeForms();
                //resetear el valor del set actual
                actualSet = 0;
                //resetear el valor de inicio 
                firstBegin = false;
                //parar los timers
                timer.Stop();
                timerTexFeedBack.Stop();
            }
            

        }//---------------------------------------------------------

        private void close_Click(object sender, RoutedEventArgs e)
        {
          
            closeForms();
            //resetear el valor del set actual
            actualSet = 0;
            //resetear el valor de inicio 
            firstBegin = false;
            //parar los timers
            timer.Stop();
            timerTexFeedBack.Stop();
            this.Close();
        }//---------------------------------------------------------


        //variables
        CSpeech cs;
        CKinect ck;
        Connection conn;
        private string defaultLanguage = "esp";
        private string VOICE_GREETING = "";
        private string VOICE_CLOSE = "";
        private string VOICE_NEXT = "";
        private string VOICE_PREVIOUS = "";
        private string VOICE_LAST_LEFT ="";
        private string VOICE_LAST_RIGHT = "";
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer timerTexFeedBack = new System.Windows.Forms.Timer();
        private int timeToValidate = 1000;
        private int velocityFade = 100;
        private Boolean firstBegin = false;
        private Boolean InitializeInBlank = false;
        private int numberScreens = 1;
        private List<Set> sets = new List<Set>();
        private List<ControlFormsBrowser> browserReferences;
        private string defaultPage = "http://aretemotion.appspot.com/arete";
        private int actualSet = 0;
        private List<System.Windows.Controls.Label> labelsInterface = new List<System.Windows.Controls.Label>();

        //lista de angulos
        private List<CPoint> trackingPositionRight = new List<CPoint>();
        private List<CPoint> trackingPositionLeft  = new List<CPoint>();
     
    }
}
