using System;
using System.Collections.Generic;

namespace NecesidadesCapacitacion.Models;

public partial class TrainingNeeds
{
    public int Id { get; set; }

    public string PresentNeed { get; set; }

    public string PositionsOrCollaborator { get; set; }

    public string SuggestedTrainingCourse { get; set; }

    public string QualityObjective { get; set; }

    public string CurrentPerformance { get; set; }

    public string ExpectedPerformance { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public int? PriorityId { get; set; }

    public int? UserId { get; set; }

    public virtual TnPriority Priority { get; set; }

    public virtual Users User { get; set; }
}