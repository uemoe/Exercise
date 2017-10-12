using System;
using System.Collections.Generic;
using System.Text;

namespace Exercise.Domain
{
    public class Meeting
    {
        public int Id { get; set; }

        public int OuterId { get; set; }

        public string Name { get; set; }

        public string State { get; set; }

        public DateTime Date { get; set; }

        public List<Race> Races { get; set; }

        public Meeting()
        {
            Races = new List<Race>();
        }

        public override string ToString() => $"{Name} {State} {Date:d} {Races?.Count ?? 0}";
   }
}
