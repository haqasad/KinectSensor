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

namespace Lecture4_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(WindowLoaded);
            Closing += new CancelEventHandler(WindowClosing);
        }

        KinectSensor sensor;

        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;

        Skeleton[] totalSkeleton = new Skeleton[6];
        
        Ellipse ellipse;
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {            
            if (KinectSensor.KinectSensors.Count > 0)
            {                                
                this.sensor = KinectSensor.KinectSensors[0];
                if (this.sensor != null && !this.sensor.IsRunning)
                {

                    this.sensor.ColorStream.Enable();
                    this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                    this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    this.image1.Source = this.colorBitmap;
                    this.sensor.ColorFrameReady += this.colorFrameReady;

                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                    this.sensor.SkeletonStream.Enable();
                    this.sensor.SkeletonFrameReady += skeletonFrameReady;

                    
                    this.sensor.Start();
                }
                else
                {
                    MessageBox.Show("No device is connected!");
                    this.Close();
                }                
            }
        }

        void colorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (null == imageFrame)
                    return;

                imageFrame.CopyPixelDataTo(colorPixels);
                int stride = imageFrame.Width * imageFrame.BytesPerPixel;

                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight), this.colorPixels, stride, 0);
                //canvas1.Children.Add(image1);
            }
        }

        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            canvas1.Children.Remove(ellipse);
            
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {                
                ellipse = new Ellipse();
                ellipse.Height = 20;
                ellipse.Width = 20;
                ellipse.Fill = new SolidColorBrush(Colors.Black);
                

                if (skeletonFrame == null)
                {
                    return;
                }
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);
                Skeleton firstSkeleton = (from trackskeleton in totalSkeleton
                                          where trackskeleton.TrackingState == SkeletonTrackingState.Tracked
                                          select trackskeleton).FirstOrDefault();
                if (firstSkeleton == null)
                {
                    return;
                }
                
                Point pointHand = new Point();
                pointHand = this.MapJointsWithUIElement(firstSkeleton);

                Canvas.SetLeft(ellipse, pointHand.X);
                Canvas.SetTop(ellipse, pointHand.Y);
                
                canvas1.Children.Add(ellipse);
            }
        }

        //private void DrawSkeleton(Skeleton skeleton)
        //{
        //    drawBone(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
        //    drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);

        //    drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
        //    drawBone(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
        //    drawBone(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
        //    drawBone(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

        //    drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
        //    drawBone(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
        //    drawBone(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
        //    drawBone(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

        //    drawBone(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
        //    drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
        //    drawBone(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
        //    drawBone(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
        //    drawBone(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

        //    drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
        //    drawBone(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
        //    drawBone(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
        //    drawBone(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);
        //}

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        private Point MapJointsWithUIElement(Skeleton skeleton)
        {
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.HandRight].Position);            
            return mappedPoint;
        }


        //void drawBone(Joint trackedJoint1, Joint trackedJoint2)
        //{
        //    Line bone = new Line();
        //    bone.Stroke = Brushes.Red;
        //    bone.StrokeThickness = 3;
        //    Point joint1 = this.ScalePosition(trackedJoint1.Position);
        //    bone.X1 = joint1.X;
        //    bone.Y1 = joint1.Y;

        //    Point joint2 = this.ScalePosition(trackedJoint2.Position);
        //    bone.X2 = joint2.X;
        //    bone.Y2 = joint2.Y;
            
        //    canvas1.Children.Add(bone);            
        //}


        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }        
    }
}
