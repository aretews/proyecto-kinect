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
using Microsoft.Kinect;
using System.Timers;
using Microsoft.Speech.Recognition;
using System.Threading;
using Microsoft.Speech.AudioFormat;

namespace KinectFinalProyect
{
        //branch version1.0 oscar
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml merge con master
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private KinectSensor sensor;
        private CoreKinect coreKinect = new CoreKinect();
        private CoreVoice coreVoice = new CoreVoice();
       
        private RecognizerInfo recognizer;
        private SpeechRecognitionEngine speechRecognitionEngine;

        int left = 50;
        int top = 50;
        int right = 50;
        int bottom = 50;
        int smoothness = 50;
        int fps = 30;
        int []defaultPosition = {50,50,50,50};
        string direction = "left"; //direccion default
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            //crear componentes
            InitializeComponent();

            //inicializar tamaños de textos
            RedefineFonts();

            //mostrar navegador
            RunBrowser("http://www.chartjs.org/");
            //activar el kinect

        }
        //obtener el sensor del kinect
        public KinectSensor getSensor() {
            return sensor;
        }
        public int getNumerOfKinectsConnected() {
            return KinectSensor.KinectSensors.Count;
        }

        public void RedefineFonts(){
            /*
             BrushConverter bc = new BrushConverter();  
             stackPanelFlasher.Background=  (Brush)bc.ConvertFrom("#C7DFFC"); 
             * */
            //pantalla principal
            main_window.Background = Brushes.Beige;

            //pantalla 5
            label_screen5.FontSize = 25;
            label_screen5.Foreground = Brushes.White;
            label_screen5.Background = Brushes.Black;

            //pantalla 4
            label_screen4.FontSize = 25;
            label_screen4.Foreground = Brushes.White;
            label_screen4.Background = Brushes.Black;

            //pantalla 3
            label_screen3.FontSize = 25;
            label_screen3.Foreground = Brushes.White;
            label_screen3.Background = Brushes.Black;

            //pantalla 2
            label_screen2.FontSize = 25;
            label_screen2.Foreground = Brushes.White;
            label_screen2.Background = Brushes.Black;

            

        }
       
        //mostrar contenido en el sitio
        public void RunBrowser(string site)
        {
            //navegador.Navigate(site);
            Uri uri = new Uri(@site, UriKind.RelativeOrAbsolute);
            navegador.Source = uri;
            label_status.Content = "Navegador activo";
           
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
           label_timer.Content = "ejecutando";
           
           navegador.Margin = new Thickness(left, top, right, bottom);

            //revisar posicion para mover
           if (direction == "right") {
               left = left + smoothness;
               right = right + smoothness;
           }
           else if (direction == "left") {
               left = left - smoothness;
               right = right - smoothness;
           }
           else if (direction == "top")
           {
               top = top - smoothness;
               bottom = bottom - smoothness;
           }
           else if (direction == "bottom")
           {
               top = top + smoothness;
               bottom = bottom - smoothness;
           }
           
         }


        //aqui se activa el sistema 
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (btn_activar.Content+""== "Activar")
            {
                //verificar cuantos kinects estan conectados
                if (coreKinect.getNumerOfKinectsConnected() > 0 )
                { 
                    
                    //asignar solo el primer kinect
                    sensor = coreKinect.getKinectNumber(0);
                    
                    //activar sensor
                    sensor.Start();
                    btn_activar.Content = "Desactivar";
                    //cambiar estatus en interfaz
                    status.Source = coreKinect.getBitMapSrc(@"C:\openCV_proyect_face_recognition\KinectFinalProyect_respaldo\KinectFinalProyect\img\status.png");

                    //habilitar el stream de color
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);

                    //tomar una foto
                    imageCanvas.Background = new ImageBrush(coreKinect.getAPicture(sensor));

                    //detectar voz
                    detect_voice();
                    
                }
                

            }
            else {
                if (sensor != null && sensor.IsRunning) {

                    sensor.ColorStream.Disable();
                    sensor.Stop();

                    //cambiar estatus en interfaz
                    status.Source = coreKinect.getBitMapSrc(@"C:\openCV_proyect_face_recognition\KinectFinalProyect_respaldo\KinectFinalProyect\img\status_off.png");

                    //cambiar estado del boton
                    btn_activar.Content = "Activar";
                }
            }
        }
        private void resetValueWebBrower() {
            left = defaultPosition[0];
            top = defaultPosition[1];
            right = defaultPosition[2];
            bottom = defaultPosition[3];
            navegador.Margin = new Thickness(left, top, right, bottom);
        }

        private void checkStartofDispatcher() {

            if (!dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / fps);
                dispatcherTimer.IsEnabled = true;
                dispatcherTimer.Start();
            }
        }

        private void move_right_Click(object sender, RoutedEventArgs e)
        {
            //restaurar valor
            resetValueWebBrower();
            //llevar el control del tiempo transcurrido de ejecución del programa
            checkStartofDispatcher();
            direction = "right";
        }

        private void move_left_Click(object sender, RoutedEventArgs e)
        {
            //restaurar valor
            resetValueWebBrower();
            //llevar el control del tiempo transcurrido de ejecución del programa
            checkStartofDispatcher();
            direction = "left";
        }


        private void move_top_Click(object sender, RoutedEventArgs e)
        {
            //restaurar valor
            resetValueWebBrower();
            //llevar el control del tiempo transcurrido de ejecución del programa
            checkStartofDispatcher();
            direction = "top";
        }

        private void move_bottom_Click(object sender, RoutedEventArgs e)
        {
            //restaurar valor
            resetValueWebBrower();
            //llevar el control del tiempo transcurrido de ejecución del programa
            checkStartofDispatcher();
            direction = "bottom";
        }

       //deteccion de voces
        private void detect_voice() { 
            //habilitar la detección de sonidos SDK
            //audiostream <<-- no necesario

            //llamada al core de detección de voz

            //1) pasarle en sensor al detectos
            coreVoice.setSensor(sensor);
            //inicializar el reconocedor de voz
            coreVoice.InitializeRecognizer();
            //pedir un sensor activo por 10 segundos
            recognizer = coreVoice.getRecognizer();
            //definir palabra clave para captar la atención del kinect
            coreVoice.DefineKeyWord("testing");

            //definiar gramática
            coreVoice.DefineChoices();

            speechRecognitionEngine = new SpeechRecognitionEngine(recognizer.Id);

            //grammar
            var grammerBuilder = new GrammarBuilder { Culture = recognizer.Culture };
            grammerBuilder.Append(coreVoice.getChoices());
            var grammar = new Grammar(grammerBuilder);

            //carga de la gramática
            speechRecognitionEngine.LoadGrammar(grammar);
            speechRecognitionEngine.SpeechRecognized += SpeechRecognitionEngineOnSpeechRecognized;

            //crear un hilo para mantener la detección de audio separada del sensor
            var thread = new Thread(StartAudioStream);


        }
        private void StartAudioStream() {
            //sample 16 bits
            // canal 1 de audio
            //bits por segundo 32000

            speechRecognitionEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm,16000,16,1,32000,2,null));
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void SpeechRecognitionEngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs) {

            Console.WriteLine("matched:  {0}", speechRecognizedEventArgs.Result.Text);

            MessageBox.Show(speechRecognizedEventArgs.Result.Text);
        }


        

       
    }
}
