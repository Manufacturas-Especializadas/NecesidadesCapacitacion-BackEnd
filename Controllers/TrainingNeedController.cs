using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NecesidadesCapacitacion.Dtos;
using NecesidadesCapacitacion.Models;
using NecesidadesCapacitacion.Services;
using System.Security.Claims;

namespace NecesidadesCapacitacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingNeedController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public TrainingNeedController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        [HttpGet]
        [Route("GetCategory")]
        public async Task<IActionResult> GetCategory()
        {
            var category = await _context.TnCategory
                                .AsNoTracking()
                                .ToListAsync();

            if(category == null)
            {
                return BadRequest("List empty");
            }

            return Ok(category);
        }

        [HttpGet]
        [Route("GetStatus")]
        public async Task<IActionResult> GetStatus()
        {
            var status = await _context.TnStatus
                        .AsNoTracking()
                        .ToListAsync();

            if(status == null)
            {
                return BadRequest("List empty");
            }

            return Ok(status);
        }

        [HttpGet]
        [Route("GetTrainingNeedsById/{id}")]
        public async Task<IActionResult> GetTrainingNeedsById(int id)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || !int.TryParse(userClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var trainingNeed = await _context.TrainingNeeds
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (trainingNeed == null)
            {
                return NotFound("Necesidad no encontrada o no tienes permiso.");
            }

            return Ok(trainingNeed);
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
                            Priority = t.Priority!.Name ?? "Sin prioridad asignada",
                            Category = t.Category!.Name ?? "Sin catagoría asignada",
                            Status = t.Status!.Name ?? "Pendiente",
                            t.ProviderUser
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
                                Status = t.Status!.Name ?? "Pendiente",
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

        [HttpGet]
        [Route("DownloadExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var trainingNeeds = await _context.TrainingNeeds
                .Select(t => new
                {
                    t.PresentNeed,
                    t.PositionsOrCollaborator,
                    t.SuggestedTrainingCourse,
                    t.QualityObjective,
                    t.CurrentPerformance,
                    t.ProviderUser,
                    t.ProviderAdmin1,
                    t.ProviderAdmin2,
                    t.ExpectedPerformance,
                    t.RegistrationDate,
                    Priority = t.Priority.Name,
                    Category = t.Category.Name,
                    User = t.User.Name,
                })
                .OrderBy(x => x.Category)
                .AsNoTracking()
                .ToListAsync();

            using (var workBook = new XLWorkbook())
            {
                var workSheet = workBook.Worksheets.Add("Necesidades por Categoría");

                var headers = new[]
                {
                    "Necesidad presente",
                    "Nombres y puestos del colaborador a incluir",
                    "Curso/Entrenamiento sugerido",
                    "Objetivo de calidad / KPI",
                    "Desempeño actual",
                    "Desempeño esperado",
                    "Sugerencia de proveedor del solicitante",
                    "Prioridad",
                    "Categoría",
                    "Nombre del solicitante"
                };

                for (int col = 0; col < headers.Length; col++)
                {
                    workSheet.Cell(1, col + 1).Value = headers[col];
                    workSheet.Cell(1, col + 1).Style.Font.Bold = true;
                    workSheet.Cell(1, col + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                string currentCategory = null;

                foreach (var item in trainingNeeds)
                {
                    if (currentCategory != item.Category)
                    {
                        currentCategory = item.Category;

                        if (row > 2) row++;
                        workSheet.Cell(row, 1).Value = $"Categoría: {currentCategory}";
                        workSheet.Range(row, 1, row, headers.Length)
                                 .Merge()
                                 .Style
                                 .Font.SetBold()
                                 .Fill.SetBackgroundColor(XLColor.FromArgb(240, 240, 240))
                                 .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                        row++;
                    }

                    workSheet.Cell(row, 1).Value = item.PresentNeed;
                    workSheet.Cell(row, 2).Value = item.PositionsOrCollaborator;
                    workSheet.Cell(row, 3).Value = item.SuggestedTrainingCourse;
                    workSheet.Cell(row, 4).Value = item.QualityObjective;
                    workSheet.Cell(row, 5).Value = item.CurrentPerformance;
                    workSheet.Cell(row, 6).Value = item.ExpectedPerformance;
                    workSheet.Cell(row, 7).Value = item.ProviderUser;
                    workSheet.Cell(row, 8).Value = item.Priority;
                    workSheet.Cell(row, 9).Value = item.Category;
                    workSheet.Cell(row, 10).Value = item.User;

                    row++;
                }

                workSheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workBook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"NecesidadesCapacitacion_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] TrainingNeedsDTO trainingNeeds)
        {
            try
            {
                if (trainingNeeds == null)
                {
                    return BadRequest("Campos vacíos");
                }

                var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userClaim == null || !int.TryParse(userClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                if (!userExists)
                {
                    return Unauthorized(new { message = "Usuario no válido" });
                }

                var newTrainingNeeds = new TrainingNeeds
                {
                    PresentNeed = trainingNeeds.PresentNeed,
                    PositionsOrCollaborator = trainingNeeds.PositionsOrCollaborator,
                    SuggestedTrainingCourse = trainingNeeds.SuggestedTrainingCourse,
                    QualityObjective = trainingNeeds.QualityObjective,
                    CurrentPerformance = trainingNeeds.CurrentPerformance,
                    ProviderUser = trainingNeeds.ProviderUser,
                    ProviderAdmin1 = trainingNeeds.ProviderAdmin1,
                    ProviderAdmin2 = trainingNeeds.ProviderAdmin2,
                    ExpectedPerformance = trainingNeeds.ExpectedPerformance,
                    RegistrationDate = trainingNeeds.RegistrationDate ?? DateTime.UtcNow,
                    PriorityId = trainingNeeds.PriorityId,
                    StatusId = 3,
                    UserId = userId,
                    CategoryId = trainingNeeds.CategoryId
                };

                _context.TrainingNeeds.Add(newTrainingNeeds);
                await _context.SaveChangesAsync();

                try
                {
                    await SendEmailCapacitacion(newTrainingNeeds);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando correo: {ex.Message}");
                }

                return Ok(new
                {
                    success = true,
                    message = "Registro exitosamente creado",
                    trainingId = newTrainingNeeds.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    details = ex.ToString()
                });
            }
        }

        [HttpPut]
        [Route("Update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] TrainingNeedsDTO trainingNeedsDTO, int id)
        {
            var trainingNeeds = await _context.TrainingNeeds.FindAsync(id);

            if(trainingNeeds == null)
            {
                return BadRequest("Id no encontrado");
            }

            trainingNeeds.ProviderAdmin1 = trainingNeedsDTO.ProviderAdmin1;
            trainingNeeds.ProviderAdmin2 = trainingNeedsDTO.ProviderAdmin2;
            trainingNeeds.StatusId = trainingNeedsDTO.StatusId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registro actualizado"
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

            _context.TrainingNeeds.Remove(trainingNeedId!);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registro eliminado",
                trainingId = trainingNeedId!.Id
            });
        }

        private async Task SendEmailCapacitacion(TrainingNeeds training)
        {
            var subject = "Nuevo registro de necesidad de capacitación";

            var body = $@"
                <h3>¡Hola!</h3>
                <p>Un usuario acaba de registrar una nueva necesidad de capacitación.</p>
                <p>Por favor, revísala a la brevedad.</p>
                <br/>
                <p>Saludos,<br/>Equipo de Soporte</p>
            ";

            var recipients = new List<string>
            {
                "jose.lugo@mesa.ms",                
            };

            await _emailService.SendEmailAsync(recipients, subject, body);
        }
    }
}