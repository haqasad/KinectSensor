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


namespace ChallangeTask5_DrawShapeFromSpeech_1
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

        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;

        public double height = 0;
        public double width = 0;

        /* for changing shape */
        int shapeCondition = 0;
        /* for changing position of shape */
        int positionCondition = 0;

        Color objectColor;
        Shape drawObject;

        Point righthandPoint = new Point();
        Point lefthandPoint = new Point();

        Stream audioStream;
        SpeechRecognitionEngine speechEngine;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            this.sensor = KinectSensor.KinectSensors[0];

            if (this.sensor != null && !this.sensor.IsRunning)
            {
                this.sensor.Start();

                var smoothParameters = new TransformSmoothParameters
                {
                    Correction = 0.1f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.05f,
                    Prediction = 0.1f,
                    Smoothing = 0.5f
                };
                this.sensor.SkeletonStream.Enable(smoothParameters);
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                this.sensor.SkeletonFrameReady += skeletonFrameReady;

                this.sensor.ColorStream.Enable();
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                this.image1.Source = this.colorBitmap;
                this.sensor.ColorFrameReady += this.colorFrameReady;
            }

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
            //Image image1 = new Image();
            //Image.HorizontalAlignmentProperty(image1, Left);
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (null == imageFrame)
                    return;

                imageFrame.CopyPixelDataTo(colorPixels);
                int stride = imageFrame.Width * imageFrame.BytesPerPixel;

                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight), this.colorPixels, stride, 0);
                //canvas1.Children.Add(image1);
            }
        }

        void skeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            canvas1.Children.Remove(drawObject);

            if (shapeCondition == 1)
            {
                /* condition for changing shape */
                drawObject = new Ellipse();
            }
            else if (shapeCondition == 2)
            {
                drawObject = new Rectangle();
            }

            if (drawObject != null)
            {
                drawObject.Height = height;
                drawObject.Width = width;
                drawObject.Fill = new SolidColorBrush(objectColor);
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
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

                if (firstSkeleton.Joints[JointType.WristRight].TrackingState ==
                   JointTrackingState.Tracked)
                {
                    righthandPoint = this.MapRightJointsWithUIElement(firstSkeleton);
                }
                if (firstSkeleton.Joints[JointType.WristLeft].TrackingState ==
                   JointTrackingState.Tracked)
                {
                    lefthandPoint = this.MapLeftJointsWithUIElement(firstSkeleton);
                }

                if (drawObject != null)
                {
                    /* condition for changing position */
                    if (positionCondition == 1)
                    {
                        drawObject.SetValue(Canvas.LeftProperty, righthandPoint.X);
                        drawObject.SetValue(Canvas.TopProperty, righthandPoint.Y);
                    }
                    else
                    {
                        drawObject.SetValue(Canvas.LeftProperty, lefthandPoint.X);
                        drawObject.SetValue(Canvas.TopProperty, lefthandPoint.Y);
                    }
                    canvas1.Children.Add(drawObject);
                }
            }
        }

        /* method for right hand position */
        private Point MapRightJointsWithUIElement(Skeleton skeleton)
        {
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.WristRight].Position);
            return mappedPoint;
        }
        /* method for left hand position */
        private Point MapLeftJointsWithUIElement(Skeleton skeleton)
        {
            Point mappedPoint = ScalePosition(skeleton.Joints[JointType.WristLeft].Position);
            return mappedPoint;
        }

        private Point ScalePosition(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.
                      MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.
                                 Resolution640x480Fps30);
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
        /* Added new grammar: righthand, lefthand */
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
            grammarBuilder.Append(new Choices("circle", "square", "rectangle"));
            // Create Grammar from GrammarBuilder
            var handObject = new Choices();
            handObject.Add("righthand", "lefthand");
            grammarBuilder.Append(handObject);
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
        /* Setting shapeCondition and positionCondition */
        private void CommandsParser(SpeechRecognizedEventArgs e)
        {
            var result = e.Result;
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
                        shapeCondition = 1;
                        width = 30; height = 30;
                        break;
                    case "square":
                        shapeCondition = 2;
                        width = 30; height = 30;
                        break;
                    case "rectangle":
                        shapeCondition = 2;
                        width = 40; height = 20;
                        break;
                    default:
                        return;
                }

                var handString = words[3].Text;
                switch (handString)
                {
                    case "righthand":
                        positionCondition = 1;
                        break;
                    case "lefthand":
                        positionCondition = 2;
                        break;
                }
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
