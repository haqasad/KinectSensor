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
using System.IO;
using System.ComponentModel;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Lecture12_2
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
        Stream audioStream;
        SpeechRecognitionEngine speechEngine;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.Start();

            this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            this.sensor.SkeletonStream.Enable();
            this.sensor.SkeletonFrameReady += this.skeletonFrameReady;

            this.sensor.ColorStream.Enable();
            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

            //this.image.Source = this.colorBitmap;
            this.sensor.ColorFrameReady += this.colorFrameReady;

            audioStream = this.sensor.AudioSource.Start();
            RecognizerInfo recognizerInfo = GetKinectRecognizer();
            if (recognizerInfo == null)
            {
                MessageBox.Show("Could not find Kinect speech recognizer");
                return;
            }

            BuildGrammarforRecognizer(recognizerInfo); // provided earlier
            statusBar.Text = "Speech Recognizer is ready";
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
            //advanceThingPosition();
            //canvas1.Children.Add(thing.Shape);

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
                    //this.MapJointsWithUIElement(skeleton);
                }
                //if (skeleton != null && this.currentSkeletonID != skeleton.TrackingId)
                //{
                //    this.currentSkeletonID = skeleton.TrackingId;
                //    int totalTrackedJoints = skeleton.Joints.Where(item => item.TrackingState == JointTrackingState.Tracked).Count();
                //    string TrackedTime = DateTime.Now.ToString("hh:mm:ss");
                //    string status = "Skeleton Id: " + this.currentSkeletonID + ", total tracked joints: " + totalTrackedJoints + ", TrackTime: " + TrackedTime + "\n";
                //    this.textBlock1.Text += status;
                //}
                DrawSkeleton(skeleton);

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
            //Canvas.SetLeft(bone, 200);
            //Canvas.SetTop(bone, 5);
            //canvas1.Children.Add(bone);
        }

        //private void MapJointsWithUIElement(Skeleton skeleton)
        //{
        //    Point mappedPoint = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
        //    //Canvas.SetLeft(righthand, mappedPoint.X);
        //    //Canvas.SetTop(righthand, mappedPoint.Y);
        //}

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }
            return null;
        }

        private void BuildGrammarforRecognizer(RecognizerInfo recognizerInfo)
        {
            var grammarBuilder = new GrammarBuilder { Culture = recognizerInfo.Culture };
            // first say Draw
            grammarBuilder.Append(new Choices("draw"));
            var colorObjects = new Choices();
            colorObjects.Add("red"); colorObjects.Add("green"); colorObjects.Add("blue");
            colorObjects.Add("yellow"); colorObjects.Add("gray");
            // New Grammar builder for color
            grammarBuilder.Append(colorObjects);
            // Another Grammar Builder for object
            grammarBuilder.Append(new Choices("circle", "square", "triangle", "rectangle"));

            var jointtype = new Choices();
            jointtype.Add("righthand"); jointtype.Add("lefthand"); jointtype.Add("rightfoot"); jointtype.Add("leftfoot");
            grammarBuilder.Append(jointtype);

            // Create Grammar from GrammarBuilder
            var grammar = new Grammar(grammarBuilder);

            // Creating another Grammar and load
            var newGrammarBuilder = new GrammarBuilder();
            newGrammarBuilder.Append("close the application");
            var grammarClose = new Grammar(newGrammarBuilder);
            speechEngine = new SpeechRecognitionEngine(recognizerInfo.Id);
            speechEngine.LoadGrammar(grammar); // loading grammer into recognizer
            speechEngine.LoadGrammar(grammarClose);

            // Attach the speech audio source to the recognizer
            int SamplesPerSecond = 16000; int bitsPerSample = 16;
            int channels = 1; int averageBytesPerSecond = 32000; int blockAlign = 2;
            speechEngine.SetInputToAudioStream(
                 audioStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm,
                 SamplesPerSecond, bitsPerSample, channels, averageBytesPerSecond,
                  blockAlign, null));

            // Register the event handler for speech recognition
            speechEngine.SpeechRecognized += speechRecognized;
            speechEngine.SpeechHypothesized += speechHypothesized;
            speechEngine.SpeechRecognitionRejected += speechRecognitionRejected;

            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void speechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e) { }

        private void speechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            wordsTenative.Text = e.Result.Text;
        }

        private void speechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            wordsRecognized.Text = e.Result.Text;
            confidenceTxt.Text = e.Result.Confidence.ToString();
            float confidenceThreshold = 0.6f;
            if (e.Result.Confidence > confidenceThreshold)
            {
                CommandsParser(e);
            }
        }

        private void CommandsParser(SpeechRecognizedEventArgs e)
        {
            var result = e.Result;
            Color objectColor;
            Shape drawObject;
            System.Collections.ObjectModel.ReadOnlyCollection<RecognizedWordUnit> words = e.Result.Words;

            if (words[0].Text == "draw")
            {
                string colorObject = words[1].Text;
                switch (colorObject)
                {
                    case "red":
                        objectColor = Colors.Red;
                        break;
                    case "green":
                        objectColor = Colors.Green;
                        break;
                    case "blue":
                        objectColor = Colors.Blue;
                        break;
                    case "yellow":
                        objectColor = Colors.Yellow;
                        break;
                    case "gray":
                        objectColor = Colors.Gray;
                        break;
                    default:
                        return;
                }
                var shapeString = words[2].Text;
                switch (shapeString)
                {
                    case "circle":
                        drawObject = new Ellipse();
                        drawObject.Width = 100; drawObject.Height = 100;
                        break;
                    case "square":
                        drawObject = new Rectangle();
                        drawObject.Width = 100; drawObject.Height = 100;
                        break;
                    case "rectangle":
                        drawObject = new Rectangle();
                        drawObject.Width = 100; drawObject.Height = 60;
                        break;
                    case "triangle":
                        var polygon = new Polygon();
                        polygon.Points.Add(new Point(0, 30));
                        polygon.Points.Add(new Point(-60, -30));
                        polygon.Points.Add(new Point(60, -30));
                        drawObject = polygon;
                        break;
                    default:
                        return;
                }
                var jointtype = words[3].Text;
                switch (jointtype)
                {
                    case "righthand":
                        Point mappedPoint = new Point();
                        if (skeleton != null)
                        {
                            mappedPoint = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
                        }
                        //mappedPoint = ScalePosition(skeleton.Joints[JointType.HandRight].Position);
                        canvas1.Children.Clear();
                        drawObject.SetValue(Canvas.LeftProperty, mappedPoint.X - 15);
                        drawObject.SetValue(Canvas.TopProperty, mappedPoint.Y - 15);
                        drawObject.Fill = new SolidColorBrush(objectColor);
                        canvas1.Children.Add(drawObject);
                        break;
                    case "lefthand":
                        Point mappedPoint1 = ScalePosition(skeleton.Joints[JointType.HandLeft].Position);
                        canvas1.Children.Clear();
                        drawObject.SetValue(Canvas.LeftProperty, mappedPoint1.X);
                        drawObject.SetValue(Canvas.TopProperty, mappedPoint1.Y);
                        drawObject.Fill = new SolidColorBrush(objectColor);
                        canvas1.Children.Add(drawObject);
                        break;
                }
                //canvas1.Children.Clear();
                //drawObject.SetValue(Canvas.LeftProperty, 80.0);
                //drawObject.SetValue(Canvas.TopProperty, 80.0);
                //drawObject.Fill = new SolidColorBrush(objectColor);
                //canvas1.Children.Add(drawObject);
            }

            if (words[0].Text == "close" && words[1].Text == "the" && words[2].Text == "application")
            {
                this.Close();
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
