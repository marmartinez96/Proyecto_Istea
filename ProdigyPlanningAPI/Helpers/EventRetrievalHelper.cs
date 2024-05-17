using Azure;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.FormModels;
using ProdigyPlanningAPI.Models;
using System.Security.Claims;

namespace ProdigyPlanningAPI.Helpers
{
    public class EventRetrievalHelper
    {
        public static EventRetrievalModel CreateRetrievalModel(DbContext context, Event e)
        {
            EventRetrievalModel eventResult = new EventRetrievalModel();
            eventResult.Id = e.Id;
            eventResult.Name = e.Name;
            eventResult.Date = e.Date;
            eventResult.Location = e.Location;
            eventResult.Description = e.Description;
            eventResult.CreatedBy = e.CreatedByNavigation.Name + ' ' + e.CreatedByNavigation.Surname;
            eventResult.Duration = e.Duration;
            foreach (Category c in e.Categories)
            {
                eventResult.Categories.Add(c.Name);
            }
            if (e.Banner != null)
            {
                eventResult.HasBanner = true;
            }
            return eventResult;
        }
    }
}
