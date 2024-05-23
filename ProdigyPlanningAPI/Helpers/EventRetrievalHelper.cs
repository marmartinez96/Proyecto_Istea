using Azure;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.FormModels;
using ProdigyPlanningAPI.Models;

namespace ProdigyPlanningAPI.Helpers
{
    public class EventRetrievalHelper
    {
        public static EventRetrievalModel CreateRetrievalModel(DbContext context, Event e)
        {
            EventRetrievalModel eventResult = new EventRetrievalModel();
            if (e != null)
            {
                eventResult.Id = e.Id;
                eventResult.Name = e.Name;
                eventResult.Date = e.Date.ToString();
                eventResult.Time = e.Time.ToString();
                eventResult.Location = e.Location;
                eventResult.Description = e.Description;
                if(e.CreatedByNavigation != null)
                {
                    eventResult.CreatedBy = e.CreatedByNavigation.Name + ' ' + e.CreatedByNavigation.Surname;
                }
                eventResult.Duration = e.Duration;
                foreach (Category c in e.Categories)
                {
                    eventResult.Categories.Add(c.Name);
                }
                if (e.Banner != null)
                {
                    eventResult.HasBanner = true;
                }
                eventResult.IsFeatured = e.IsFeatured;
            } 
            return eventResult;
        }
    }
}
