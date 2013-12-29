using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace  IdentifiedObjects
{
    //Contains all of the types used to identify time points, spans, and intervals 
    /// <summary>
    /// Represents a single time interval in a regular schedule
    /// </summary>
    struct RegularTimePoint
    {
        //gives the particular time point 
        public int SequenceNumber { get; set; }
        public DateTime TimeValue{ get{ return timeValue; } }
        /// <summary>
        /// Stored locally after first running getTimeValue
        /// </summary>
        private DateTime timeValue;
        private DateTime getTimeValue()
        {
            return schedule.StartTime.AddTicks(schedule.IntervalDuration.Ticks * this.SequenceNumber);
        }
        private RegularIntervalSchedule schedule;
        public RegularIntervalSchedule Schedule { get { return schedule; } }
    }
    /// <summary>
    /// Base Class of all time schedules
    /// </summary>
    public abstract class BasicIntervalSchedule
    {
        private DateTime startTime;
        public DateTime StartTime { get { return startTime; } }      

    }


    /// <summary>
    /// Represents a time schedule where all of the intervals are the same length
    /// </summary>
    public class RegularIntervalSchedule : BasicIntervalSchedule
    {
        private TimeSpan intervalDuration;
        public TimeSpan IntervalDuration { get { return intervalDuration; } }
        
    }

   
}
