using Newtonsoft.Json;
using ProdigyPlanningAPI.Helpers;

namespace ProdigyPlanningAPI.FormModels
{
    public class AddEventModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly Date { get; set; } = DateOnly.MinValue;
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly Time { get; set; } = TimeOnly.MinValue;
        public string Location { get; set; }
        public int? CategoryId { get; set; }
        public int Duration { get; set; }
    }
}
