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
using System.Threading;
using System.ComponentModel;
using Microsoft.Kinect;

namespace Lecture11_1
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
        WriteableBitmap colorBitmap;
        byte[] colorPixels;
        // Skeleton[] totalSkeleton = new Skeleton[6];
        Stream audioStream;
        string wavfilename = "C:\\Users\\haqasad\\Source\\Repos\\KinectSensor\\Lecture11\\Lecture11_1\\bin\\Debug\\kinectAudio.wav";

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.Enable();
            this.sensor.ColorStream.Enable();
            this.sensor.AllFramesReady += allFramesReady;

            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.image1.Source = this.colorBitmap;

            this.sensor.AudioSource.SoundSourceAngleChanged += soundSourceAngleChanged;
            this.sensor.AudioSource.BeamAngleChanged += beamAngleChanged;

            // start the sensor.
            this.sensor.Start();
        }

        private void startAudioStreamBtn_Click(object sender, RoutedEventArgs e)
        {
            audioStream = this.sensor.AudioSource.Start();
        }

        private void stopAudioStreamBtn_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.Stop();
        }

        void beamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            this.soundBeamAngle.Text = e.Angle.ToString();
        }

        void soundSourceAngleChanged(object sender, SoundSourceAngleChangedEventArgs e)
        {
            this.soundSourceAngle.Text = e.Angle.ToString();
            this.confidenceLevel.Text = e.ConfidenceLevel.ToString();
        }

        void allFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (null == imageFrame)
                    return;
                imageFrame.CopyPixelDataTo(colorPixels);
                int stride = imageFrame.Width * imageFrame.BytesPerPixel;

                this.colorBitmap.WritePixels(
          new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight), this.colorPixels, stride, 0);
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            var audioThread = new Thread(new ThreadStart(RecordAudio));
            audioThread.SetApartmentState(ApartmentState.MTA);
            audioThread.Start();

        }

        public void RecordAudio()
        {
            int recordingLength = (int)10 * 2 * 16000;
            byte[] buffer = new byte[1024];
            Boolean startAudioStreamHere = false;
            using (FileStream fileStream = new FileStream(wavfilename, FileMode.Create))
            {
                WriteWavHeader(fileStream, recordingLength);
                if (audioStream == null)
                {
                    startAudioStreamHere = true;
                    audioStream = this.sensor.AudioSource.Start();
                }
                int count, totalCount = 0;
                while ((count = audioStream.Read(buffer, 0, buffer.Length)) > 0 && totalCount < recordingLength)
                {
                    fileStream.Write(buffer, 0, count); totalCount += count;
                }
                if (startAudioStreamHere == true)
                    this.sensor.AudioSource.Stop();
            }
        }

        static void WriteWavHeader(Stream stream, int dataLength)
        {
            using (var memStream = new MemoryStream(64))
            {
                int cbFormat = 18; //sizeof(WAVEFORMATEX)
                WAVEFORMATEX format = new WAVEFORMATEX()
                {
                    wFormatTag = 1,
                    nChannels = 1,
                    nSamplesPerSec = 16000,
                    nAvgBytesPerSec = 32000,
                    nBlockAlign = 2,
                    wBitsPerSample = 16,
                    cbSize = 0
                };
                using (var binarywriter = new BinaryWriter(memStream))
                {
                    //RIFF header
                    WriteString(memStream, "RIFF");
                    binarywriter.Write(dataLength + cbFormat + 4);
                    WriteString(memStream, "WAVE");
                    WriteString(memStream, "fmt ");
                    binarywriter.Write(cbFormat);
                    //WAVEFORMATEX
                    binarywriter.Write(format.wFormatTag);
                    binarywriter.Write(format.nChannels);
                    binarywriter.Write(format.nSamplesPerSec);
                    binarywriter.Write(format.nAvgBytesPerSec);
                    binarywriter.Write(format.nBlockAlign);
                    binarywriter.Write(format.wBitsPerSample);
                    binarywriter.Write(format.cbSize);
                    //data header
                    WriteString(memStream, "data");
                    binarywriter.Write(dataLength);
                    memStream.WriteTo(stream);
                }
            }
        }

        struct WAVEFORMATEX
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
            public ushort cbSize;
        }

        static void WriteString(Stream stream, string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(wavfilename) && File.Exists(wavfilename))
            {
                kinectaudioPlayer.Source = new Uri(wavfilename, UriKind.RelativeOrAbsolute);
                kinectaudioPlayer.LoadedBehavior = MediaState.Play;
                kinectaudioPlayer.UnloadedBehavior = MediaState.Close;
            }
        }

        private void noiseSupression_Checked(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.NoiseSuppression = true;
        }

        private void echoCancellation_Checked(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.EchoCancellationMode = EchoCancellationMode.CancellationOnly;
            this.sensor.AudioSource.EchoCancellationSpeakerIndex = 0;
        }

        private void gainControl_Checked(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.AutomaticGainControlEnabled = true;
        }

        private void noiseSupression_Unchecked(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.NoiseSuppression = false;
        }

        private void echoCancellation_Unchecked(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.EchoCancellationMode = EchoCancellationMode.None;            
        }

        private void gainControl_Unchecked(object sender, RoutedEventArgs e)
        {
            this.sensor.AudioSource.AutomaticGainControlEnabled = false;
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
