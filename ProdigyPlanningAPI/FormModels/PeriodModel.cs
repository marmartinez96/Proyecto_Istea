using System.ComponentModel.DataAnnotations;

namespace ProdigyPlanningAPI.FormModels
{
    public class PeriodModel
    {
        [StringLength(6)]
        [Range(0, Int32.MaxValue)]
        public string cd { get; set; } = DateTime.Now.Month.ToString().PadLeft(2,'0')+DateTime.Now.Year.ToString();
    }
}
