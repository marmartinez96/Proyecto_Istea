namespace ProdigyPlanningAPI.FormModels
{
    public class ChangeEventModel
    {
        public int Id { get; set; }
        public string NewName { get; set; }
        public string NewDescription { get; set; }
        public DateOnly NewDate { get; set; }
        public TimeOnly NewTime { get; set; }
        public string NewLocation { get; set; }
        public string NewCategory { get; set; }
        public int? NewDuration { get; set; }
    }
}
