using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Lecture16_1
{
    public enum GestureType
    {
        SwipeToRight,
        SwipeToLeft,
        ZoomIn,
        ZoomOut
    }

    public enum RecognitionResult
    {
        Unknown,
        Failed,
        Success
    }

    class GestureRecognitionEngine
    {
        
        int SkipFramesAfterGestureIsDetected = 0;
        public event EventHandler<GestureEventArgs> GestureRecognized;
        public GestureType gestureType { get; set; }
        public Skeleton Skeleton { get; set; }
        public bool IsGestureDetected { get; set; }
        // list of gestures to be detected
        private List<GestureBase> gestureCollection = null;
        public GestureRecognitionEngine()
        {
            this.InitilizeGesture();
        }
        private void InitilizeGesture()
        {
            this.gestureCollection = new List<GestureBase>();
            //this.gestureCollection.Add(new ZoomInGesture());
            //this.gestureCollection.Add(new ZoomOutGesture());
            //this.gestureCollection.Add(new SwipeToRightGesture());
            // add SwipeToLeftGesture recognizer to the list
            this.gestureCollection.Add(new SwipeToLeftGesture());
        }

        // reset data structures for a new round of gesture recognition
        private void RestGesture()
        {
            this.gestureCollection = null;
            this.InitilizeGesture();
            this.SkipFramesAfterGestureIsDetected = 0;
            this.IsGestureDetected = false;
        }
        public void StartRecognize()
        {
            if (this.IsGestureDetected)
            {
                // create a short break when we are done one round of gesture recognition
                while (this.SkipFramesAfterGestureIsDetected <= 30)
                {
                    this.SkipFramesAfterGestureIsDetected++;
                }
                // reset our data structures for a new round of gesture recognition
                this.RestGesture();
                return;
            }
            // perform gesture recognition for every gesture recognizer in our list
            foreach (var item in this.gestureCollection)
            {
                if (item.CheckForGesture(this.Skeleton))
                {
                    if (this.GestureRecognized != null)
                    {
                        // fire a gesture event when a gesture is recognized
                        this.GestureRecognized(this, new GestureEventArgs(RecognitionResult.Success, item.GestureType));
                        this.IsGestureDetected = true;
                    }
                }
            }
        }
    }
}
