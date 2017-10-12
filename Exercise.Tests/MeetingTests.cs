using Exercise.Data;
using Exercise.Domain;
using Exercise.Domain.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Exercise.Data.BusinessLogic;

namespace Exercise.Tests
{
    public class MeetingTests
    {
        DateTime dat1 = new DateTime(2017, 10, 10, 17, 10, 0, 0);
        private IJsonService _jsonService = new JsonService();

        // Poor man's DI and strategy pattern
        //private IRepository repository = new SaveWithDelete();
        private ISaveStrategy strategy = new SaveWithUpdate();

        private static DbContextOptions inMemoryTestDb = new DbContextOptionsBuilder()
                                                                    .UseInMemoryDatabase("Test").Options;

        private static DbContextOptions sqlServerTestDb = new DbContextOptionsBuilder()
            .UseSqlServer
              ("Server = (localdb)\\mssqllocaldb; Database = TestDb; Trusted_Connection = True; ").Options;

        // Test broken because  InMemory provider don't check referential integrity
        [Theory]
        [MemberData(nameof(GetThreeTestDataBases))]
        public void ShouldBeBrokenInMemoryProviderFK(string caseName, DbContextOptions options)
        {
            var meeting = GetOriginalMeetingfromJson();

            using (var context = new MeetingContext(options))
            {
                ClearTestDb(context);

                strategy.SaveMeeting(context, meeting);
                context.SaveChanges();

                Assert.True(meeting.Id > 0);

                var race = meeting.Races.First();
                context.Attach(race);
                race.MeetingId = 5000;
                var ex = Record.Exception(() => context.SaveChanges());
                Assert.IsType<DbUpdateException>(ex);
                Assert.Contains("FOREIGN KEY", ex.InnerException.Message);
            }
        }

        [Theory]
        [MemberData(nameof(GetTwoTestDataBases))]
        public void CanInsertMeetingIntoDb(string caseName, DbContextOptions options)
        {
            using (var context = new MeetingContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var meeting = CreateTestMeeting("Hahahaha");
                Debug.WriteLine($"Default Samurai Id {meeting.Id}");
                context.Meetings.Add(meeting);
                var efDefaultId = meeting.Id;
                Debug.WriteLine($"EF Default Samurai Id {efDefaultId}");
                context.SaveChanges();
                Debug.WriteLine($"DB assigned Samurai Id {meeting.Id}");
                Assert.NotEqual(efDefaultId, meeting.Id);
            }
        }

        [Theory]
        [MemberData(nameof(GetTwoTestDataBases))]
        public void CanInsertMeetingWithSaveChanges(string caseName, DbContextOptions options)
        {
            using (var context = new MeetingContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var meeting = new Meeting
                {
                    Name = "Be-Be-Be"
                };
                context.Meetings.Add(meeting);
                context.SaveChanges();
            }
            using (var context2 = new MeetingContext(options))
            {
                Assert.Equal(1, context2.Meetings.Count());
            }
        }

        [Theory]
        [MemberData(nameof(GetTwoTestDataBases))]
        public void SaveOriginalFile(string caseName, DbContextOptions options)
        {
            var meeting = GetOriginalMeetingfromJson();

            using (var context = new MeetingContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                strategy.SaveMeeting(context, meeting);
                context.SaveChanges();
            }

            using (var context = new MeetingContext(options))
            {
                var insertedMeeting = context.Meetings.Find(meeting.Id);
                context.Entry(insertedMeeting)
                    .Collection(b => b.Races)
                    .Load();

                Assert.Equal(3, insertedMeeting.Races.Count());
                var trackingEntities = context.ChangeTracker.Entries<Race>().Count();
                Assert.Equal(3, trackingEntities);
                Assert.Equal(insertedMeeting.Id, insertedMeeting.Races.First().MeetingId);
            }
        }

        [Theory]
        [MemberData(nameof(GetTwoTestDataBases))]
        public void NaiveScenarioSaveAndUpdateSourceAndSaveAgain(string caseName, DbContextOptions options)
        {
            var meeting = GetOriginalMeetingfromJson();

            using (var context = new MeetingContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                strategy.SaveMeeting(context, meeting);
                context.SaveChanges();
            }

            // Another JsonFile with additional race
            meeting = GetOriginalMeetingfromJson();
            Race newRace = CreateNewRace();

            // Apply changes to first to emulate second file
            meeting.Date = meeting.Date.AddDays(2);
            var RaceId = meeting.Races[0].OuterId;
            meeting.Races[0].Name = "DDDD";
            meeting.Races.Add(newRace);

            using (var context = new MeetingContext(options))
            {
                var trackingEntities = context.ChangeTracker.Entries().Count();

                Assert.Equal(0, trackingEntities);

                var insertedMeeting = context.Meetings.AsNoTracking()
                    .Include(b => b.Races).First(m => m.OuterId == meeting.OuterId);

                Debug.WriteLine($"Meeting new graph {meeting.Id} {meeting.OuterId}");

                strategy.SaveMeeting(context, meeting);

                context.SaveChanges();
            }

            using (var context = new MeetingContext(options))
            {
                var trackingEntities = context.ChangeTracker.Entries().Count();
                Assert.Equal(0, trackingEntities);

                var insertedMeeting = context.Meetings.AsNoTracking()
                                    .Include(b => b.Races).First(m => m.OuterId == meeting.OuterId);

                Assert.Equal(4, insertedMeeting.Races.Count());
                Assert.Equal("DDDD", insertedMeeting.Races.First(l => l.OuterId == RaceId).Name);
                trackingEntities = context.ChangeTracker.Entries().Count();
                Assert.Equal(0, trackingEntities);
            }
        }

        private Race CreateNewRace(string name = "BBB", int outerId = 100)
        {
            return new Race { Name = name, Number = 2, OuterId = outerId, Starttime = dat1, Endtime = dat1.AddMinutes(20) };
        }

        private Meeting GetOriginalMeetingfromJson()
        {
            var raceData = _jsonService.ParseOriginalSample();

            var meeting = _jsonService.Transform(raceData);

            return meeting;
        }

        private static Meeting CreateNewTestMeetingWithId(string name)
        {
            return new Meeting { Id = 1, Name = name, State = "NSW", Date = new DateTime(2017, 09, 23) };
        }

        private static Meeting CreateTestMeeting(string name)
        {
            return new Meeting { Name = name, State = "NSW", Date = new DateTime(2017, 09, 23) };
        }

        private static DbContextOptions CreateSQLiteOptions()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            var builder = new DbContextOptionsBuilder().
                UseSqlite(connection);
            return builder.Options;
        }

        // List of DataBase
        public static IEnumerable<object[]> GetTwoTestDataBases()
        {
            yield return new object[] { "InMemory", inMemoryTestDb };

            yield return new object[] { "SQL Server", sqlServerTestDb };

        }

        // List of DataBase
        public static IEnumerable<object[]> GetThreeTestDataBases()
        {
            foreach (var db in GetTwoTestDataBases())
                yield return db;

            var options = CreateSQLiteOptions();

            //SQLite in memory provider
            yield return new object[] { "SQLite", options };
        }

        private static void ClearTestDb(MeetingContext context)
        {
            if (context.Database.IsSqlite()) context.Database.OpenConnection();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }


}
