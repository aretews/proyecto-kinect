using System;
using System.Collections.Generic;

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
using Microsoft.Speech.Recognition.SrgsGrammar;
using Microsoft.Speech.Synthesis;
using Coding4Fun.Kinect.Wpf;
using System.Diagnostics;
using System.Runtime.InteropServices;



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

        //controlar lista de procesos

        List<Process> list = new List<Process>();

        int left = 50;
        int top = 50;
        int right = 50;
        int bottom = 50;
        int smoothness = 50;
        int fps = 30;
        int []defaultPosition = {50,50,50,50};
        string direction = "left"; //direccion default
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        Boolean userLogged = false;
        Boolean hearing = false;

        




        public MainWindow()
        {

            this.Topmost = true;

            //crear componentes
            InitializeComponent();

            //inicializar tamaños de textos
            RedefineFonts();

            //mostrar navegador default
            //redirectBrowser("https://www.youtube.com/watch?v=3-EMH59VrRw");
            redirectBrowser("http://www.googl.com");

            //lanzar aplicacion externa
            generateBrowser(4);

            //activar el kinect

            //speech voice
            //SpeechSynthesizer s = new SpeechSynthesizer();
            //s.Speak("Oscar Diaz");

        }
        //obtener el sensor del kinect
        public KinectSensor getSensor() {
            return sensor;
        }

        public void generateBrowser(int number) {

            

            for (int i = 0; i < number; i++) {

                Process chrome = new Process();

                list.Add(chrome);
                chrome.StartInfo.FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
                chrome.StartInfo.Arguments = "--new-window http://www.google.com.mx";
                chrome.Start();

                
            }

            if (killProcessNumber(3)) {
                MessageBox.Show("aplicación cerrada");
            }
        }

        

        public Boolean killProcessNumber(int number) {
            int count = 1;
            foreach (Process chrome in list){
                if (count == number) {
                    list.Remove(chrome);
                    chrome.Kill();
                    return true;
                }
                count +=1;
            }
            return false;
        }


        public int getNumerOfKinectsConnected()
        {
            return KinectSensor.KinectSensors.Count;
        }

        public void RedefineFonts(){
            /*
             BrushConverter bc = new BrushConverter();  
             stackPanelFlasher.Background=  (Brush)bc.ConvertFrom("#C7DFFC"); 
             * */
            //pantalla principal
            main_window.Background = Brushes.Beige;

            //letrero de bienvenida
            label_welcome.Visibility = Visibility.Hidden;
            label_welcome.FontSize = 25;
            label_welcome.Foreground = Brushes.White;
            label_welcome.Background = Brushes.CadetBlue;

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
                    //sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                    sensor.ColorStream.Enable();

                    //tomar una foto
                    //imageCanvas.Background = new ImageBrush(coreKinect.getAPicture(sensor));

                    //empezar el streaming de video
                    //sensor.ColorFrameReady += SensorOnColorFrameReady;

                    //profundidad
                    sensor.DepthStream.Range = DepthRange.Default;
                    sensor.DepthStream.Enable();
                    sensor.DepthFrameReady += SensorOnDepthFrameReady;


                    //tracking mano y saber si hay una persona frente al kinect
                    sensor.SkeletonStream.Enable();
                    //realizar el track sentado
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    //sensor.AllFramesReady += sensor_AllFramesReady;


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

        private void SensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs depthImageFrameReadyEventArgs)
        { 
            //deteccion de stream y paso de frame por canvas
            using (var frame = depthImageFrameReadyEventArgs.OpenDepthImageFrame())
            {
                if (frame != null) {
                    var depthImagePixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
                    frame.CopyDepthImagePixelDataTo(depthImagePixels);

                    var colorPixels = new byte[4 * sensor.DepthStream.FramePixelDataLength];
                    for (int i = 0; i < colorPixels.Length; i += 4)
                    {
                        if (depthImagePixels[i / 4].PlayerIndex != 0)
                        {
                            colorPixels[i +1] = 255;
                        }
                    }

                    videoCanvas.Background = new ImageBrush(colorPixels.ToBitmapSource(640, 480));
                    // videoCanvas.Background = new ImageBrush(frame.ToBitmapSource());
                
                }
                
               
            }
        
        }
        private void SensorOnColorFrameReady(object sender, ColorImageFrameReadyEventArgs colorImageFrameReady)
        {
            //deteccion de stream y paso de frame por canvas
            using (var frame = colorImageFrameReady.OpenColorImageFrame())
            {
                //crear un array del tamaño del frame
                var bitmap = coreKinect.CreateBitmap(frame);
                videoCanvas.Background = new ImageBrush(bitmap);

            }

        }



        private void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e) {

            var  depthImagePixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;
                frame.CopyDepthImagePixelDataTo(depthImagePixels);
            }

            using (var frame = e.OpenColorImageFrame())
            {

                if (frame == null)
                    return;

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
            //coreVoice.DefineKeyWord("kinect");

            //definiar gramática
           // coreVoice.DefineChoices();

            speechRecognitionEngine = new SpeechRecognitionEngine(recognizer.Id);

            //grammar
            var grammerBuilder = new GrammarBuilder { Culture = recognizer.Culture };
            //grammerBuilder.Append(coreVoice.getChoices());
            //var grammar = new Grammar(grammerBuilder);
            SrgsDocument grammarNames = new SrgsDocument(@"C:\Users\design\Documents\GitHub\proyecto-kinect\KinectFinalProyect\dictionary.xml");
            var grammar = new Grammar(grammarNames);

            //carga de la gramática
            speechRecognitionEngine.LoadGrammar(grammar);
            speechRecognitionEngine.SpeechRecognized += SpeechRecognitionEngineOnSpeechRecognized;

            //crear un hilo para mantener la detección de audio separada del sensor
            var thread = new Thread(StartAudioStream);
            thread.Start();
           // MessageBox.Show("corriendo hilo");

        }
        private void StartAudioStream() {
            //sample 16 bits
            // canal 1 de audio
            //bits por segundo 32000

            speechRecognitionEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm,16000,16,1,32000,2,null));
            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }
        private void showWelcomeMessage(string message){
            label_welcome.Visibility = Visibility.Visible;
            label_welcome.Content = "Bienvenido: " + message;
        }

        private void redirectBrowser(string url) {
            Uri uri = new Uri(@url, UriKind.RelativeOrAbsolute);
            navegador.Source = uri;
        }

        private void SpeechRecognitionEngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            //Console.WriteLine("matched:  {0}", speechRecognizedEventArgs.Result.Text);
            if (speechRecognizedEventArgs.Result.Confidence < .6)
                return;

            //string phraseDectected = speechRecognizedEventArgs.Result.Text;
            string phraseDectected = speechRecognizedEventArgs.Result.Text;
            //para poder entender los comandos kinect debe recibir la orden "arete"
            MessageBox.Show(phraseDectected);
            if (phraseDectected == "kinect")
            {
                MessageBox.Show("esuchando");
                hearing = true;
                return;
            }

            //comandos de area
            if ((phraseDectected == "Finanzas") && (hearing==true))
            {
                redirectBrowser("http://www.chartjs.org/");
                return;
            }
            
            //comandos para personal
            if ((phraseDectected == "Oscar Diaz") && (hearing == true))
            {
                //si se logea mediante voz se debe de habilitar el logeo 
                userLogged = true;
                showWelcomeMessage(phraseDectected);
                redirectBrowser("http://www.chartjs.org/");
                return;
            }
             //comando de salida del sistema
            if ((phraseDectected == "salir") && (hearing == true))
            {
                redirectBrowser("http://www.google.com");
                 hearing = false;
                 return;
            }
        }

       
    }
}
