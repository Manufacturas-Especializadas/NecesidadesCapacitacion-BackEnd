namespace NecesidadesCapacitacion.Dtos
{
    public class TrainingNeedsDTO
    {
        public string PresentNeed { get; set; }

        public string PositionsOrCollaborator { get; set; }

        public string SuggestedTrainingCourse { get; set; }

        public string QualityObjective { get; set; }

        public string CurrentPerformance { get; set; }

        public string ExpectedPerformance { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public int? PriorityId { get; set; }
    }
}