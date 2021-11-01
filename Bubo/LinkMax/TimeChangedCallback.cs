using Autodesk.Max;
using Autodesk.Max.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class TimeChangedCallback : TimeChangeCallback
    {
        static TimeChangedCallback _timeChangedCallbackInstance;
        public static TimeChangedCallback Instance
        {
            get
            {
                if (_timeChangedCallbackInstance == null)
                {
                    _timeChangedCallbackInstance = new TimeChangedCallback();
                }
                return _timeChangedCallbackInstance;
            }
        }
        public override void TimeChanged(int t)
        {
            if (LinkMax._timeChanged != null)
            {
                LinkMax._timeChanged(new TimeChangeEventArgs(t));
            }
        }

    }
    public class TimeChangeEventArgs : EventArgs
    {
        public int NewTime { get; private set; }

        public TimeChangeEventArgs(int newTime)
        {
            NewTime = newTime;
        }
    }
    public delegate void TimeChangedEvent(TimeChangeEventArgs e);
}
