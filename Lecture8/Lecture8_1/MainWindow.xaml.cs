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

namespace Lecture8_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /* The event model in the .NET Framework is based on having an event delegate that connects an event with
         * its handler. To raise an event, two elements are needed:
         * A delegate that identifies the method that provides the response to the event
         * Optionally, a class that holds the event data, if the event provides data
         * 
         * The delegate is a type that defines a signature, that is, the return valuer type and parameter list types
         * for a method. You can use the delegate type to declare a variable that can refer to any method with the
         * same signature as the delegate
         * 
         * The standard signature of an event handler delegate defines a method that does not return a value. This 
         * method's first parameter is of type Object and refers to the instance that raises the event. Its second 
         * parameter is derived from type EventArgs and holds the event data. If the event does not generate event
         * data, the second parameter is simply the value of the EventArgs.Empty field. Otherwise, the second parameter
         * is a type derived from EventArgs and supplies any fields or properties needed to hold the event data.
         * 
         * The EventHandler delegate is a predefined delegate that specifically represents an event handler method
         * for an event that does not generate data. If your event does generate data, you must use the generic
         * EventHandler<TEventArgs> delegate class.
         * 
         * To associate the event with the method that will handle the event, add an instance of the delegate to the
         * event. The event handler is called whenever the event occurs, unless you remove the delegate */
        public MainWindow()
        {
            InitializeComponent();
        }

        KinectSensor sensor;
        Skeleton[] totalSkeleton = new Skeleton[6];

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            this.sensor.SkeletonStream.Enable();

            /* SkeletonFrameReady is an event
             * public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady
             * SkeletonFrameReadyEventArgs holds data about SkeletonFrameReady event
             * It is inherited from EventArgs empty class
             * It returns skeleton image data upon success or null upon failure
             * The following statement suggests that skeletonFrameReady eventhandler method to register with 
             * SkeletonFrameReady event */
            this.sensor.SkeletonFrameReady += skeletonFrameReady;
            this.sensor.Start();
        }

        /* skeletonFrameReady is the SkeletonFrameReady event's eventhandler method
         * Standard naming convesion for eventhandler method:
         * the name of the object reference, followed by an underscore, followed by the name of the event
         * There is no need for this here because the event and the eventhandler method are in the same class */
        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            /* Opens a SkeletonFrame object, which contains one frame of skeleton data */
            using(SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
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
                if (firstSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
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
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
            Canvas.SetLeft(righthand, mappedPoint.X);
            Canvas.SetTop(righthand, mappedPoint.Y);
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
        
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if(this.sensor!=null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }        
    }
}
