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
using System.ComponentModel;
using Microsoft.Kinect;

namespace ChallangeTask1_DrawingApp2
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
        int changeColor = 0;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;

            /* SkeletonFrameReady is an event
             * public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady
             * SkeletonFrameReadyEventArgs holds data about SkeletonFrameReady event
             * It is inherited from EventArgs empty class
             * It returns skeleton image data upon success or null upon failure
             * The following statement suggests that skeletonFrameReady eventhandler method to register with 
             * SkeletonFrameReady event */
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
        }

        /* skeletonFrameReady is the SkeletonFrameReady event's eventhandler method
         * Standard naming convesion for eventhandler method:
         * the name of the object reference, followed by an underscore, followed by the name of the event
         * There is no need for this here because the event and the eventhandler method are in the same class */
        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                /* If Skeleton Frame is null return null */
                if (skeletonFrame == null)
                {
                    return;
                }

                /* get the skeletal information in this frame */
                skeletonFrame.CopySkeletonDataTo(totalSkeleton);

                /* from clause:
                 * A query expression must begin with a from clause. Additionally, a query expression can contain
                 * sub-queries, which also begin with a from clause. The from clause specifies the following:
                 * * The data source on which the query or sub-query will be run
                 * * A local range variable that represents each element in the source sequence
                 * In this case, trackskeleton is the range variable and totalSkeleton is the data source
                 * in connects trackskeleton with totalSkeleton
                 * 
                 * where clause:
                 * The where clause is used to specify constrains
                 * Here, where clause is followed by a condition
                 * 
                 * select clause:
                 * In a query expression, the select clause specifies the type of values that will be produced when
                 * the query is executed. The result is based on the evaluation of all the previous claused and on 
                 * any expression in the select clause itself. A query expression must terminate with either a select
                 * or a group clause.
                 * 
                 * This query uses System.Linq
                 * 
                 * Kinect sensor can take/track 6 skeleton data at the same time
                 * totalSkeleton holds an array of 6 skeleton
                 * Range variable trackskeleton holds the skeletondata which fulfills the below condition:
                 * select the skeleton data that is tracked irrespective of first or the default skeleton
                 * And keep the skeleton data in firstSkeleton variable */
                Skeleton firstSkeleton = (from trackskeleton in totalSkeleton where trackskeleton.TrackingState == SkeletonTrackingState.Tracked select trackskeleton).FirstOrDefault();

                /* If SkeletonFrameReadyEventArgs eventhandler returns null and firstSkeleton would also be equal to
                 * null. If firstSkeleton is equal to null then return null */
                if (firstSkeleton == null)
                {
                    return;
                }

                /* If the TrackingState of the specified joint type is tracked and trusted (Tracked),
                 * call MapJointsWithUIElements with parameter firstSkeleton (type Skeleton) */
                if (firstSkeleton.Joints[JointType.WristRight].TrackingState == JointTrackingState.Tracked)
                {
                    this.MapJointsWithUIElement(firstSkeleton);
                }
            }
        }

        /* MapJointsWithUIElement method: converts 3D skeleton coordinate (found from Position), into 2D coordinate
         * (by ScalePosition()); sets origin point of UIElement (Ellipse) to top-left (by SetLeft(), SetTop())
         * Return type:     void
         * Argument type:   Skeleton
         * 
         * Skeleton.Joints property stores the collection of joints in a skeleton. Concatenate JointType enumeration
         * to use a joint type from a listing of the different joint types
         * Position property gets the 3D coordinate of a joint type and ScalePosition method takes it as argument
         * ScalePosition returns coordinate in 2D position (coordinate for JointType.HandRight)
         * 
         * Canvas class: inherited from System.Windows.Controls.Panel
         * Defines an area within which you can explicitly position child elements by using coordinates that are relative
         * to the Canvas area
         * In this case, Ellipse (righthand) is the child element for Canvas
         * 
         * SetLeft method:  Sets the value of the Canvas.Left attached property for a given dependency object; in
         * this case, dependency object is righthand (Ellipse)
         * Return type:     void
         * Argument type:   UIElement, Double
         * Ellipse (righthand) is a children class of UIElement class, so it can be used as an UIElement type
         * mappedPoint.X property sets a double value of the Canvas.Left property provided by ScalePosition() for a 
         * given dependency object (righthand)
         * All the same for SetTop() */
        private void MapJointsWithUIElement(Skeleton skeleton)
        {
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
            Canvas.SetLeft(righthand, mappedPoint.X);
            Canvas.SetTop(righthand, mappedPoint.Y);

            Color ChangeColor;            
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 5; ellipse.Height = 5;

            switch (changeColor)
            {
                case 1:
                    ChangeColor = Colors.Red;
                    break;
                case 2:
                    ChangeColor = Colors.Blue;
                    break;
                case 3:
                    ChangeColor = Colors.Green;
                    break;
                case 4:
                    ChangeColor = Colors.Yellow;
                    break;
                default:
                    ChangeColor = Colors.Black;
                    break;
            }

            ellipse.SetValue(Canvas.LeftProperty, mappedPoint.X);
            ellipse.SetValue(Canvas.TopProperty, mappedPoint.Y);
            ellipse.Fill = new SolidColorBrush(ChangeColor);
            skeletonCanvas.Children.Add(ellipse);            
        }

        /* ScalePosition method: takes input of a 3D position (X, Y, Z) in skeleton, converts them into a single
         * point in a frame of depth data (using X, Y, Z and DepthImageFormat); this depth data (depthPoint.X, 
         * depthPoint.Y) is then represented as coordinate (X,Y) in 2D space          
         * Return type:     pubic struct Point (double, double)
         * Argument type:   public struct SkeletonPoint
         * 
         * Point:           represents an X and Y coordinate pair in two-dimensional space
         * Public properties: public double X {get;set;}, public double Y {get;set;}
         * 
         * SkeletonPoint:   contains a 3D position (or point) in skeleton space
         * Public properties: public float X {get;set;}, public float Y {get;set;}, public float Z {get;set;}
         * 
         * CoordinateMapper.MapSkeletonPointToDepthPoint method: maps a point from skeleton space to depth space
         * Return type:     public struct DepthImagePoint
         * Argument type:   public struct SkeletonPoint, public enum DepthImageFormat
         * 
         * DepthImagePoint: contains a single point in a frame of depth data
         * Public properties: public int Depth {get;set;}, public int X {get;set;}, public int Y {get;set;} */
        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /* redbutton_Click eventhandler method:
         * Changes the color property of the drawn line
         * 
         * SolicColorBrush (sealed) class:
         * Paints an area with a solid color */
        private void redbutton_Click(object sender, RoutedEventArgs e)
        {
            /* this.Resources is a type object
             * an explicit casting is necessary to equate type of both sides */
            changeColor = 1;
        }

        private void bluebutton_Click(object sender, RoutedEventArgs e)
        {
            changeColor = 2;
        }

        private void greenbutton_Click(object sender, RoutedEventArgs e)
        {
            changeColor = 3;
        }

        private void yellowbutton_Click(object sender, RoutedEventArgs e)
        {
            changeColor = 4;
        }

        /* clrcanvasbutton_Click eventhandler method:
         * Removes the polyline from canvas */
        private void clrcanvasbutton_Click(object sender, RoutedEventArgs e)
        {
            skeletonCanvas.Children.Clear();
            changeColor = 0;
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
