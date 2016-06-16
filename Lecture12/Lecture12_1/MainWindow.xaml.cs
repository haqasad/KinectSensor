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

namespace Lecture12_1
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
        Stream audioStream;
        SpeechRecognitionEngine speechEngine;

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.Start();

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
                canvas1.Children.Clear();
                drawObject.SetValue(Canvas.LeftProperty, 80.0);
                drawObject.SetValue(Canvas.TopProperty, 80.0);
                drawObject.Fill = new SolidColorBrush(objectColor);
                canvas1.Children.Add(drawObject);
            }

            if (words[0].Text == "close" && words[1].Text == "the" && words[2].Text == "application")
            {
                this.Close();
            }
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
