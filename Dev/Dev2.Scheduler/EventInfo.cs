using System;
using Dev2.Scheduler.Interfaces;
using Newtonsoft.Json;

namespace Dev2.Scheduler
{
    public class EventInfo : IEventInfo
    {
        
    
        public EventInfo(DateTime startDate, TimeSpan duration, DateTime endDate, bool success, string eventId, string failureReason)
        {
            
            EventId = eventId;
            Success = success;
            EndDate = endDate;
            Duration = duration;
            StartDate = startDate;
            FailureReason = failureReason;
        }
        [JsonConstructor]
        public EventInfo(DateTime startDate, TimeSpan duration, DateTime endDate, bool success, string eventId):this( startDate,  duration,  endDate,  success,  eventId,"")
        {

            EventId = eventId;
            Success = success;
            EndDate = endDate;
            Duration = duration;
            StartDate = startDate;
      
        }
        public DateTime StartDate { get; private set; }
        public TimeSpan Duration { get; private set; }
        public DateTime EndDate { get; private set; }
        public bool Success { get; private set; }
        public string EventId { get; private set; }
        public string FailureReason { get; private set; }
    }
}