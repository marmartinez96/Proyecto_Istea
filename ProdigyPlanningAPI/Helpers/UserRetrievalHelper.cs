using Microsoft.EntityFrameworkCore;
using ProdigyPlanningAPI.FormModels;
using ProdigyPlanningAPI.Models;

namespace ProdigyPlanningAPI.Helpers
{
    public class UserRetrievalHelper
    {
        public static UserRetrievalModel CreateRetrievalModel(DbContext context, User u)
        {
            UserRetrievalModel userResult = new UserRetrievalModel();
            if(u != null)
            {
                userResult.Name = u.Name;
                userResult.Surname = u.Surname;
                userResult.Email = u.Email;
            }
            return userResult;
        }

    }
}
