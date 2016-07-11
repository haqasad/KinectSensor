using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace Lecture16_1
{
    public class SwipeToLeftGesture : GestureBase
    {
        // intermediate right hand position used for validation of the gesture
        private SkeletonPoint validatePosition;

        // starting right hand position when the gesture start condition is met (starting pose)
        private SkeletonPoint startingPostion;

        // distance between the hand right and the left shoulder
        private float shoulderDiff;

        // constructor 
        public SwipeToLeftGesture() : base(GestureType.SwipeToLeft) { }

        // check to see if the starting pose is seen
        // called for every skeleton frame received
        //protected override bool ValidateGestureStartCondition(Skeleton skeleton) { }

        protected override bool ValidateGestureStartCondition(Skeleton skeleton)
        {
            var handRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var spinePosition = skeleton.Joints[JointType.Spine].Position;

            // Starting pose:
            // right hand lower than right shoulder && right hand higher than right elbow
            // && left hand lower than spine
            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
                    (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
                        && handLeftPosition.Y < spinePosition.Y)
            {
                shoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderLeft]);
                validatePosition = skeleton.Joints[JointType.HandRight].Position;
                startingPostion = skeleton.Joints[JointType.HandRight].Position;
                return true;
            }
            return false;
        }
        // called for every skeleton frame
        protected override bool IsGestureValid(Skeleton skeletonData)
        {
            // current right hand position
            var currentHandRightPoisition = skeletonData.Joints[JointType.HandRight].Position;
            // current right hand should be on the left of the previous right hand position, 
            // i.e., the right hand is moving to the left
            if (validatePosition.X < currentHandRightPoisition.X)
            {
                // if the right hand is moving to the right, stop doing gesture recognition
                return false;
            }
            // update the validatePosition using the current right hand position
            validatePosition = currentHandRightPoisition;

            // gesture so far so good
            return true;
        }
        // check if the final pose has reached
        protected override bool ValidateGestureEndCondition(Skeleton skeleton)
        {
            // distance between the staring right hand position and 
            // the last right hand position
            double distance = Math.Abs(startingPostion.X - validatePosition.X);
            // the distance between the current right hand and the left shoulder
            float currentshoulderDiff = GestureHelper.GetJointDistance(skeleton.Joints[JointType.HandRight], skeleton.Joints[JointType.ShoulderLeft]);

            // the right hand has moved for 0.1m since its starting position and
            // the right hand is getting closer to the left shoulder => we are done!
            if (distance > 0.1 && currentshoulderDiff < shoulderDiff)
                return true;

            // otherwise, the right hand has not moved enough distance yet
            return false;
        }
        protected override bool ValidateBaseCondition(Skeleton skeleton)
        {
            var handRightPoisition = skeleton.Joints[JointType.HandRight].Position;
            var handLeftPosition = skeleton.Joints[JointType.HandLeft].Position;
            var shoulderRightPosition = skeleton.Joints[JointType.ShoulderRight].Position;
            var spinePosition = skeleton.Joints[JointType.Spine].Position;

            // right hand is to the left of the right shoulder, and
            // right hand is higher than right elbow, and
            // left hand is lower than spine
            if ((handRightPoisition.Y < shoulderRightPosition.Y) &&
               (handRightPoisition.Y > skeleton.Joints[JointType.ElbowRight].Position.Y)
               && (handLeftPosition.Y < spinePosition.Y))
            {
                // swipe to the left is ongoing, so far so good
                return true;
            }
            // condition is not met, terminate
            return false;
        }

    }
}
