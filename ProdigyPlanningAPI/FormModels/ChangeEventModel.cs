namespace ProdigyPlanningAPI.FormModels
{
    public class ChangeEventModel
    {
        public int Id { get; set; }
        public string NewName { get; set; }
        public string NewDescription { get; set; }
        public DateTime? NewDate { get; set; }
        public string NewLocation { get; set; }
        public string NewCategory { get; set; }
        public int NewDuration { get; set; }
    }
}
