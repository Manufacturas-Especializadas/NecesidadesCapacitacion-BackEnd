using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NecesidadesCapacitacion.Dtos;
using NecesidadesCapacitacion.Models;
using System.Security.Claims;

namespace NecesidadesCapacitacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingNeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TrainingNeedController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetTrainingNeed")]
        public async Task<IActionResult> GetTrainingNeedAsync()
        {
            var list = await _context.TrainingNeeds
                    .AsNoTracking()
                    .ToListAsync();

            if(list == null)
            {
                return NotFound("List empty");
            }


            return Ok(list);
        }

        [HttpGet]
        [Route("GetPriority")]
        public async Task<IActionResult> GetPriority()
        {
            var list = await _context.TnPriority
                    .AsNoTracking()
                    .ToListAsync();

            if(list == null)
            {
                return NotFound("List empty");
            }

            return Ok(list);
        }

        [Authorize]
        [HttpGet]
        [Route("GetTrainingNeeds")]
        public async Task<IActionResult> GetTrainingNeeds()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || !int.TryParse(userClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var list = await _context.TrainingNeeds
                        .OrderByDescending(t => t.Id)
                        .Where(t => t.UserId == userId)
                        .Select(t => new
                        {
                            t.Id,
                            t.PresentNeed,
                            t.PositionsOrCollaborator,
                            t.SuggestedTrainingCourse,
                            t.QualityObjective,
                            t.CurrentPerformance,
                            t.ExpectedPerformance,
                            Priority = t.Priority.Name,
                        })
                        .AsNoTracking()
                        .ToListAsync();

            if(list == null)
            {
                return BadRequest("List empty");
            }

            return Ok(list);
        }
        

        [HttpGet]
        [Route("GetTrainingNeedsByUser")]
        public async Task<IActionResult> GetTrainingNeedsByUser()
        {
            var userNeeds = await _context.Users
                    .Where(u => u.TrainingNeeds.Any())
                    .Select(u => new UserTrainingNeedsDTO
                    {
                        UserId = u.Id,
                        UserName = u.Name,
                        PayRollNumber = u.PayRollNumber,
                        Role = u.Rol.Name,
                        TrainingNeeds = u.TrainingNeeds
                            .Select(t => new TrainingNeedSummaryDTO
                            {
                                Id = t.Id,
                                PresentNeed = t.PresentNeed,
                                SuggestedTrainingCourse = t.SuggestedTrainingCourse,
                                Priorirty = t.Priority.Name,
                                RegistrationDate = t.RegistrationDate,
                            })
                            .ToList(),
                        TotalNeeds = u.TrainingNeeds.Count,
                        HighPriorityCount = u.TrainingNeeds.Count(t => t.Priority.Name == "MUY URGENTE"),
                        MediumPriorityCount = u.TrainingNeeds.Count(t => t.Priority.Name == "URGENTE"),
                        LowPriorityCount = u.TrainingNeeds.Count(t => t.Priority.Name == "POCO URGENTE")
                    })
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToListAsync();

            return userNeeds.Any()
                    ? Ok(userNeeds)
                    : NotFound("No se encontraron necesidades agrupadas por usuario");
        }

        [Authorize]
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] TrainingNeedsDTO trainingNeeds)
        {
            if(trainingNeeds == null)
            {
                return BadRequest("Campos vacios");
            }

            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if(userClaim == null || !int.TryParse(userClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if(!userExists)
            {
                return Unauthorized(new { message = "Usuario no valid" });
            }

            var newTrainingNeeds = new TrainingNeeds
            {
                PresentNeed = trainingNeeds.PresentNeed,
                PositionsOrCollaborator = trainingNeeds.PositionsOrCollaborator,
                SuggestedTrainingCourse = trainingNeeds.SuggestedTrainingCourse,
                QualityObjective = trainingNeeds.QualityObjective,
                CurrentPerformance = trainingNeeds.CurrentPerformance,
                ExpectedPerformance = trainingNeeds.ExpectedPerformance,
                RegistrationDate = trainingNeeds.RegistrationDate ?? DateTime.UtcNow,
                PriorityId = trainingNeeds.PriorityId,
                UserId = userId
            };

            _context.TrainingNeeds.Add(newTrainingNeeds);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registro existosamente creado",
                trainingId = newTrainingNeeds.Id
            });
        }

        [HttpDelete]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trainingNeedId = await _context.TrainingNeeds.FindAsync(id);

            if(trainingNeedId == null)
            {
                NotFound("Id no encontrado");
            }

            _context.TrainingNeeds.Remove(trainingNeedId);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registro eliminado",
                trainingId = trainingNeedId.Id
            });
        }
    }
}