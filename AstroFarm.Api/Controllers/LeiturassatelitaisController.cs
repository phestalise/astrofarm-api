using AstroFarm.Api.Data;
using AstroFarm.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroFarm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeiturasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeiturasController(AppDbContext context)
        {
            _context = context;
        }

        private static string CalcStatusSolo(decimal? ndvi)
        {
            if (ndvi == null) return "Desconhecido";
            if (ndvi >= 0.6m) return "Bom";
            if (ndvi >= 0.3m) return "Regular";
            return "Crítico";
        }

        private static string CalcRisco(decimal? ndvi)
        {
            if (ndvi == null) return "alto";
            if (ndvi >= 0.6m) return "baixo";
            if (ndvi >= 0.3m) return "médio";
            return "alto";
        }

        // GET: api/leituras?propriedadeId=1&limit=30
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetLeituras(
            [FromQuery] int propriedadeId,
            [FromQuery] int limit = 30)
        {
            var leituras = await _context.LeiturasSatelitais
                .AsNoTracking()
                .Where(l => l.IdPropriedade == propriedadeId)
                .OrderByDescending(l => l.DtLeitura)
                .Take(limit)
                .Select(l => new
                {
                    id = l.Id,
                    propriedadeId = l.IdPropriedade,
                    ndvi = l.Ndvi,
                    temperatura = l.Temperatura,
                    umidade = l.Umidade,
                    dataLeitura = l.DtLeitura.ToString("yyyy-MM-ddTHH:mm:ss"),
                    statusSolo = l.Ndvi >= 0.6m ? "Bom" : l.Ndvi >= 0.3m ? "Regular" : "Crítico"
                })
                .ToListAsync();

            return Ok(leituras);
        }

        // GET: api/leituras/ultima/1
        [HttpGet("ultima/{propriedadeId}")]
        public async Task<ActionResult<object>> GetUltima(int propriedadeId)
        {
            var leitura = await _context.LeiturasSatelitais
                .AsNoTracking()
                .Where(l => l.IdPropriedade == propriedadeId)
                .OrderByDescending(l => l.DtLeitura)
                .Select(l => new
                {
                    id = l.Id,
                    propriedadeId = l.IdPropriedade,
                    ndvi = l.Ndvi,
                    temperatura = l.Temperatura,
                    umidade = l.Umidade,
                    dataLeitura = l.DtLeitura.ToString("yyyy-MM-ddTHH:mm:ss"),
                    statusSolo = l.Ndvi >= 0.6m ? "Bom" : l.Ndvi >= 0.3m ? "Regular" : "Crítico"
                })
                .FirstOrDefaultAsync();

            if (leitura == null)
                return NotFound(new { message = "Nenhuma leitura encontrada." });

            return Ok(leitura);
        }

        // GET: api/leituras/dashboard?produtorId=1
        [HttpGet("dashboard")]
        public async Task<ActionResult<IEnumerable<object>>> GetDashboard([FromQuery] int produtorId)
        {
            var propriedades = await _context.Propriedades
                .AsNoTracking()
                .Where(p => p.IdProdutor == produtorId)
                .ToListAsync();

            if (!propriedades.Any())
                return Ok(new List<object>());

            var resultado = new List<object>();

            foreach (var prop in propriedades)
            {
                var leitura = await _context.LeiturasSatelitais
                    .AsNoTracking()
                    .Where(l => l.IdPropriedade == prop.Id)
                    .OrderByDescending(l => l.DtLeitura)
                    .FirstOrDefaultAsync();

                if (leitura == null) continue;

                resultado.Add(new
                {
                    propriedadeId = prop.Id,
                    nome = prop.NomeFazenda,
                    leitura = new
                    {
                        id = leitura.Id,
                        propriedadeId = leitura.IdPropriedade,
                        ndvi = leitura.Ndvi,
                        temperatura = leitura.Temperatura,
                        umidade = leitura.Umidade,
                        dataLeitura = leitura.DtLeitura.ToString("yyyy-MM-ddTHH:mm:ss"),
                        statusSolo = CalcStatusSolo(leitura.Ndvi)
                    },
                    risco = CalcRisco(leitura.Ndvi)
                });
            }

            return Ok(resultado);
        }
    }
}