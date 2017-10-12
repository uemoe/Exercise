using System;

namespace Exercise.Domain
{
    public class Race
    {
        public int Id { get; set; }

        public int OuterId { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public DateTime Starttime { get; set; }

        public DateTime Endtime { get; set; }

        public Meeting Meeting { get; set; }

        public int MeetingId { get; set; }
    }
}
