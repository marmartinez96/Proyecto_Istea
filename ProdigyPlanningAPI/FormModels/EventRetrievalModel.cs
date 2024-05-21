namespace ProdigyPlanningAPI.FormModels
{
    public class EventRetrievalModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public int? Duration { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
        public bool HasBanner { get; set; } = false;
        public bool IsFeatured { get; set; } = false;

    }
}
