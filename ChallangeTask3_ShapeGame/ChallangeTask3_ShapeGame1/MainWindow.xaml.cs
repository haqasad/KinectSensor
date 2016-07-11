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

namespace ChallangeTask3_ShapeGame1
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

        Skeleton[] totalSkeleton = new Skeleton[6];
        Skeleton skeleton;
        int currentSkeletonID = 0;

        WriteableBitmap colorBitmap;
        byte[] colorPixels;

        Thing thing = new Thing();
        double gravity = 0.06;

        int count = 0;
        int Count = 0;
        TextBox textBox = new TextBox();

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            var smoothParameters = new TransformSmoothParameters
            {
                Correction = 0.1f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.05f,
                Prediction = 0.1f,
                Smoothing = 0.5f
            };
            this.sensor.SkeletonStream.Enable(smoothParameters);
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
            thing.Shape.SetValue(Canvas.LeftProperty, (thing.Center.X - thing.Shape.Width));
            thing.Shape.SetValue(Canvas.TopProperty, (thing.Center.Y - thing.Shape.Width));
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

            Canvas.SetLeft(textBox, (thing.Center.X - thing.Shape.Width + 50));
            Canvas.SetTop(textBox, (thing.Center.Y - thing.Shape.Width + 5));            
            canvas1.Children.Add(textBox);

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
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
                if (skeleton != null && this.currentSkeletonID != skeleton.TrackingId)
                {
                    this.currentSkeletonID = skeleton.TrackingId;
                }
                DrawSkeleton(skeleton);
            }
            Point rht_handPt = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
            Point rht_wristPt = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
            Point rht_elbowPt = ScalePosition(skeleton.Joints[JointType.ElbowRight].Position);

            Point lft_handPt = ScalePosition(skeleton.Joints[JointType.HandLeft].Position);
            Point lft_wristPt = ScalePosition(skeleton.Joints[JointType.WristLeft].Position);
            Point lft_elbowPt = ScalePosition(skeleton.Joints[JointType.ElbowLeft].Position);

            if (thing.Hit(rht_handPt) || thing.Hit(rht_wristPt) || thing.Hit(rht_elbowPt))
            {
                this.thing.YVelocity = -1.0 * this.thing.YVelocity;
                if (rightsideAngle() > 90)
                {                                           
                    this.thing.XVelocity = (1.0 * this.thing.YVelocity) / 6;
                }
                else
                {
                    this.thing.XVelocity = (-1.0 * this.thing.YVelocity) / 6;
                }
                Count++;
                if (Count == 1)
                {
                    count++;
                    this.textBox.Text = "" + count;
                }
            }
            else if (thing.Hit(lft_handPt) || thing.Hit(lft_wristPt) || thing.Hit(lft_elbowPt))
            {
                this.thing.YVelocity = -1.0 * this.thing.YVelocity;
                if (leftsideAngle() > 90)
                {
                    this.thing.XVelocity = (-1.0 * this.thing.YVelocity) / 6;
                }
                else
                {
                    this.thing.XVelocity = (1.0 * this.thing.YVelocity) / 6;
                }
                Count++;
                if (Count == 1)
                {
                    count++;
                    this.textBox.Text = "" + count;
                }
            }
            else
            {
                Count = 0;
            }
        }

        private void DrawSkeleton(Skeleton skeleton)
        {
            drawBone(skeleton.Joints[JointType.Head], skeleton.Joints[JointType.ShoulderCenter]);
            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Spine]);

            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft]);
            drawBone(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft]);
            drawBone(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft]);
            drawBone(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft]);

            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight]);
            drawBone(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight]);
            drawBone(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight]);
            drawBone(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight]);

            drawBone(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.HipCenter]);
            drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipLeft]);
            drawBone(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]);
            drawBone(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.AnkleLeft]);
            drawBone(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.FootLeft]);

            drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.HipRight]);
            drawBone(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]);
            drawBone(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.AnkleRight]);
            drawBone(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.FootRight]);
        }

        void drawBone(Joint trackedJoint1, Joint trackedJoint2)
        {
            Line bone = new Line();
            bone.Stroke = Brushes.Red;
            bone.StrokeThickness = 3;
            Point joint1 = this.ScalePosition(trackedJoint1.Position);
            bone.X1 = joint1.X;
            bone.Y1 = joint1.Y;

            Point joint2 = this.ScalePosition(trackedJoint2.Position);
            bone.X2 = joint2.X;
            bone.Y2 = joint2.Y;

            canvas1.Children.Add(bone);
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
            if (thing.Center.Y >= canvas1.Height || thing.Center.X >= canvas1.Width)
            {
                thing.Center.X = 300;
                thing.Center.Y = 0;
                thing.XVelocity = 0;
                thing.YVelocity = 0;
                count = 0;
            }
        }

        float rightsideAngle()
        {
            Point rightElbow = this.ScalePosition(skeleton.Joints[JointType.ElbowRight].Position);
            Point rightShoulder = this.ScalePosition(skeleton.Joints[JointType.ShoulderRight].Position);
            Point rightHip = this.ScalePosition(skeleton.Joints[JointType.HipRight].Position);

            double dist_rhtElbow_rhtShoulder = Math.Sqrt(Math.Pow((rightElbow.X - rightShoulder.X), 2) + Math.Pow((rightElbow.Y - rightShoulder.Y), 2));
            double dist_rhtShoulder_rhtHip = Math.Sqrt(Math.Pow((rightShoulder.X - rightHip.X), 2) + Math.Pow((rightShoulder.Y - rightHip.Y), 2));
            double dist_rhtHip_rhtElbow = Math.Sqrt(Math.Pow((rightHip.X - rightElbow.X), 2) + Math.Pow((rightHip.Y - rightElbow.Y), 2));
            return (float)(Math.Acos((Math.Pow(dist_rhtElbow_rhtShoulder, 2) + Math.Pow(dist_rhtShoulder_rhtHip, 2) - Math.Pow(dist_rhtHip_rhtElbow, 2)) / (2 * dist_rhtElbow_rhtShoulder * dist_rhtShoulder_rhtHip)) * (180 / Math.PI));
        }

        float leftsideAngle()
        {
            Point leftElbow = this.ScalePosition(skeleton.Joints[JointType.ElbowLeft].Position);
            Point leftShoulder = this.ScalePosition(skeleton.Joints[JointType.ShoulderLeft].Position);
            Point leftHip = this.ScalePosition(skeleton.Joints[JointType.HipLeft].Position);

            double dist_lftElbow_lftShoulder = Math.Sqrt(Math.Pow((leftElbow.X - leftShoulder.X), 2) + Math.Pow((leftElbow.Y - leftShoulder.Y), 2));
            double dist_lftShoulder_lftHip = Math.Sqrt(Math.Pow((leftShoulder.X - leftHip.X), 2) + Math.Pow((leftShoulder.Y - leftHip.Y), 2));
            double dist_lftHip_lftElbow = Math.Sqrt(Math.Pow((leftHip.X - leftElbow.X), 2) + Math.Pow((leftHip.Y - leftElbow.Y), 2));
            return (float)(Math.Acos((Math.Pow(dist_lftElbow_lftShoulder, 2) + Math.Pow(dist_lftShoulder_lftHip, 2) - Math.Pow(dist_lftHip_lftElbow, 2)) / (2 * dist_lftElbow_lftShoulder * dist_lftShoulder_lftHip)) * (180 / Math.PI));
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }
    }
}
