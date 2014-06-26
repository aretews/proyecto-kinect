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

namespace KinectFinalProyect
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private KinectSensor sensor;
        public MainWindow()
        {
            //crear componentes
            InitializeComponent();

            //inicializar tamaños de textos
            RedefineFonts();

            //mostrar navegador
            RunBrowser("http://www.chartjs.org/");
            //activar el kinect
            //al hacer click sobre el boton activar  cambios

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
        //correr el sensor
        public Boolean RunSensor() {
            if (getNumerOfKinectsConnected() > 0)
            {   
                if (sensor.IsRunning)
                {
                    
                    sensor.Stop();
                    MessageBox.Show("sensor inactivo");
                    return false;
                }
                else {

                    sensor = KinectSensor.KinectSensors[0];
                    sensor.Start();
                    return true;
                }
            }
            else
            {
                MessageBox.Show("kinect no detectado");
                return false;
            }
            
        }
        //mostrar contenido en el sitio
        public void RunBrowser(string site)
        {   
            //navegador.Navigate(site);
            Uri uri = new Uri(@site, UriKind.RelativeOrAbsolute);
            navegador.Source = uri;
            label_status.Content = "Navegador activo";

        }


        //crear una imagen de bits a partir de la ruta local de una imagen
        public BitmapImage getBitMapSrc(string path) {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            return bitmap;
        }
        public BitmapSource getAPicture(KinectSensor sensor){
            using (var frame = sensor.ColorStream.OpenNextFrame(1000))
            {
                //crear un array del tamaño del frame
                var pixelData = new byte[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixelData);
                var stride = frame.Width * frame.BytesPerPixel;
                var bitmap = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
                return bitmap;
            }
        }

        //aqui se activa el sistema 
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (btn_activar.Content+""== "Activar")
            {
                //verificar cuantos kinects estan conectados
                if (KinectSensor.KinectSensors.Count > 0) { 
                    KinectSensor.KinectSensors.StatusChanged +=(o,args) =>
                                                                {
   
                                                                };
                    //asignar solo el primer kinect
                    sensor = KinectSensor.KinectSensors[0];
                    //activar sensor
                    sensor.Start();
                    btn_activar.Content = "Desactivar";
                    //cambiar estatus en interfaz
                    status.Source = getBitMapSrc(@"C:\openCV_proyect_face_recognition\KinectFinalProyect\KinectFinalProyect\img\status.png");

                    //habilitar el stream de color
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);

                    //tomar una foto
                    imageCanvas.Background = new ImageBrush(getAPicture(sensor));
                    
                }
                

            }
            else {
                if (sensor != null && sensor.IsRunning) {

                    sensor.ColorStream.Disable();
                    sensor.Stop();

                    //cambiar estatus en interfaz
                    status.Source = getBitMapSrc(@"C:\openCV_proyect_face_recognition\KinectFinalProyect\KinectFinalProyect\img\status_off.png");

                    //cambiar estado del boton
                    btn_activar.Content = "Activar";
                }
            }
        }


       
    }
}
