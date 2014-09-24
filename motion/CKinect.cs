using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Windows.Media;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace motion
{
    class CKinect
    {
        private KinectSensor kinectSensor = null;
        private ColorFrameReader colorFrameReader = null;
        private WriteableBitmap colorBitmap = null;
        public System.Windows.Controls.Image streamVideo;
        public System.Windows.Controls.Image streamSkeleton;

        //variables para el esqueleto
        private const double HandSize = 30;
        private const double JointThickness = 5;
        private const double ClipBoundsThickness = 10;
        private const float InferredZPositionClamp = 0.8f;
        private readonly System.Windows.Media.Brush handClosedBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 255, 0, 0));
        private System.Windows.Media.Brush handOpenBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 255, 0));
        private readonly System.Windows.Media.Brush handLassoBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 255));
        private readonly System.Windows.Media.Brush handTracking = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 255, 255, 255));
        private readonly System.Windows.Media.Brush trackedJointBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 68, 192, 68));
        private readonly System.Windows.Media.Brush inferredJointBrush = System.Windows.Media.Brushes.Yellow;
        private readonly System.Windows.Media.Pen inferredBonePen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Gray, 1);
        private System.Windows.Media.DrawingGroup drawingGroup;
        

       
        private CoordinateMapper coordinateMapper = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private List<Tuple<JointType, JointType>> bones;
        private int displayWidth;
        private int displayHeight;
        private List<System.Windows.Media.Pen> bodyColors;
        

        //dos colores de esqueleto activo o no activo
        private Pen skeletonInactive = new Pen(Brushes.Orange,6);
        private Pen skeletonActive   = new Pen(Brushes.Green,6);

        //variables para controlar los gestos y saber cual es el esqueleto activo
        private ulong skeletonIdAttention = 0;
        private ulong skeletonIdActive = 0;

        //guardar las posiciones de la mano
        //tracking angulo formado
        private List<CPoint> trackingPositionRight;
        private List<CPoint> trackingPositionLeft;

        //variables para el gesto de dibujo de linea 
        private double distanceSensibilityX = 30;
        private double distanceSensibilityY = 10;

        private string gestureActive = "icons";
        private System.Windows.Forms.Timer timerGesture = new System.Windows.Forms.Timer();
        private Canvas loaderGesture;
        private int widthLoader = 0;



        public void InitializeKinect(System.Windows.Controls.Image streamVideo, System.Windows.Controls.Image streamSkeleton, List<CPoint> trackingPositionRight, List<CPoint> trackingPositionLeft, Canvas loaderGesture, MainWindow mainWin)
        {
            //referencia al display del cargador de gesto en la interfaz
            this.loaderGesture = loaderGesture;
            //referencia a la aplicación principal
            this.MainApplication = mainWin;

            //timer gesture
            timerGesture.Tick += new EventHandler(TimerGestureEvent);
            timerGesture.Interval = 12;


            //referencias para los streams de video y esqueleto
            this.streamVideo = streamVideo;
            this.streamSkeleton = streamSkeleton;
            //referencias a las listas de posiciones validas
            this.trackingPositionRight = trackingPositionRight;
            this.trackingPositionLeft = trackingPositionLeft;

            //sensor
            this.kinectSensor = KinectSensor.GetDefault();

            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            // set IsAvailableChanged event notifier
            //this.kinectSensor.Open();
            this.streamVideo.Source = this.colorBitmap;

            //inicializar esqueleto
            InitializeSkeleton();

        }



        public void InitializeSkeleton() {

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // open the sensor
            this.kinectSensor.Open();

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.streamSkeleton.Source  = new DrawingImage(this.drawingGroup);

            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }

           
        }

        public void CloseSensor() {
            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable

                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                        
                    }
                }
            }
        }// ----------------------------------------------------------------------------------------

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                   
                    foreach (Body body in this.bodies)
                    {
                        //solo hay dos tipos de esqueletos activos y no activos activos con color verde y no activos color naranja


                        if (body.IsTracked)
                        {
                            
                            //guardar el id del esqueleto trakeado que sera quien tendra el control momentameamente hasta que otra persona alce la mano derecha para pedir la palabra.
                            //guardar el id del esqueleto activo
                            skeletonIdActive = body.TrackingId;

                            Pen drawPen = null;
                            if(skeletonIdAttention ==skeletonIdActive){
                                drawPen = skeletonActive;
                            }
                            else {
                                drawPen = skeletonInactive;
                            }


                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }
                            //dibuja todo el cuerpo
                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }//--------------------------------------------------------------------------------------------------------

        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }

            //GESTOS
            //obtener la atención del kinect

            //siguiente set  (desplazamiento de la mano de derecha a izquierda )
            Point pointHandRight = jointPoints[JointType.HandRight];
            Point pointHandLeft = jointPoints[JointType.HandLeft];
            Point pointHead = jointPoints[JointType.Head];
            Point pointSpineMid = jointPoints[JointType.SpineMid];
            Point pointElbow = jointPoints[JointType.ElbowRight];

            CameraSpacePoint positionHandZ = joints[JointType.HandRight].Position;


           
            

            if (GestureGetAttentionKinect(pointHandRight, pointHead, pointElbow) == true)
            {
                //guardar el id del esqueleto que pidio la palabra
                skeletonIdAttention = skeletonIdActive;
            }
            //si no esta pidiendo la palabra quizas este realizando un gesto para cambiar las pantallas 
            else  if (skeletonIdAttention == skeletonIdActive) { 

                    //validar movimientos  set siguiente y set anterior
                    RunGesture(jointPoints, drawingContext);

                } //cualquier otro esqueleto que intente interactura no puede hacer nada

        }//-------------------------------------

       
        public void RunGesture(IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            
            if (gestureActive.Equals("swift"))
            {

                //se considera un movimiento para el set siguiente valido aquel que se mueve de derecha a izquierda con la mano derecha.
                trackingGestureSwift("right",jointPoints, drawingContext);
                trackingGestureSwift("left", jointPoints,drawingContext);
            }
            else
                if (gestureActive.Equals("icons"))
                {
                    trackingGestureIcons(jointPoints,drawingContext);

                }
        
        }

        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }//--------------------------------------------------


        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, 15, 15);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, 15, 15);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, 15, 15);
                    break;
            }
        }//--------------------------------------------------------------------------------------


        //----------------------------- CARGA DE IMAGENES----------------------------------//


        private void showImage(string imageUrl ,double posX, double posY, DrawingContext drawingContext)
        {
            //mostrar los botones a un lado del cuerpo para indicar que es siguiente y anterior
            BitmapImage imgSrcNext = new BitmapImage();
            imgSrcNext.BeginInit();
            imgSrcNext.UriSource = new Uri(@imageUrl, UriKind.RelativeOrAbsolute);
            imgSrcNext.EndInit();
            drawingContext.DrawImage(imgSrcNext, new Rect(posX, posY, 75, 75));
        }//-------------------------------------
       
        private Boolean GestureGetAttentionKinect(Point rightHand,Point head,Point pointElbow) {
            
            if (rightHand.X >head.X && (rightHand.Y+20)<head.Y) { 
                return true;
            }else{
                return false;
            }
        }//-----------------------------------

        private void trackingGestureSwift(string direction,IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        {
            Point pointHandRight = jointPoints[JointType.HandRight];
            Point pointHandLeft = jointPoints[JointType.HandLeft];
            Point pointHead = jointPoints[JointType.Head];
            Point pointSpineMid = jointPoints[JointType.SpineMid];
            Point pointElbow = jointPoints[JointType.ElbowRight];

            if (direction.Equals("right"))
            {
                // La mano debe de estar del lado derecho de la columna y a una altura superior o igual a la altura de esta
                if (trackingPositionRight.Count == 0)
                { //agregar sin validacion
                    CPoint nPoint = new CPoint(pointHandRight.X, pointHandRight.Y);
                    //CPoint nPoint = new CPoint(400,221);
                    trackingPositionRight.Add(nPoint);                }
                else
                {
                    CPoint lastElementInserted = trackingPositionRight[trackingPositionRight.Count - 1];
                    //la altura entre el punto anterior guardado y el reciente no debe de pasar "distanceSensibility" para poder ser considerado como otro punto valido en el movimiento
                    double valueY = lastElementInserted.getPosY() - pointHandRight.Y;
                    double valueX = lastElementInserted.getPosX() - pointHandRight.X;

                    //dibujar traza guardada
                    DrawPointsSaved(drawingContext);

                    if (valueY < 0)
                    {
                        valueY = valueY * -1;
                    }

                    //si esta dentro del limite de "distanceSensibility" y la mano esta arriba de la mitad de la columna es válido
                    if (valueY <= distanceSensibilityY && (valueX > 0 && valueX <= distanceSensibilityX) && pointHandRight.Y <= pointSpineMid.Y)
                    {
                        CPoint nPoint = new CPoint(pointHandRight.X, pointHandRight.Y);
                        trackingPositionRight.Add(nPoint);
                    }
                }
            }
            else
            {

                if (trackingPositionLeft.Count == 0)
                { //agregar sin validacion
                    // trackingPositionLeft.Add(position);
                }
                else
                {
                    //double lastElementInserted = trackingPositionLeft[trackingPositionLeft.Count - 1];

                    //si la resta es positiva el se esta moviendo en la direccion indicada

                }


            }
        
        }//---------------------------------------------------

        private void trackingGestureIcons(IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext)
        { 
        
            //dibujar en todo momento los iconos que estaran a la misma altura que el hombro y separado 20 unidades después de este
            Point pointShoulderRight = jointPoints[JointType.ShoulderRight];
            Point pointShoulderLeft = jointPoints[JointType.ShoulderLeft];
            Point pointHandRight = jointPoints[JointType.HandRight];
            Point pointHandLeft = jointPoints[JointType.HandLeft];

            double[] posIconRight = { pointShoulderRight.X + 15, pointShoulderRight.Y - 75 };
            double[] posIconLeft = { pointShoulderLeft.X - 90, pointShoulderLeft.Y - 75 };

            showImage("imgNext.png",posIconRight[0], posIconRight[1], drawingContext);
            showImage("imgPrevious.png",posIconLeft[0], posIconLeft[1], drawingContext);


            Point p1 = new Point();
            p1.X = pointShoulderLeft.X - 90;
            p1.Y = pointShoulderLeft.Y - 75;

            Point p2 = new Point();
            p2.X = pointShoulderLeft.X - 15;
            p2.Y = pointShoulderLeft.Y - 75;

            Point p3 = new Point();
            p3.X = pointShoulderLeft.X - 90;
            p3.Y = pointShoulderLeft.Y ;

            Point p4 = new Point();
            p4.X = pointShoulderLeft.X - 15;
            p4.Y = pointShoulderLeft.Y;

            drawingContext.DrawEllipse(this.handClosedBrush, null, p1, 5, 5);
            drawingContext.DrawEllipse(this.handClosedBrush, null, p2, 5, 5);
            drawingContext.DrawEllipse(this.handClosedBrush, null, p3, 5, 5);
            drawingContext.DrawEllipse(this.handClosedBrush, null, p4, 5, 5);




            //verificar que la mano derecha se encuentre en el área del boton marcado

            if ((pointHandRight.X > pointShoulderRight.X + 15) && (pointHandRight.X < pointShoulderRight.X+75)
                && (pointHandRight.Y > pointShoulderRight.Y - 75) && (pointHandRight.Y < pointShoulderRight.Y))
                
            {
                //si al tener la mano sobre le icono se cumple el tiempo de espera del loader cambia a el siguiente set
                changeSet("right");

                //cambiar el cursor por una mano 
                showImage("handRight.png", posIconRight[0], posIconRight[1], drawingContext);
                handOpenBrush =  new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 183, 82, 233));
                
                if (!timerGesture.Enabled) {
                    timerGesture.Start();
                }


            }
            else if ((pointHandLeft.X > pointShoulderLeft.X - 90 && pointHandLeft.X < pointShoulderLeft.X - 15)
                 && (pointHandLeft.Y > pointShoulderLeft.Y - 75 && pointHandLeft.Y < pointShoulderLeft.Y)
                ){//validar lado izquierdo

                changeSet("left");
                //cambiar el cursor por una mano 
                showImage("handLeft.png", posIconLeft[0], posIconLeft[1], drawingContext);
                handOpenBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 183, 82, 233));

                if (!timerGesture.Enabled)
                {
                    timerGesture.Start();
                }


               
            }else{ // si esta fuera de los iconos
                
                handOpenBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 255, 0));

                //resetear el valor del ancho de la pantalla
                widthLoader = 0;
                timerGesture.Stop();
                //quitar el diseño visual del cargador de la interfaz
                loaderGesture.Children.Clear();
            }



        }//------------------------------------------------


        private void changeSet(string value) {
            if (widthLoader == 1000) {
                timerGesture.Stop();
                MainApplication.updateBrowserWithSet(value);
                widthLoader = 0;
            }
        
        }


        private void TimerGestureEvent(object sender, EventArgs e)
        {
            //crear borde del cargador
            Rectangle rect = new Rectangle();
            rect.Width = 1000;
            rect.Height = 100;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            rect.StrokeThickness = 5;
            

            Rectangle loader = new Rectangle();
            loader.Width = widthLoader;
            loader.Height = 100;
            loader.Fill = new SolidColorBrush(Colors.Red);

            if (widthLoader < 1000)
            {
                //crear relleno
                loader.Width = loader.Width + 10;
                loaderGesture.Children.Add(loader);
                loaderGesture.InvalidateVisual();
                widthLoader = widthLoader + 10;

                loaderGesture.Children.Add(rect);
                
                
            }
            

        }//--------------------------------------------
        

        public void DrawPointsSaved(DrawingContext drawingContext) {
            foreach (CPoint point in trackingPositionRight) {
                Point p = new Point();
                p.X = point.getPosX();
                p.Y = point.getPosY();

                drawingContext.DrawEllipse(this.handTracking, skeletonInactive, p, 5, 5);
            }
            
        }


        //variables gesto de seleccion de iconos
        private MainWindow MainApplication;

    }
}
