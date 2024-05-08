namespace ProdigyPlanningAPI.FormModels
{
    public class ChangeEventModel
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
        public string NewDescription { get; set; }
        public DateTime? NewDate { get; set; }
        public string NewLocation { get; set; }
    }
}
