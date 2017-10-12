using System;
using Newtonsoft.Json;
using System.Linq;

namespace Exercise.Domain
{   /// <summary>
    /// Add by copy-paste special from SampleData file, than correct type couple type
    /// </summary>
    public class RaceData
    {
        public MeetingJson meeting { get; set; }

        public override string ToString()
        {
            return $"Meeting name {this.meeting.name}, state is {this.meeting.state} number of races is {this.meeting.races.Length}" + ":\n" + string.Join("\n", this.meeting.races.Select(x => x.racename));
        }
    }

    public class MeetingJson
    {
        public int id { get; set; }

        public string name { get; set; }

        public string state { get; set; }

        public DateTime Date { get; set; }

        public RaceJson[] races { get; set; }
    }

    public class RaceJson
    {
        public int id { get; set; }

        public int racenumber { get; set; }

        public string racename { get; set; }

        public DateTime starttime { get; set; }

        public DateTime endtime { get; set; }
    }
}
