namespace ProdigyPlanningAPI.FormModels
{
    public class ChangePasswordModel
    {
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
        public string ConfirmPassword { get; set;}
    }
}
