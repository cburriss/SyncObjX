using System;
using System.Timers;
using Microsoft.Win32;

namespace SyncObjX.Logging
{
    // http://stackoverflow.com/questions/8480063/event-for-datechange-at-midnight
    public static class MidnightNotifier
    {
        private static readonly Timer timer;

        static MidnightNotifier()
        {
            timer = new Timer(GetSleepTime());
            timer.Elapsed += (s, e) =>
            {
                OnDayChanged();
                timer.Interval = GetSleepTime();
            };
            timer.Start();

            // will not fire in Windows Service without hidden Windows Form (or starting "message pump" manually)
            // http://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents.timechanged(v=vs.110).aspx
            // http://msdn.microsoft.com/en-us/library/microsoft.win32.systemevents(v=vs.110).aspx
            SystemEvents.TimeChanged += OnSystemTimeChanged;
        }

        private static double GetSleepTime()
        {
            var midnightTonight = DateTime.Today.AddDays(1);
            var differenceInMilliseconds = (midnightTonight - DateTime.Now).TotalMilliseconds;
            return differenceInMilliseconds;
        }

        private static void OnDayChanged()
        {
            var handler = DayChanged;
            if (handler != null)
                handler(null, null);
        }

        private static void OnSystemTimeChanged(object sender, EventArgs e)
        {
            timer.Interval = GetSleepTime();
        }

        public static event EventHandler<EventArgs> DayChanged;
    }
}
