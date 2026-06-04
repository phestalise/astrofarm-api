using AstroFarm.Api.Data;
using AstroFarm.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AstroFarm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutoresController(AppDbContext context)
        {
            _context = context;
        }

        
        [HttpPost("Cadastro")]
        public async Task<ActionResult<Produtor>> Cadastrar([FromBody] Produtor produtor)
        {
            try
            {
                if (produtor == null)
                    return BadRequest(new { message = "Dados inválidos." });

                produtor.Nome = produtor.Nome?.Trim();
                produtor.Cpf = produtor.Cpf?.Replace(".", "").Replace("-", "").Trim();
                produtor.Estado = produtor.Estado?.Trim().ToUpper();
                produtor.Cidade = produtor.Cidade?.Trim();

                if (string.IsNullOrWhiteSpace(produtor.Nome))
                    return BadRequest(new { message = "Nome obrigatório." });

                if (string.IsNullOrWhiteSpace(produtor.Cpf))
                    return BadRequest(new { message = "CPF obrigatório." });

                if (string.IsNullOrWhiteSpace(produtor.Senha))
                    return BadRequest(new { message = "Senha obrigatória." });

                if (string.IsNullOrWhiteSpace(produtor.Estado) || produtor.Estado.Length != 2)
                    return BadRequest(new { message = "Estado inválido." });

                if (string.IsNullOrWhiteSpace(produtor.Cidade))
                    return BadRequest(new { message = "Cidade obrigatória." });

                var existeCpf = await _context.Produtores
                    .AsNoTracking()
                    .AnyAsync(p => p.Cpf == produtor.Cpf);

                if (existeCpf)
                    return BadRequest(new { message = "CPF já cadastrado." });

                produtor.DtCadastro = DateTime.UtcNow;

                _context.Produtores.Add(produtor);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProdutor), new { id = produtor.Id }, produtor);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro de banco ao salvar produtor",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro interno",
                    detail = ex.Message
                });
            }
        }

        // =========================
        // LOGIN
        // =========================
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dadosLogin)
        {
            try
            {
                if (dadosLogin == null)
                    return BadRequest(new { message = "Dados inválidos." });

                var cpf = dadosLogin.Cpf?.Replace(".", "").Replace("-", "").Trim();

                if (string.IsNullOrWhiteSpace(cpf) || string.IsNullOrWhiteSpace(dadosLogin.Senha))
                    return BadRequest(new { message = "CPF e senha obrigatórios." });

                var produtor = await _context.Produtores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Cpf == cpf && p.Senha == dadosLogin.Senha);

                if (produtor == null)
                    return Unauthorized(new { message = "CPF ou senha inválidos." });

                return Ok(produtor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro no login",
                    detail = ex.Message
                });
            }
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<ActionResult<Produtor>> GetProdutor(int id)
        {
            var produtor = await _context.Produtores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (produtor == null)
                return NotFound(new { message = "Produtor não encontrado." });

            return Ok(produtor);
        }

        // =========================
        // UPDATE
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProdutor(int id, [FromBody] Produtor produtorAtualizado)
        {
            try
            {
                var produtor = await _context.Produtores.FirstOrDefaultAsync(p => p.Id == id);

                if (produtor == null)
                    return NotFound(new { message = "Produtor não encontrado." });

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Nome))
                    produtor.Nome = produtorAtualizado.Nome.Trim();

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Email))
                    produtor.Email = produtorAtualizado.Email.Trim();

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Telefone))
                    produtor.Telefone = produtorAtualizado.Telefone.Trim();

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Estado))
                    produtor.Estado = produtorAtualizado.Estado.Trim().ToUpper();

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Cidade))
                    produtor.Cidade = produtorAtualizado.Cidade.Trim();

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Cpf))
                    produtor.Cpf = produtorAtualizado.Cpf.Replace(".", "").Replace("-", "").Trim();

                if (!string.IsNullOrWhiteSpace(produtorAtualizado.Senha))
                    produtor.Senha = produtorAtualizado.Senha;

                await _context.SaveChangesAsync();

                return Ok(produtor);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro ao atualizar no banco",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Erro interno",
                    detail = ex.Message
                });
            }
        }
    }

    // =========================
    // LOGIN DTO
    // =========================
    public class LoginRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}