using AstroFarm.Api.Data;
using AstroFarm.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroFarm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropriedadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PropriedadesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPropriedades([FromQuery] int produtorId)
        {
            var query = _context.Propriedades.AsNoTracking();

            if (produtorId > 0)
                query = query.Where(p => p.IdProdutor == produtorId);

            var lista = await query
                .OrderByDescending(p => p.DtRegistro)
                .Select(p => new
                {
                    id           = p.Id,
                    produtorId   = p.IdProdutor,
                    nomeFazenda  = p.NomeFazenda,
                    areaHectares = p.AreaHectares,
                    estado       = p.Estado,
                    municipio    = p.Municipio,
                    latitude     = p.Latitude,
                    longitude    = p.Longitude,
                    dtRegistro   = p.DtRegistro.ToString("yyyy-MM-ddTHH:mm:ss"),
                    totalCulturas = p.Culturas.Count()
                })
                .ToListAsync();

            return Ok(lista);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPropriedade(int id)
        {
            var p = await _context.Propriedades
                .AsNoTracking()
                .Include(x => x.Culturas)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null)
                return NotFound(new { message = "Propriedade não encontrada." });

            return Ok(new
            {
                id           = p.Id,
                produtorId   = p.IdProdutor,
                nomeFazenda  = p.NomeFazenda,
                areaHectares = p.AreaHectares,
                estado       = p.Estado,
                municipio    = p.Municipio,
                latitude     = p.Latitude,
                longitude    = p.Longitude,
                dtRegistro   = p.DtRegistro.ToString("yyyy-MM-ddTHH:mm:ss"),
                culturas     = p.Culturas.Select(c => new
                {
                    id           = c.Id,
                    tipoCultura  = c.TipoCultura,
                    safra        = c.Safra,
                    areaPlantada = c.AreaPlantada,
                    status       = c.Status
                })
            });
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostPropriedade([FromBody] PropriedadeDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Dados inválidos." });

            if (string.IsNullOrWhiteSpace(dto.NomeFazenda))
                return BadRequest(new { message = "Nome da fazenda é obrigatório." });

            if (string.IsNullOrWhiteSpace(dto.Estado) || dto.Estado.Length != 2)
                return BadRequest(new { message = "Estado inválido (use sigla com 2 letras)." });

            if (string.IsNullOrWhiteSpace(dto.Municipio))
                return BadRequest(new { message = "Município é obrigatório." });

            if (dto.AreaHectares <= 0)
                return BadRequest(new { message = "Área deve ser maior que zero." });

            var propriedade = new Propriedade
            {
                IdProdutor   = dto.IdProdutor,
                NomeFazenda  = dto.NomeFazenda.Trim(),
                AreaHectares = dto.AreaHectares,
                Estado       = dto.Estado.Trim().ToUpper(),
                Municipio    = dto.Municipio.Trim(),
                Latitude     = dto.Latitude,
                Longitude    = dto.Longitude,
                DtRegistro   = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            };

            _context.Propriedades.Add(propriedade);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(dto.TipoCultura))
            {
                var cultura = new Cultura
                {
                    IdPropriedade  = propriedade.Id,
                    TipoCultura    = dto.TipoCultura.Trim(),
                    Safra          = dto.Safra?.Trim() ?? DateTime.Now.Year.ToString(),
                    AreaPlantada   = (decimal?)dto.AreaHectares,
                    DtPlantio      = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    Status         = "A",
                };
                _context.Culturas.Add(cultura);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetPropriedade), new { id = propriedade.Id }, new
            {
                id           = propriedade.Id,
                produtorId   = propriedade.IdProdutor,
                nomeFazenda  = propriedade.NomeFazenda,
                areaHectares = propriedade.AreaHectares,
                estado       = propriedade.Estado,
                municipio    = propriedade.Municipio,
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPropriedade(int id, [FromBody] PropriedadeDto dto)
        {
            var propriedade = await _context.Propriedades.FirstOrDefaultAsync(p => p.Id == id);

            if (propriedade == null)
                return NotFound(new { message = "Propriedade não encontrada." });

            if (!string.IsNullOrWhiteSpace(dto.NomeFazenda))
                propriedade.NomeFazenda = dto.NomeFazenda.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Estado))
                propriedade.Estado = dto.Estado.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(dto.Municipio))
                propriedade.Municipio = dto.Municipio.Trim();

            if (dto.AreaHectares > 0)
                propriedade.AreaHectares = dto.AreaHectares;

            if (dto.Latitude.HasValue)
                propriedade.Latitude = dto.Latitude;

            if (dto.Longitude.HasValue)
                propriedade.Longitude = dto.Longitude;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id           = propriedade.Id,
                nomeFazenda  = propriedade.NomeFazenda,
                areaHectares = propriedade.AreaHectares,
                estado       = propriedade.Estado,
                municipio    = propriedade.Municipio,
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePropriedade(int id)
        {
            try
            {
                var propriedade = await _context.Propriedades.FindAsync(id);

                if (propriedade == null)
                    return NotFound(new { message = "Propriedade não encontrada." });

                var alertas = _context.Alertas.Where(a => a.IdPropriedade == id);
                _context.Alertas.RemoveRange(alertas);

                var leituras = _context.LeiturasSatelitais.Where(l => l.IdPropriedade == id);
                _context.LeiturasSatelitais.RemoveRange(leituras);

                var culturas = _context.Culturas.Where(c => c.IdPropriedade == id);
                _context.Culturas.RemoveRange(culturas);

                _context.Propriedades.Remove(propriedade);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Propriedade removida." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao remover propriedade.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }

    public class PropriedadeDto
    {
        public int    IdProdutor   { get; set; }
        public string NomeFazenda  { get; set; } = string.Empty;
        public double AreaHectares { get; set; }
        public string Estado       { get; set; } = string.Empty;
        public string Municipio    { get; set; } = string.Empty;
        public double? Latitude    { get; set; }
        public double? Longitude   { get; set; }
        public string? TipoCultura { get; set; }
        public string? Safra       { get; set; }
    }
}