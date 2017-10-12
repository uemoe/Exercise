using Exercise.Domain;
using Exercise.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Exercise.Tests
{
    public class JsonServiceTest
    {
        private IJsonService _jsonService = new JsonService();

        [Fact] //(Skip = "Because time consuming")
        public void CanParseJsonSampleData()
        {
            var raceData = _jsonService.ParseOriginalSample();

            raceData.meeting.name += "HuHu";
            var meeting = _jsonService.Transform(raceData);

            Assert.Equal("CanterburryHuHu", meeting.Name);
            Assert.Equal(3, meeting.Races.Count);
        }
    }
}
