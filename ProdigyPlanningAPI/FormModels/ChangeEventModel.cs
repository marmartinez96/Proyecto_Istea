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
        public DateOnly NewDate { get; set; }
        [JsonConverter(typeof(TimeOnlyJsonConverter))]
        public TimeOnly NewTime { get; set; }
        public string NewLocation { get; set; }
        public string NewCategory { get; set; }
        public int? NewDuration { get; set; }
    }
}
