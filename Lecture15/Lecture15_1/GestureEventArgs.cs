using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lecture15_1
{
    class GestureEventArgs
    {
        public GestureRecognitionEngine.GestureType gsType { get; internal set; }
        public GestureRecognitionEngine.RecognitionResult Result { get; internal set; }
        public GestureEventArgs(GestureRecognitionEngine.GestureType t, GestureRecognitionEngine.RecognitionResult result)
        {
            this.Result = result;
            this.gsType = t;
        }
    }
}
