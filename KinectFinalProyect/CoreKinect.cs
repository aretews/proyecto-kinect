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


        //obtener el sensor del kinect
        public KinectSensor getSensor()
        {
            return sensor;
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


    }
}
