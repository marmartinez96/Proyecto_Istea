namespace ProdigyPlanningAPI.FormModels
{
    public class SignupModel
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Password { get; set; }
        public int UserQuestionId { get; set; }
        public string? UserQuestionAnswer { get; set; }
        public string? Roles { get; set; }
    }
}
