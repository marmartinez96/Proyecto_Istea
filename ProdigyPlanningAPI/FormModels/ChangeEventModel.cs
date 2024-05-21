using Newtonsoft.Json;
using ProdigyPlanningAPI.Helpers;

namespace ProdigyPlanningAPI.FormModels
{
    public class ChangeEventModel
    {
        public int Id { get; set; }
        public string NewName { get; set; }
        public string NewDescription { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly NewDate { get; set; } = DateOnly.MinValue;
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly NewTime { get; set; } = TimeOnly.MinValue;
        public string NewLocation { get; set; }
        public int? NewCategoryId { get; set; }
        public int? NewDuration { get; set; }
    }
}
