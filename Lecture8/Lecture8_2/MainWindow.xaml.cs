﻿using System;
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
using System.Drawing;
using Microsoft.Kinect;
using System.ComponentModel;

namespace Lecture8_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //SolidColorBrush b;
        public MainWindow()
        {
            InitializeComponent();
            //b = this.Resources["colorChange"] as SolidColorBrush;

        }
        //MainWindow m = new MainWindow();
        
        KinectSensor sensor;
        Skeleton[] totalSkeleton = new Skeleton[6];
        //Point x;      

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            
            this.sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(skeletonFrameReady);            

            var smoothParameters = new TransformSmoothParameters
            {
                Correction = 0.05f,
                JitterRadius = 0.5f,
                MaxDeviationRadius = 0.1f,
                Prediction = 0.1f,
                Smoothing = 0.8f
            };

            this.sensor.SkeletonStream.Enable(smoothParameters);
            this.sensor.Start();

            //thing.ellipse = new Ellipse();
            //thing.ellipse.Width = 5;
            //thing.ellipse.Height = 5;
            //thing.ellipse.Fill = new SolidColorBrush(Colors.Black);
            //skeletonCanvas.Children.Add(thing.ellipse);
        }

        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return;
                }
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);
                Skeleton firstSkeleton = (from trackskeleton in totalSkeleton where trackskeleton.TrackingState == SkeletonTrackingState.Tracked select trackskeleton).FirstOrDefault();
                if (firstSkeleton == null)
                {
                    return;
                }
                if (firstSkeleton.Joints[JointType.WristRight].TrackingState == JointTrackingState.Tracked)
                {
                    getPoint = this.MapJointsWithUIElement(firstSkeleton);
                    //thing.getPoint = thing.MapJointsWithUIElement(firstSkeleton);                    
                }
                //return firstSkeleton;
            }
            
        }                                 

        //private struct Thing
        //{
        //    //public Point Center;
        //    //public Polyline ellipse;
        //    //public Ellipse ellipse;
        //    //Point mappedPoint = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
        //    //KinectSensor sensor;
        //    Point trailPoint;
        //    //public Pen p;
        //    public void change(int t)
        //    {
        //        int caseSwitch = t;
        //        switch (caseSwitch)
        //        {
        //            case 1:
        //                //Polyline redpolyline = new Polyline();
        //                redpolyline.Stroke = new SolidColorBrush(Colors.Red);
        //                redpolyline.Points.Add(new Point(trailPoint.X = getPoint.X, trailPoint.Y = getPoint.Y));                       
        //                break;
        //            case 2:
        //                Polyline bluepolyline = new Polyline();
        //                bluepolyline.Fill = new SolidColorBrush(Colors.Red);
        //                bluepolyline.Points.Add(new Point(trailPoint.X = getPoint.X, trailPoint.Y = getPoint.Y));
        //                break;
        //            default:
        //                Polyline defaultpolyline = new Polyline();
        //                defaultpolyline.Stroke = new SolidColorBrush(Colors.Black);
        //                defaultpolyline.Points.Add(new Point(trailPoint.X = getPoint.X, trailPoint.Y = getPoint.Y));
        //                break;
        //        }

        //    }

        //    public Point getPoint { get; set; }

        //    //public Point MapJointsWithUIElement(Skeleton skeleton)
        //    //{
        //    //    Point mappedPoint = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
        //    //    //Canvas.SetLeft(righthand, mappedPoint.X);
        //    //    //Canvas.SetTop(righthand, mappedPoint.Y);
        //    //    return mappedPoint;
        //    //}

        //    //private Point ScalePosition(SkeletonPoint skeletonPoint)
        //    //{
        //    //    DepthImagePoint depthPoint = new DepthImagePoint();
        //    //    if (this.sensor != null)
        //    //    {
        //    //        depthPoint = (this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30));
        //    //    }                                
        //    //    return new Point(depthPoint.X, depthPoint.Y);
        //    //}

        //}

        //Thing thing = new Thing();
        public Point getPoint { get; set; }

        public void change(int t)
        {
            //Point trailPoint = new Point();
            //trailPoint.X = getPoint.X;
            //trailPoint.Y = getPoint.Y;
            int caseSwitch = t;
            switch (caseSwitch)
            {
                case 1:
                    //this.redpolyline = new Polyline();
                    //redpolyline = new Polyline();
                    arcPath.Stroke = new SolidColorBrush(Colors.Red);
                    //redpolyline.Points.Add(new Point(trailPoint.X, trailPoint.Y));
                    //skeletonCanvas.Children.Add(redpolyline);
                    break;
                case 2:
                    //Polyline bluepolyline = new Polyline();
                    arcPath.Stroke = null;
                    arcPath2.Stroke = new SolidColorBrush(Colors.Blue);                    
                    //bluepolyline.Points.Add(new Point(trailPoint.X = getPoint.X, trailPoint.Y = getPoint.Y));
                    break;
                default:
                    Polyline defaultpolyline = new Polyline();
                    defaultpolyline.Stroke = new SolidColorBrush(Colors.Black);
                    //defaultpolyline.Points.Add(new Point(trailPoint.X = getPoint.X, trailPoint.Y = getPoint.Y));
                    break;
            }

        }

        //Polyline redpolyline;redpolyline = new Polyline();
        private Point MapJointsWithUIElement(Skeleton skeleton)
        {
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
            Canvas.SetLeft(righthand, mappedPoint.X);
            Canvas.SetTop(righthand, mappedPoint.Y);

            
            //Point trailPoint;
            double x = mappedPoint.X;
            double y = mappedPoint.Y;
            Point trailPoint = new Point();
            //Point trailPoint2 = new Point();
            line.Points.Add(new Point(trailPoint.X = x, trailPoint.Y = y));
            line2.Points.Add(new Point(trailPoint.X = x, trailPoint.Y = y));
            //redpolyline = new Polyline();
            //redpolyline.Stroke = new SolidColorBrush(Colors.Red);
            //redpolyline.Points.Add(new Point(trailPoint.X = x, trailPoint.Y = y));
            //skeletonCanvas.Children.Add(redpolyline);
            //bluepolyline.Points.Add(new Point(trailPoint2.X = x, trailPoint2.Y = y));
            //skeletonCanvas.Children.Add(bluepolyline);
            return new Point(mappedPoint.X, mappedPoint.Y);
            //int t;
            //return mappedPoint;
            //x = mappedPoint;

            //thing.p = new Pen();
            //thing.p.Thickness = 3;


            //thing.ellipse = new Polyline();
            //double x = mappedPoint.X;
            //double y = mappedPoint.Y;
            //Point trailPoint = new Point();
            //thing.ellipse.Points.Add(new Point(trailPoint.X = x, trailPoint.Y = y));

            //thing.ellipse = new Ellipse();
            //thing.ellipse.Width = 5;
            //thing.ellipse.Height = 5;
            //thing.ellipse.Fill = (SolidColorBrush)this.Resources["colorChange"];
            //thing.ellipse.SetValue(Canvas.LeftProperty, mappedPoint.X);
            //thing.ellipse.SetValue(Canvas.TopProperty, mappedPoint.Y);
            //skeletonCanvas.Children.Add(thing.ellipse);

            //Ellipse ellipse = new Ellipse()
            //{
            //    Width = 5,
            //    Height = 5,
            //    Fill = 
            //};
            //skeletonCanvas.Children.Add(ellipse);

            //double x = mappedPoint.X;
            //double y = mappedPoint.Y;
            //Point trailPoint = new Point();
            //trail.Points.Add(new Point(trailPoint.X = x, trailPoint.Y = y));
        }

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        
        private void redbutton_Click(object sender, RoutedEventArgs e)
        {
            //SolidColorBrush colorChange = (SolidColorBrush)this.Resources["colorChange"];
            //b.Color = Colors.Red;
            //colorChange.Color = Colors.Red;
            //thing.ellipse.SetValue(Shape.FillProperty, new SolidColorBrush(Colors.Red));
            //thing.change(1);

            //Polyline redpolyline = new Polyline();
            //double m = mappedPoint.X;
            //double n = mappedPoint.Y;
            //Point trailPoint = new Point();
            //redpolyline.Points.Add(new Point(trailPoint.X = m, trailPoint.Y = n));
            change(1);

        }

        private void bluebutton_Click(object sender, RoutedEventArgs e)
        {
            //SolidColorBrush colorChange = (SolidColorBrush)this.Resources["colorChange"];
            //colorChange.Color = Colors.Blue;
            change(2);
        }

        private void greenbutton_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush colorChange = (SolidColorBrush)this.Resources["colorChange"];
            colorChange.Color = Colors.Green;
        }

        private void yellowbutton_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush colorChange = (SolidColorBrush)this.Resources["colorChange"];
            colorChange.Color = Colors.Yellow;
        }

        private void clrcanvasbutton_Click(object sender, RoutedEventArgs e)
        {
            if (skeletonCanvas!=null && skeletonCanvas.Children.Count != 0)
            {
                //skeletonCanvas.Children.Remove(ellipse);
            }
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
