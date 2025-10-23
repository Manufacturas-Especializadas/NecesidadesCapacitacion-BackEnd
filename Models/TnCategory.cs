using System;
using System.Collections.Generic;

namespace NecesidadesCapacitacion.Models;

public partial class TnCategory
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<TrainingNeeds> TrainingNeeds { get; set; } = new List<TrainingNeeds>();
}