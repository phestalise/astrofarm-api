using AstroFarm.Api.Data;
using AstroFarm.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroFarm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AlertasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/alertas?produtorId=1&ativo=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAlertas(
            [FromQuery] int produtorId,
            [FromQuery] bool ativo = true)
        {
            var query = _context.Alertas
                .AsNoTracking()
                .Include(a => a.Propriedade)
                .Where(a => a.Propriedade != null && a.Propriedade.IdProdutor == produtorId);

            if (ativo)
                query = query.Where(a => a.Resolvido == "N");

            var alertas = await query
                .OrderByDescending(a => a.DtAlerta)
                .Select(a => new
                {
                    id = a.Id,
                    propriedadeId = a.IdPropriedade,
                    tipo = a.TipoAlerta.ToLower(),
                    mensagem = a.Descricao ?? a.TipoAlerta,
                    gravidade = a.NivelRisco.ToLower() == "alto" ? "alta"
                              : a.NivelRisco.ToLower() == "medio" || a.NivelRisco.ToLower() == "médio" ? "média"
                              : "baixa",
                    ativo = a.Resolvido == "N",
                    createdAt = a.DtAlerta.ToString("yyyy-MM-ddTHH:mm:ss")
                })
                .ToListAsync();

            return Ok(alertas);
        }

        [HttpPut("{id}/resolver")]
        public async Task<IActionResult> Resolver(int id)
        {
            var alerta = await _context.Alertas.FindAsync(id);

            if (alerta == null)
                return NotFound(new { message = "Alerta não encontrado." });

            alerta.Resolvido = "S";
            alerta.DtResolucao = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Alerta resolvido." });
        }
    }
}