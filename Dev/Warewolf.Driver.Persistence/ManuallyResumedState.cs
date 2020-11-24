using System;
using System.Collections.Generic;
using System.Text;
using Hangfire.Common;
using Hangfire.States;

namespace Warewolf.Driver.Persistence
{
    public class ManuallyResumedState : IState
    {
        public ManuallyResumedState(string overrideValues)
        {
            this.ResumedAt = DateTime.UtcNow;
            this.OverrideValues = overrideValues;
        }

        public static readonly string StateName = "ManuallyResumed";

        public Dictionary<string, string> SerializeData() => new Dictionary<string, string>()
        {
            {
                "ManuallyResumedAt",
                JobHelper.SerializeDateTime(this.ResumedAt)
            },
            {
                "OverrideValues",
                string.Join(Environment.NewLine, this.OverrideValues)
            },
        };

        public string OverrideValues { get; }
        public DateTime ResumedAt { get; }

        public string Name => StateName;

        public string Reason => "Manually Resumed";

        public bool IsFinal => true;

        public bool IgnoreJobLoadException => false;
    }
}