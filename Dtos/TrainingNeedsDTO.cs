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

        public string? ProviderUser { get; set; }

        public string? ProviderAdmin1 { get; set; }

        public string? ProviderAdmin2 { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public int? PriorityId { get; set; }

        public int? CategoryId { get; set; }
    }
}