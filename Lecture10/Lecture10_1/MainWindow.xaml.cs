using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Kinect;

namespace Lecture10_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor sensor;
        Skeleton[] totalSkeleton = new Skeleton[6];
        WriteableBitmap colorBitmap;
        byte[] colorPixels;
        Skeleton skeleton;
        Thing thing = new Thing();
        double gravity = 0.017;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {            
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            this.sensor.SkeletonStream.Enable();
            this.sensor.SkeletonFrameReady += this.skeletonFrameReady;

            this.sensor.ColorStream.Enable();
            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            this.image.Source = this.colorBitmap;
            this.sensor.ColorFrameReady += this.colorFrameReady;
            this.sensor.Start();

            thing.Shape = new Ellipse();
            thing.Shape.Width = 30;
            thing.Shape.Height = 30;
            thing.Shape.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 255));
            thing.Center.X = 300;
            thing.Center.Y = 0;
            thing.Shape.SetValue(Canvas.LeftProperty, thing.Center.X - thing.Shape.Width);
            thing.Shape.SetValue(Canvas.TopProperty, thing.Center.Y - thing.Shape.Width);
            canvas1.Children.Add(thing.Shape);
        }

        void colorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (null == imageFrame)
                    return;

                imageFrame.CopyPixelDataTo(colorPixels);
                int stride = imageFrame.Width * imageFrame.BytesPerPixel;

                this.colorBitmap.WritePixels(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight), this.colorPixels, stride, 0);
            }
        }

        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            canvas1.Children.Clear();
            advanceThingPosition();
            canvas1.Children.Add(thing.Shape);
            
            using(SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return;
                }
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);
                skeleton = (from trackskeleton in totalSkeleton where trackskeleton.TrackingState == SkeletonTrackingState.Tracked select trackskeleton).FirstOrDefault();
                if (skeleton == null)
                {
                    return;
                }
                if (skeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                {
                    this.MapJointsWithUIElement(skeleton);
                }
            }
           
            Point handPt = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
            if (thing.Hit(handPt))
            {
                this.thing.YVelocity = -1.0 * this.thing.YVelocity;
            }
        }

        private void MapJointsWithUIElement(Skeleton skeleton)
        {
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
            Canvas.SetLeft(righthand, mappedPoint.X);
            Canvas.SetTop(righthand, mappedPoint.Y);
        }

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        
        private struct Thing
        {
            public Point Center;
            public double YVelocity;
            public double XVelocity;
            public Ellipse Shape;

            public bool Hit(Point joint)
            {
                double minDxSquared = this.Shape.RenderSize.Width;
                minDxSquared *= minDxSquared;
                double dist = SquaredDistance(Center.X, Center.Y, joint.X, joint.Y);
                if (dist <= minDxSquared)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        private static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            return ((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1));
        }        

        void advanceThingPosition()
        {
            thing.Center.Offset(thing.XVelocity, thing.YVelocity);
            thing.YVelocity += this.gravity;
            thing.Shape.SetValue(Canvas.LeftProperty, thing.Center.X - thing.Shape.Width);
            thing.Shape.SetValue(Canvas.TopProperty, thing.Center.Y - thing.Shape.Width);

            // if goes out of bound, reset position, as well as velocity
            if (thing.Center.Y >= canvas1.Height)
            {
                thing.Center.Y = 0;
                thing.XVelocity = 0;
                thing.YVelocity = 0;
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if(this.sensor!=null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }
    }
}
