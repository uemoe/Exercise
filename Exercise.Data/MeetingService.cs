using System;
using Exercise.Domain;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Exercise.Data.BusinessLogic
{
   
    public class MeetingService 
    {
        private readonly MeetingContext _context;

        private readonly ISaveStrategy _strategy;

        public MeetingService(MeetingContext context, ISaveStrategy strategy)
        {
            _context = context;
            _strategy = strategy;
        }
        public void SaveMeeting(Meeting input)
        {
            _strategy.SaveMeeting(_context, input);
        }
    }

    public class SaveWithDelete : ISaveStrategy
    {
        public void SaveMeeting(MeetingContext context, Meeting input)
        {
            var graph = context.Meetings.AsNoTracking()
                                         //            .Include(b => b.Races)
                                         .FirstOrDefault(m => m.OuterId == input.OuterId);

            if (graph != null)
            {
                context.Meetings.Remove(graph);
                context.SaveChanges();
            }

            context.Add(input);
        }
    }

    public interface ISaveStrategy
    {
        void SaveMeeting(MeetingContext context, Meeting input);
    }

    public class SaveWithUpdate : ISaveStrategy
    {
        public void SaveMeeting(MeetingContext context, Meeting input)
        {
            var graph = context.Meetings.AsNoTracking()
                                        .Include(b => b.Races)
                             .FirstOrDefault(m => m.OuterId == input.OuterId);

            if (graph != null)
            {
                input.Id = graph.Id;
                var dictionary = graph.Races
                                      .ToDictionary(r => r.OuterId, r => r);

                input.Races.ForEach(x => x.Id = (dictionary.ContainsKey(x.OuterId) ? dictionary[x.OuterId].Id : x.Id));

                context.Meetings.Update(input);
                
              //  context.SaveChanges();
            }
            else
            {
                context.Add(input);
            }

        }
    }
}