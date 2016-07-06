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


namespace Lecture9_2
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
        WriteableBitmap colorBitmap;
        byte[] colorPixels;
        Skeleton skeleton;
        int currentSkeletonID = 0;

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
            this.sensor.SkeletonFrameReady += skeletonFrameReady;

            this.sensor.ColorStream.Enable();
            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            this.image.Source = this.colorBitmap;
            this.sensor.ColorFrameReady += this.colorFrameReady;
            this.sensor.Start();
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
            textBox.Clear();
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null) { return; }
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);
                skeleton = (from trackskeleton in totalSkeleton where trackskeleton.TrackingState == SkeletonTrackingState.Tracked select trackskeleton).FirstOrDefault();
                if (skeleton == null)
                    return;
                if (skeleton != null && this.currentSkeletonID != skeleton.TrackingId)
                {
                    this.textBox.Text += "" + calculateAngle();
                }
                DrawSkeleton(skeleton);
                drawArc();
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

        private string calculateAngle()
        {
            Vector joint11 = (Vector)this.ScalePosition(skeleton.Joints[JointType.ElbowRight].Position);
            Vector joint12 = (Vector)this.ScalePosition(skeleton.Joints[JointType.ShoulderRight].Position);

            Vector joint21 = (Vector)this.ScalePosition(skeleton.Joints[JointType.Spine].Position);
            Vector joint22 = (Vector)this.ScalePosition(skeleton.Joints[JointType.ShoulderCenter].Position);

            Vector rightArm = Vector.Add(joint11, joint12);
            Vector torso = Vector.Add(joint21, joint22);
            
            string angle = Vector.AngleBetween(rightArm, torso).ToString("F2");
            return angle;
        }

        void drawArc()
        {
            Point joint1 = this.ScalePosition(skeleton.Joints[JointType.WristRight].Position);
            Point joint2 = this.ScalePosition(skeleton.Joints[JointType.Spine].Position);
            /* PathFigure class: represents a subsection of a geometry, a single connected series of two-dimensional
             * geometric segments */
            PathFigure pthFigure = new PathFigure();
            /* PathFigure StartPoint: 
             * Property;
             * Gets or sets the Point where the PathFigure begins */
            pthFigure.StartPoint = new Point(joint1.X, joint1.Y);

            /* ArcSegment class: represents an elliptical arc between two points */
            ArcSegment arcSeg = new ArcSegment();
            /* ArcSegment Point:
             * Property;
             * Gets or sets the endpoint of the elliptical arc */
            arcSeg.Point = new Point(joint2.X, joint2.Y);
            /* ArcSegment Size:
             * Property;
             * Gets or sets the x and y radius of the arc as a Size structure */
            arcSeg.Size = new Size(20, 20);
            /* ArcSegment IsLargeArc:
             * Property;
             * Gets or sets a value that indicates whether the arc should be greater than 180 degrees */
            arcSeg.IsLargeArc = false;
            /* ArcSegment SweepDirection:
             * Property;
             * Gets or sets a value that specifies whether the arc is drawn in the Clockwise or Counterclockwise
             * direction */
            arcSeg.SweepDirection = SweepDirection.Clockwise;
            /* ArcSegment RotationAngle:
             * Property;
             * Gets or sets the amount (in degrees) by which the ellipse is rotated about the x-axis */
            arcSeg.RotationAngle = 0;

            /* PathSegmentCollection class:
             * Represents a collection of PathSegment objects that can be individually accessed by index */
            PathSegmentCollection myPathSegmentColloection = new PathSegmentCollection();
            /* PathSegmentCollection Add:
             * Adds a PathSegment to the end of the collection */
            myPathSegmentColloection.Add(arcSeg);

            /* PathFigure Segments:
             * Property;
             * Gets or sets the collection of segments that define the shape of this PathFigure object */
            pthFigure.Segments = myPathSegmentColloection;

            /* PathFigureCollection class: */
            PathFigureCollection pthFigureCollection = new PathFigureCollection();
            /* PathFigureCollection Add: */
            pthFigureCollection.Add(pthFigure);

            /* PathGeometry class: */
            PathGeometry pthGeometry = new PathGeometry();
            /* PathGeometry Figures: */
            pthGeometry.Figures = pthFigureCollection;

            /* Path class: Draws a series of connected lines and curves */
            Path arcPath = new Path();
            /* Path Stroke:
             * Property;
             * Gets or sets the Brush that specifies how the Shape outline is painted */
            arcPath.Stroke = new SolidColorBrush(Colors.Black);
            /* Path StrokeThickness:
             * Property;
             * Gets or sets the width of the Shape outline */
            arcPath.StrokeThickness = 1;
            /* Path Data:
             * Property;
             * Gets or sets the geometry that specifies the shape to be drawn */
            arcPath.Data = pthGeometry;
            /* Path Fill:
             * Property;
             * Gets or sets the Brush that specifies how the shape's interior is painted */
            //arcPath.Fill = new SolidColorBrush(Colors.Yellow);

            canvas1.Children.Add(arcPath);
        }

        

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution320x240Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
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
