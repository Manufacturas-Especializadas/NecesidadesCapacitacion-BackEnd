namespace NecesidadesCapacitacion.Dtos
{
    public class UserTrainingNeedsDTO
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public int PayRollNumber { get; set; }

        public string Role { get; set; }

        public List<TrainingNeedSummaryDTO> TrainingNeeds { get; set; }

        public int TotalNeeds { get; set; }

        public int HighPriorityCount { get; set; }

        public int MediumPriorityCount { get; set; }

        public int LowPriorityCount { get; set; }
    }
}