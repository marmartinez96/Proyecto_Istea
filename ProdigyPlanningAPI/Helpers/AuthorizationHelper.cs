using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Models;
using System.Security.Claims;

namespace ProdigyPlanningAPI.Helpers
{
    public class AuthorizationHelper
    {
        public static dynamic ValidateToken(ClaimsIdentity identity, ProdigyPlanningContext _context)
        {
            try
            {
                var id = identity.Claims.FirstOrDefault(a => a.Type == "id").Value;

                User user = _context.Users.FirstOrDefault(a => a.Id.ToString() == id);

                if(user == null)
                {
                    throw new Exception("Verifique estar usando el token correcto");
                }

                return new
                {
                    success = true,
                    message = "success",
                    result = user
                };
            }
            catch(Exception ex)
            {
                return new
                {
                    success = false,
                    message = ex.Message,
                    result = ""
                };
            }
        }
    }
}
