namespace NecesidadesCapacitacion.Dtos
{
    public class TrainingNeedSummaryDTO
    {
        public int Id { get; set; }

        public string PresentNeed { get; set; }

        public string SuggestedTrainingCourse { get; set; }

        public string Priorirty {  get; set; }

        public DateTime? RegistrationDate { get; set; } 
    }
}