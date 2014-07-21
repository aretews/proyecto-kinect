using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
using System.Windows.Forms;
//tracking de gestos
using Kinect.Toolbox;


using System.Runtime.InteropServices;
using System.Diagnostics;

using System.Drawing;

using System.Linq;

//skeleton
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Globalization;


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
        private CoreBrowser corebrowser = new CoreBrowser();
       
        private RecognizerInfo recognizer;
        private SpeechRecognitionEngine speechRecognitionEngine;
        Boolean userLogged = false;
        //control de pantallas
        private const int SW_HIDE = 0;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_MAXIMIZED = 2;
        private IntPtr hWnd;
        private int countTime;
        private DepthImagePixel[] depthImagePixels;

        //cursor mano

        private Boolean handSet = false;


        //skeleton
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly System.Windows.Media.Brush centerPointBrush = System.Windows.Media.Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly System.Windows.Media.Brush trackedJointBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly System.Windows.Media.Brush inferredJointBrush = System.Windows.Media.Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly System.Windows.Media.Pen trackedBonePen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.BlueViolet, 15);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly System.Windows.Media.Pen inferredBonePen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Gray, 1);

        //cursor
        private readonly System.Windows.Media.Brush handColorCursor = System.Windows.Media.Brushes.LightSeaGreen;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;



       
        //controlar lista de procesos

        List<CoreBrowser> list = new List<CoreBrowser>();

        List<Shape> trackGesture = new List<Shape>();

        string[] linksBrowser = { "http://bl.ocks.org/mbostock/4063269", "http://bl.ocks.org/mbostock/4062085", "http://bl.ocks.org/mbostock/4063423", "http://mbostock.github.io/d3/talk/20111116/bundle.html", "http://mbostock.github.io/d3/talk/20111018/collision.html", "http://bl.ocks.org/mbostock/4060606", "http://bost.ocks.org/mike/nations/", "http://bl.ocks.org/kerryrodden/7090426" };

     
        int []defaultPosition = {50,50,50,50};
        private System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        
        Boolean hearing = false;


        //[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        //private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public MainWindow()
        {

            this.Topmost = true;
            this.Activate();


            //crear componentes
            InitializeComponent();

            //inicializar tamaños de textos
            RedefineFonts();

            //lanzar el control de tiempo
            if (!dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0,1);
                
                dispatcherTimer.IsEnabled = true;
                dispatcherTimer.Start();
            }
            
            //lanzar aplicacion externa
            generateBrowser(1);
                        
            //generateApplication("");

            //detectScreens();

            //speech voice
            //SpeechSynthesizer s = new SpeechSynthesizer();
            //s.Speak("Oscar Diaz");

        }
       

        public void generateApplication(string path){
            Process process = new Process();

            process.StartInfo.FileName = (@"C:\Windows\System32\notepad.exe");
            process.Start();

            int display = 0;

            process.WaitForInputIdle();
            IntPtr p = process.MainWindowHandle;

            Console.WriteLine("puntero es: " + p + " nombre aplicacion: " + process.MainWindowTitle + " " +  process.MainWindowHandle);

            System.Drawing.Point screenlocation = Screen.AllScreens[display].Bounds.Location;
            
            SetWindowPos(process.MainWindowHandle, -1, screenlocation.X, screenlocation.Y, Screen.AllScreens[display].Bounds.Width, Screen.AllScreens[display].Bounds.Height, 1);
            process.Refresh();


        }

        public void generateBrowser(int number) {

            


            for (int i = 1; i <= number; i++) {
                CoreBrowser browser = new CoreBrowser();
                list.Add(browser);
                browser.setNumberScreen(i);
                browser.setLinkBrowser(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe");
                //browser.setLinkBrowser(@"C:\Program Files\Internet Explorer\iexplore.exe");
                //browser.setLinkBrowser(@"C:\Windows\System32\notepad.exe");
                browser.setArguments("--new-window --enable-logging --app=" + linksBrowser[i - 1]);
                browser.startBrowser();
                //device 
                hWnd = browser.getProcess().MainWindowHandle;
            }//fin de for crear los procesos
           
             
        }



        public void redirectProcessToWindows() {

            int display = 1;
            foreach (CoreBrowser browser in list)
            {
                Process process = browser.getProcess();


                browser.getProcess().WaitForInputIdle();
                IntPtr p = process.MainWindowHandle;

                Console.WriteLine("puntero es: " + p + " nombre aplicacion: " + process.MainWindowTitle + " " + process.MainWindowHandle);

                System.Drawing.Point screenlocation = Screen.AllScreens[display].Bounds.Location;

                SetWindowPos(process.MainWindowHandle, -1, screenlocation.X, screenlocation.Y, Screen.AllScreens[display].Bounds.Width, Screen.AllScreens[display].Bounds.Height, 1);
                process.Refresh();

            }
        
        }

        public void getAllProcess() {

            /*OBTENER TODOS LOS PROCESOS Y MOSTRARLOS */
            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                //Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                if (theprocess.ProcessName == "chrome")
                {
                    Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                }
            }
            
        }


        public void detectScreens() {
            string screenData = "";
            int count = 0;
            foreach (var screen in Screen.AllScreens) {

                screenData = screen.DeviceName;
                
                //System.Windows.Forms.MessageBox.Show(screen.DeviceName);
               // System.Windows.Forms.MessageBox.Show(screen.Bounds.ToString());
                //System.Windows.Forms.MessageBox.Show(screen.WorkingArea.ToString());
                //System.Windows.Forms.MessageBox.Show(screen.Primary.ToString());
                Console.WriteLine(screenData + " " + Screen.AllScreens[count].Bounds.Width);
                count += 1;
            }

            
           


        }

        public Boolean killProcessNumber(int number) {
            int count = 1;
            foreach (CoreBrowser browser in list){
                if (count == number) {
                    list.Remove(browser);
                    browser.getProcess().Kill();
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
            //main_window.Background = Brushes.Beige;

            //tamaño del contador
            time_loaded.FontSize = 25;
        }
       

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            time_loaded.Content = ""+countTime;

            //borrar circulos mostados
            if (trackGesture.Count>0) {
                foreach (Shape shape in trackGesture) {
                    videoCanvas.Children.Remove(shape);
                }
            }

            countTime += 1;
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
                    //al activar mandar el sensor a la clase para que pueda manipularlo posteriormente
                    coreKinect.setSensor(sensor);

                    btn_activar.Content = "Desactivar";
                    //cambiar estatus en interfaz
                    status.Source = coreKinect.getBitMapSrc(@"\img\status.png");

                    //habilitar el stream de color
                    //sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
                    sensor.ColorStream.Enable();

                    //tomar una foto
                    //videoCanvas.Background = new ImageBrush(coreKinect.getAPicture(sensor));

                    //empezar el streaming de video
                    //sensor.ColorFrameReady += SensorOnColorFrameReady;

                    //profundidad
                    sensor.DepthStream.Range = DepthRange.Default;
                    sensor.DepthStream.Enable();
                    //sensor.DepthFrameReady += SensorOnDepthFrameReady;


                    //tracking mano y saber si hay una persona frente al kinect
                    sensor.SkeletonStream.Enable();
                   

                   
                    
                    //realizar el track sentado
                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    sensor.SkeletonStream.EnableTrackingInNearRange = true;

                    LoadSkeleton();

                    //habilitar para tener todos los tipos en un solo manejador
                    sensor.AllFramesReady += sensor_AllFramesReady;

                    


                    //detectar voz
                    detect_voice();
                    
                }
                

            }
            else {
                if (sensor != null && sensor.IsRunning) {

                    sensor.ColorStream.Disable();
                    sensor.Stop();

                    //cambiar estatus en interfaz
                    status.Source = coreKinect.getBitMapSrc(@"\img\status_off.png");

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
                    //videoCanvas.Background = new ImageBrush(frame.ToBitmapSource());
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

            depthImagePixels = new DepthImagePixel[sensor.DepthStream.FramePixelDataLength];
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
                var bitmap = coreKinect.CreateBitmap(frame,depthImagePixels);
                videoCanvas.Background = new ImageBrush(bitmap);

            }
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                    return;
                var skeletons = new Skeleton[frame.SkeletonArrayLength];
                frame.CopySkeletonDataTo(skeletons);
                var skeleton = skeletons.FirstOrDefault(s =>s.TrackingState == SkeletonTrackingState.Tracked);
                if (skeleton == null)
                    return;
                var position = skeleton.Joints[JointType.HandRight].Position;
                
                var mapper = new CoordinateMapper(sensor);
                var colorPoint = mapper.MapSkeletonPointToColorPoint(position, ColorImageFormat.RgbResolution640x480Fps30);

                Shape circle = CreateCircle(colorPoint);
                canvas_draw.Children.Add(circle);
                //guardar referencia
                trackGesture.Add(circle);

               // videoCanvas.Children.Remove(circle);
                
               
                

                //Console.WriteLine("hand position: {0}:{1}", position.X, position.Y);
                
            
            }


        }

        private Shape CreateCircle(ColorImagePoint colorPoint)
        {
            var circle = new Ellipse();
            circle.Fill = System.Windows.Media.Brushes.Blue;
            circle.Height = 6;
            circle.Width = 6;
            circle.Stroke = System.Windows.Media.Brushes.Blue;
            circle.StrokeThickness = 2;

            Canvas.SetLeft(circle, colorPoint.X);
            Canvas.SetTop(circle, colorPoint.Y);

            Console.WriteLine("x:"+colorPoint.X +" y:"+colorPoint.Y);
            return circle;

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
        

        private void SpeechRecognitionEngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            //Console.WriteLine("matched:  {0}", speechRecognizedEventArgs.Result.Text);
            if (speechRecognizedEventArgs.Result.Confidence < .6)
                return;

            //string phraseDectected = speechRecognizedEventArgs.Result.Text;
            string phraseDectected = speechRecognizedEventArgs.Result.Text;
            //para poder entender los comandos kinect debe recibir la orden "arete"
            //MessageBox.Show(phraseDectected);
            if (phraseDectected == "kinect")
            {
                System.Windows.Forms.MessageBox.Show("escuchando");
                hearing = true;
                return;
            }

            //comandos de area
            if ((phraseDectected == "Finanzas") && (hearing==true))
            {
                //decirle a todos los browser su nueva direccion
                int i = 1;
                foreach (CoreBrowser browser in list) {
                    browser.setArguments("--new-window --enable-logging --app=" + linksBrowser[i - 1]);
                }


                return;
            }
            
            //comandos para personal
            if ((phraseDectected == "Oscar Diaz") && (hearing == true))
            {
                //si se logea mediante voz se debe de habilitar el logeo 
                userLogged = true;
               
                return;
            }
             //comando de salida del sistema
            if ((phraseDectected == "salir") && (hearing == true))
            {
                
                 hearing = false;
                 return;
            }
        }

        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    System.Windows.Media.Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        private void LoadSkeleton()
        {
           
            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);
            imageCanvasSkeleton.Source = this.imageSource;

            if (null != sensor)
            {
                sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            }
        }

      
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                dc.DrawRectangle(System.Windows.Media.Brushes.LightSlateGray, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        //RenderClippedEdges(skel, dc);
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);

            //reemplazar la mano
            handSet = true;
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);
            handSet = false;

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                System.Windows.Media.Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private System.Windows.Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new System.Windows.Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {   
            
            //texto preformateado
            System.Windows.FlowDirection p = System.Windows.FlowDirection.LeftToRight;

            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            //Console.WriteLine(joint0.Position + "");

            FormattedText formattedText = new FormattedText(this.SkeletonPointToScreen(joint0.Position)+ "", CultureInfo.GetCultureInfo("en-us"),
                                              p,
                                              new Typeface(new System.Windows.Media.FontFamily("Arial").ToString()),
                                              30, System.Windows.Media.Brushes.White);


            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            System.Windows.Media.Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }
            //dibuja la linea de union del hueso
            drawingContext.DrawEllipse(this.centerPointBrush, null, this.SkeletonPointToScreen(joint0.Position), BodyCenterThickness, BodyCenterThickness);
            drawingContext.DrawText(formattedText,this.SkeletonPointToScreen(joint0.Position));
            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));

            //dibujar una mano para facilitar el rastreo
            if(handSet){
                drawingContext.DrawEllipse(this.handColorCursor, null, this.SkeletonPointToScreen(joint1.Position), BodyCenterThickness, BodyCenterThickness);
            }



        }

        
       
    }
}
