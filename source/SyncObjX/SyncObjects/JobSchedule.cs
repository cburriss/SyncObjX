using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.SyncObjects
{
    [DataContract]
    public class JobSchedule : SyncObject
    {

        public static int MinimumAllowedFrequencyInSeconds = 15;

        public static readonly TimeSpan MinimumAllowedStartTime = new TimeSpan(0, 0, 0);

        public static readonly TimeSpan MaximumAllowedEndTime = new TimeSpan(23, 59, 59);


        public static readonly bool IsEnabledDefault = true;

        public static readonly DateTime StartDateDefault = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        public static readonly DateTime EndDateDefault = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day);

        public static readonly TimeSpan StartTimeDefault = MinimumAllowedStartTime;

        public static readonly TimeSpan EndTimeDefault = MaximumAllowedEndTime;

        public static readonly HashSet<DayOfWeek> DaysOfWeekDefault 
            = new HashSet<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

        public const int FrequencyInSecondsDefault = 60 * 60; // hourly


        private bool _isEnabled = IsEnabledDefault;

        private DateTime _startDate = StartDateDefault;

        private DateTime _endDate = EndDateDefault;

        private TimeSpan _startTime = StartTimeDefault;

        private TimeSpan _endTime = EndTimeDefault;

        private HashSet<DayOfWeek> _daysOfWeek = DaysOfWeekDefault;

        private int _frequencyInSeconds = FrequencyInSecondsDefault;


        [DataMember]
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }

        [DataMember]
        public DateTime StartDate
        {
            get { return _startDate; }

            set
            {
                if(value > EndDate)
                    throw new Exception("Scheduled start date cannot be later than scheduled end date.");
                else
                    _startDate = new DateTime(value.Year, value.Month, value.Day);
            }
        }

        [DataMember]
        public DateTime EndDate
        {
            get { return _endDate; }

            set
            {
                if(value < StartDate)
                    throw new Exception("Scheduled start date cannot be later than scheduled end date.");
                else
                    _endDate = new DateTime(value.Year, value.Month, value.Day);
            }
        }

        [DataMember]
        public TimeSpan StartTime
        {
            get { return _startTime; }

            set
            {
                if(value < MinimumAllowedStartTime)
                    throw new Exception(string.Format("The scheduled start time cannot be less than {0}.", MinimumAllowedStartTime.ToString()));
                else if(value > _endTime)
                    throw new Exception("Scheduled start time cannot be later than scheduled end time.");
                else
                    _startTime = value;
            }
        }

        [DataMember]
        public TimeSpan EndTime
        {
            get { return _endTime; }

            set
            {
                if (value > MaximumAllowedEndTime)
                    throw new Exception(string.Format("The scheduled end time cannot be greater than {0}.", MaximumAllowedEndTime.ToString()));
                else if (value < _startTime)
                    throw new Exception("Scheduled start time cannot be later than scheduled end time.");
                else
                    _endTime = value;
            }
        }

        [DataMember]
        public HashSet<DayOfWeek> DaysOfWeek
        {
            get { return _daysOfWeek; }

            set 
            {
                if (value == null || value.Count == 0)
                    throw new Exception("At least one day of week must be specified for the sync schedule.");
                else
                    _daysOfWeek = value; 
            }
        }

        [DataMember]
        public string DaysOfWeekAsString
        {
            get { return GetDaysOfWeekAsString(DaysOfWeek); }

            private set { }
        }

        [DataMember]
        public int FrequencyInSeconds
        {
            get { return _frequencyInSeconds; }

            set 
            {
                if (value < MinimumAllowedFrequencyInSeconds)
                    throw new Exception(string.Format("Scheduled frequency cannot be less than {0} seconds.", MinimumAllowedFrequencyInSeconds));
                else
                    _frequencyInSeconds = value;
            }
        }

        public JobSchedule(Guid jobScheduleId, DateTime startDate, DateTime endDate, int frequencyInSeconds)
            : this(jobScheduleId, startDate, endDate, DaysOfWeekDefault, StartTimeDefault, EndTimeDefault, frequencyInSeconds) { }

        public JobSchedule(Guid jobScheduleId, DateTime startDate, DateTime endDate, HashSet<DayOfWeek> daysOfWeek, int frequencyInSeconds)
            : this(jobScheduleId, startDate, endDate, daysOfWeek, StartTimeDefault, EndTimeDefault, frequencyInSeconds) { }

        public JobSchedule(Guid jobScheduleId, DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan endTime, int frequencyInSeconds)
            : this(jobScheduleId, startDate, endDate, DaysOfWeekDefault, startTime, endTime, frequencyInSeconds) { }

        public JobSchedule(Guid jobScheduleId, DateTime startDate, DateTime endDate, HashSet<DayOfWeek> daysOfWeek, TimeSpan startTime, TimeSpan endTime, int frequencyInSeconds)
        {
            if (jobScheduleId == Guid.Empty)
                throw new Exception("Job schedule ID must be a valid GUID.");

            Id = jobScheduleId;

            StartDate = startDate;

            EndDate = endDate;

            DaysOfWeek = daysOfWeek;

            StartTime = startTime;

            EndTime = endTime;

            FrequencyInSeconds = frequencyInSeconds;
        }

        public static string GetDaysOfWeekAsString(HashSet<DayOfWeek> daysOfWeek)
        {
            if (daysOfWeek == null || daysOfWeek.Count == 0)
                return "";

            string daysOfWeekAsString = "";

            if (daysOfWeek.Contains(DayOfWeek.Monday))
                daysOfWeekAsString += "M,";

            if (daysOfWeek.Contains(DayOfWeek.Tuesday))
                daysOfWeekAsString += "T,";

            if (daysOfWeek.Contains(DayOfWeek.Wednesday))
                daysOfWeekAsString += "W,";

            if (daysOfWeek.Contains(DayOfWeek.Thursday))
                daysOfWeekAsString += "H,";

            if (daysOfWeek.Contains(DayOfWeek.Friday))
                daysOfWeekAsString += "F,";

            if (daysOfWeek.Contains(DayOfWeek.Saturday))
                daysOfWeekAsString += "S,";

            if (daysOfWeek.Contains(DayOfWeek.Sunday))
                daysOfWeekAsString += "U,";

            if (daysOfWeekAsString.LastIndexOf(",") == (daysOfWeekAsString.Length - 1))
                return daysOfWeekAsString.Remove(daysOfWeekAsString.Length - 1);
            else
                return daysOfWeekAsString;
        }

        public static HashSet<DayOfWeek> GetDaysOfWeekFromString(string daysOfWeek)
        {
            HashSet<DayOfWeek> daysOfWeekHash = new HashSet<DayOfWeek>();

            if (daysOfWeek == null || daysOfWeek.Trim() == string.Empty)
                return daysOfWeekHash;

            var splitDaysOfWeek = daysOfWeek.Split(',');

            foreach (var dayCode in splitDaysOfWeek)
            {
                switch(dayCode)
                {
                    case "M":
                        daysOfWeekHash.Add(DayOfWeek.Monday);
                        break;

                    case "T":
                        daysOfWeekHash.Add(DayOfWeek.Tuesday);
                        break;

                    case "W":
                        daysOfWeekHash.Add(DayOfWeek.Wednesday);
                        break;

                    case "H":
                        daysOfWeekHash.Add(DayOfWeek.Thursday);
                        break;

                    case "F":
                        daysOfWeekHash.Add(DayOfWeek.Friday);
                        break;

                    case "S":
                        daysOfWeekHash.Add(DayOfWeek.Saturday);
                        break;

                    case "U":
                        daysOfWeekHash.Add(DayOfWeek.Sunday);
                        break;

                    default:
                        throw new Exception(string.Format("Day of week could not be determined from '{0}'.", dayCode));
                }
            }

            return daysOfWeekHash;
        }

        public DateTime? GetNextRunTime()
        {
            DateTime? nextRunTime = null;
            
            var now = DateTime.Now;

            // the schedule is no longer valid because of the end date
            if (now.Date > EndDate)
                return null;

            // if the 1st run time is in the future, return the start date and time
            if (now.Date < StartDate)
                return new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);

            var runDate = now.Date;

            while (runDate <= EndDate)
            {
                var runTimes = GetRunTimes(runDate, excludePastRunTimes: true);

                if (runTimes.Count > 0)
                {
                    nextRunTime = new DateTime(runDate.Year, runDate.Month, runDate.Day, runTimes[0].Hours, runTimes[0].Minutes, runTimes[0].Seconds);
                    break;
                }

                runDate = runDate.AddDays(1);
            }

            return nextRunTime;
        }

        public List<TimeSpan> GetRunTimes(DateTime runDate, bool excludePastRunTimes = true)
        {
            List<TimeSpan> runTimesForDay = new List<TimeSpan>();

            var now = DateTime.Now;

            if (excludePastRunTimes && runDate.Date < now.Date)
                return runTimesForDay;

            if (runDate.Date > EndDate || runDate.Date < StartDate || !DaysOfWeek.Contains(runDate.DayOfWeek))
                return runTimesForDay;

            DateTime nextRunTime = new DateTime(runDate.Year, runDate.Month, runDate.Day, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);
            DateTime lastRunTime = new DateTime(runDate.Year, runDate.Month, runDate.Day, EndTime.Hours, EndTime.Minutes, EndTime.Seconds);

            var runDateIsToday = runDate.Date == now.Date;

            while (nextRunTime <= lastRunTime)
            {
                if (!excludePastRunTimes || runDate.Date > now.Date || (runDateIsToday && nextRunTime >= now))
                    runTimesForDay.Add(nextRunTime.TimeOfDay);

                nextRunTime = nextRunTime.AddSeconds(FrequencyInSeconds);
            }

            return runTimesForDay;
        }
    }
}
