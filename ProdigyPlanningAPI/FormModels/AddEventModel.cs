namespace ProdigyPlanningAPI.FormModels
{
    public class AddEventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public int Duration { get; set; }
    }
}
