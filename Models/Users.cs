﻿using System;
using System.Collections.Generic;

namespace NecesidadesCapacitacion.Models;

public partial class Users
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int PayRollNumber { get; set; }

    public int RolId { get; set; }

    public string PasswordHash { get; set; }

    public string RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual Roles Rol { get; set; }

    public virtual ICollection<TrainingNeeds> TrainingNeeds { get; set; } = new List<TrainingNeeds>();
}