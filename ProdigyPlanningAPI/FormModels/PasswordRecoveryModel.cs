namespace ProdigyPlanningAPI.FormModels
{
    public class PasswordRecoveryModel
    {
        public string? Email { get; set; }
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }
}
