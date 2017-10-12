using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exercise.Domain.Services
{
    public class JsonService: IJsonService
    {
        public JsonService()
        {
        }

        public RaceData ParseOriginalSample() => ParseJson<RaceData>(SampleData.JsonFile);
        

        public T ParseJson<T>(string jsonText)
        {
            var raceData = JsonConvert.DeserializeObject<T>(jsonText);

            return raceData;
        }

        public Meeting Transform(RaceData raceData)
        {
            var m = raceData.meeting;

            return new Meeting
            {
                OuterId = m.id,
                Name = m.name,
                Date = m.Date,
                State = m.state,
                Races = m.races.Select(r =>
                    new Race
                    {
                        OuterId = r.id,
                        Number = r.racenumber,
                        Name = r.racename,
                        Starttime = r.starttime,
                        Endtime = r.endtime
                    }).ToList()
            };
        }
    }
}
