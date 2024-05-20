using Newtonsoft.Json;
using ProdigyPlanningAPI.Helpers;

namespace ProdigyPlanningAPI.FormModels
{
    public class AddEventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly Date { get; set; }
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly Time { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public int Duration { get; set; }
    }
}
