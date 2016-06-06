using Microsoft.Kinect;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.ComponentModel;

/* namespace is equivalent to package in Java. Here Lecture2 is the name of the package */
namespace Lecture2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /* partial class allows a class to be split over multiple files. It is helpful while working with a large project. A partial class can have partial
     *  method. MainWindow class or partial class is inherited from the class Window. The ':' means inherited from */
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            /* InitializeComponent() method initializes form.xaml (MainWindow.xaml). It creates a link between the xaml file and C# file */
            InitializeComponent();
        }
        /* Enumerating the KinectSensor class. KinectSensor class cannot be instantiated because it is a sealed class. A sealed class cannot be 
         * instantiated. sensor is a field of type KinectSensor */
        KinectSensor sensor; 
        /* Upon initialization of the program, WindowLoaded event handler is fired. The object sender portion will be a reference to WindowLoaded event
         * handler */
        private void WindowLoaded(object sender, RoutedEventArgs e) 
        {
            /* Count the number of KinectSensors connected with the system and if the number of connected sensors are more than 0, execute the following 
             * statements */
            if(KinectSensor.KinectSensors.Count>0)
            {
                /* Select the sensor number 0 as the current operating sensor */
                this.sensor = KinectSensor.KinectSensors[0]; 
                /* sensor is a type KinectSensor. If the fields in sensor are null (the sensor is just connected) and the sensor is running then execute
                 * the following condition */
                if(this.sensor != null && !this.sensor.IsRunning)
                {
                    this.sensor.Start();
                    displayInfo();
                }
            }
            else
            {
                MessageBox.Show("No device is connected with system!");
                this.Close();
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e) // CancelEventArgs ?
        {
            if(this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }

        private void displayInfo()
        {
            //throw new NotImplementedException();
            this.textBlock1.Text = this.sensor.DeviceConnectionId;
            this.textBlock2.Text = this.sensor.ToString();
            this.textBlock3.Text = this.sensor.ElevationAngle.ToString();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.ElevationAngle = this.sensor.ElevationAngle + 1;
            displayInfo();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.sensor.ElevationAngle = this.sensor.ElevationAngle - 1;
            displayInfo();
        }
    }
}
