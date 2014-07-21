using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectFinalProyect
{
    public class CoreKinect
    {

        private KinectSensor sensor;
        private DepthImagePixel[] depthImagePixels;


        //obtener el sensor del kinect
        public KinectSensor getSensor()
        {
            return sensor;
        }
        public void setSensor(KinectSensor sensor) {

            this.sensor = sensor;
        }

        //obtener el numero de sensores conectados
        public int getNumerOfKinectsConnected()
        {
            return KinectSensor.KinectSensors.Count;
        }
        public KinectSensor getKinectNumber(int num) {
            return KinectSensor.KinectSensors[num];
        }

        //correr el sensor
        public Boolean RunSensor()
        {
            if (getNumerOfKinectsConnected() > 0)
            {
                if (sensor.IsRunning)
                {

                    sensor.Stop();
                    MessageBox.Show("sensor desactivado");
                    return false;
                }
                else
                {

                    sensor = KinectSensor.KinectSensors[0];
                    sensor.Start();
                    MessageBox.Show("sensor activado");
                    return true;
                }
            }
            else
            {
                MessageBox.Show("kinect no detectado");
                return false;
            }

        }

        //crear una imagen de bits a partir de la ruta local de una imagen
        public BitmapImage getBitMapSrc(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            return bitmap;
        }

        // a partir del sensor obtiene una imagen pasando un segundo
        public BitmapSource getAPicture(KinectSensor sensor)
        {
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

        //obtiene una imagen del frame pasado
        public  BitmapSource CreateBitmap(ColorImageFrame frame)
        {
            var pixelData = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(pixelData);
            //convesion a escala de grises
            GrayscaleData(pixelData);


            var stride = frame.Width * frame.BytesPerPixel;
            var bitmap = BitmapSource.Create(frame.Width,frame.Height,96,96,PixelFormats.Bgr32,null,pixelData,stride);

            return bitmap;
        }
        public BitmapSource CreateBitmap(ColorImageFrame frame,DepthImagePixel[] depthImagePixel)
        {
            //a veces existen frames nulos por lo que es necesario apartarlos del flujo 
            if (frame ==null)
            {
                return null;
            }
            else {
                var pixelData = new byte[frame.PixelDataLength];
                frame.CopyPixelDataTo(pixelData);
                //convesion a escala de grises
                this.depthImagePixels = depthImagePixel;
                GrayscaleDataDeep(pixelData);


                var stride = frame.Width * frame.BytesPerPixel;
                var bitmap = BitmapSource.Create(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);


                return bitmap;
            
            }
            
        }

        public  void GrayscaleData(byte[] pixelData)
        {
            for (int i = 0; i < pixelData.Length; i += 4)
            {
               
                 var max = Math.Max(pixelData[i], Math.Max(pixelData[i + 1], pixelData[i + 2]));
                 pixelData[i] = max;
                 pixelData[i + 1] = max;
                 pixelData[i + 2] = max;

            }
        }
        public void GrayscaleDataDeep(byte[] pixelData)
        {
            var mapper = new CoordinateMapper(sensor);
            var depthPoints = new DepthImagePoint[640 * 480];
            mapper.MapColorFrameToDepthFrame(ColorImageFormat.RgbResolution640x480Fps30,DepthImageFormat.Resolution640x480Fps30,depthImagePixels,depthPoints);

            for (int i = 0; i < depthPoints.Length; i++) {
                var point = depthPoints[i];
                if (point.Depth > 600 || KinectSensor.IsKnownPoint(point))
                {
                    var pixelDataIndex = i * 4;
                    var max = Math.Max(pixelData[pixelDataIndex], Math.Max(pixelData[pixelDataIndex + 1], pixelData[pixelDataIndex + 2]));
                    pixelData[pixelDataIndex] = max;
                    pixelData[pixelDataIndex + 1] = max;
                    pixelData[pixelDataIndex + 2] = max;
                }
                else { 
                    
                
                }
            }

           
        }

        //detectar la profundidad
        /*==================================================*\
         * PROFUNDIDAD
         * -los ultimos 13 bits del stream indican la distancia
         * a la cual se encuentra el objeto
         * -los primeros 3 bits indican a que usuario estan 
         * relacionados los pixeles
         * 
        \*==================================================*/


    }
}
