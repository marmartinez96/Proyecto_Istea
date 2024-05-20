using Newtonsoft.Json;
using ProdigyPlanningAPI.Helpers;

namespace ProdigyPlanningAPI.FormModels
{
    public class FilterEventModel
    {
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly FromDate { get; set; } = DateOnly.MinValue;
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? ToDate { get; set; } = DateOnly.MaxValue;
    }
}
