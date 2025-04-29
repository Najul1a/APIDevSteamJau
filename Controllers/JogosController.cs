using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIDevSteamJau.Data;
using APIDevSteamJau.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using APIDevSteamJau.Data;

namespace APIDevSteamJau.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JogosController : ControllerBase
    {
        public readonly APIContext _context;

        private readonly IWebHostEnvironment _webHostEnvironment;  // Informãções do servidor web

        public JogosController(APIContext context)
        {
            _context = context;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/Jogos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Jogo>>> GetJogos()
        {
            return await _context.Jogos.ToListAsync();
        }

        // GET: api/Jogos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Jogo>> GetJogo(Guid id)
        {
            var jogo = await _context.Jogos.FindAsync(id);

            if (jogo == null)
            {
                return NotFound();
            }

            return jogo;
        }

        // PUT: api/Jogos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutJogo(Guid id, Jogo jogo)
        {
            if (id != jogo.JogoId)
            {
                return BadRequest();
            }
            //copiar o preço do jogo para o preço original
            jogo.precoOriginal = jogo.Preco;

            //Calcular o preço com desconto
            if (jogo.Desconto > 0)
            {
                jogo.Preco = jogo.Preco - (jogo.Preco * (jogo.Desconto / 100));
            }

            _context.Entry(jogo).State = EntityState.Modified;

            try
            {

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JogoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Jogos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Jogo>> PostJogo([FromBody] Jogo jogo)


        {
            //copiar o preço do jogo para o preço original
            jogo.precoOriginal = jogo.Preco;

            //Calcular o preço com desconto
            if (jogo.Desconto > 0)
            {
                jogo.Preco = jogo.Preco - (jogo.Preco * (jogo.Desconto / 100));
            }

            _context.Jogos.Add(jogo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJogo", new { id = jogo.JogoId }, jogo);
        }

        // DELETE: api/Jogos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJogo(Guid id)
        {
            var jogo = await _context.Jogos.FindAsync(id);
            if (jogo == null)
            {
                return NotFound();
            }

            _context.Jogos.Remove(jogo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JogoExists(Guid id)
        {
            return _context.Jogos.Any(e => e.JogoId == id);
        }

        // [HttpPOST] : Upload da Foto de Perfil
        [HttpPost("UploadBanner")]
        public async Task<IActionResult> UploadBanner(IFormFile file, Guid jogoId)
        {
            // Verifica se o arquivo é nulo ou vazio
            if (file == null || file.Length == 0)
                return BadRequest("Arquivo não pode ser nulo ou vazio.");

            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null)
                return NotFound("Jogo não encontrado.");

            // Verifica se o arquivo é uma imagem
            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("O arquivo deve ser uma imagem.");

            var bannerFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources", "Banners");
            if (!Directory.Exists(bannerFolder))
                Directory.CreateDirectory(bannerFolder);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Formato de arquivo não suportado.");

            var fileName = $"{jogoId}{fileExtension}";
            var filePath = Path.Combine(bannerFolder, fileName);

            // Verifica se o arquivo já existe e o remove
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            // Salva o arquivo no disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("Resources", "Banners", fileName).Replace("\\", "/");

            // Atualiza o Campo Banner do Jogo
            jogo.Banner = fileName;
            _context.Entry(jogo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Retorna o caminho da imagem
            return Ok(new { FilePath = relativePath });
        }


        [HttpGet("GetBanner/{jogoId}")]
        public async Task<IActionResult> GetBanner(Guid jogoId)
        {
            // Verifica se o jogo existe
            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null)
                return NotFound("Jogo não encontrado.");

            var bannerFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Resources", "Banners");
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            string? bannerPath = null;
            foreach (var ext in allowedExtensions)
            {
                var potentialPath = Path.Combine(bannerFolder, $"{jogoId}{ext}");
                if (System.IO.File.Exists(potentialPath))
                {
                    bannerPath = potentialPath;
                    break;
                }
            }
            // Se a imagem não for encontrada
            if (userImagePath == null)
                return NotFound("Imagem de perfil não encontrada.");

            if (bannerPath == null)
                return NotFound("Banner não encontrado.");

            var imageBytes = await System.IO.File.ReadAllBytesAsync(bannerPath);
            var base64Image = Convert.ToBase64String(imageBytes);

            return Ok(new { Base64Image = $"data:image/{Path.GetExtension(bannerPath).TrimStart('.')};base64,{base64Image}" });
        }
        //[HttpPUT] : Aplicar um Desconto
        [HttpPut("AplicarDesconto")]
        public async Task<IActionResult> AplicarDesconto(Guid jogoId, int desconto)
        {
            // Verifica se o jogo existe
            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null)
                return NotFound("Jogo não encontrado.");

            // Verifica se o desconto é válido
            if (desconto < 0 || desconto > 100)
                return BadRequest("Desconto deve ser entre 0 e 100.");

            // Aplica o desconto
            jogo.Desconto = desconto;
            jogo.Preco = (decimal)(jogo.precoOriginal - (jogo.precoOriginal * (desconto / 100)));

            // Atualiza o jogo no banco de dados
            _context.Entry(jogo).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(jogo);
        }

        // [HttpPUT] : Remover um Desconto
        [HttpPut("RemoverDesconto")]
        public async Task<IActionResult> RemoverDesconto(Guid jogoId)
        {
            // Verifica se o jogo existe
            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null)
                return NotFound("Jogo não encontrado.");

            // Remove o desconto
            jogo.Desconto = 0;
            jogo.Preco = (decimal)jogo.PrecoOriginal;

            // Atualiza o jogo no banco de dados
            _context.Entry(jogo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(jogo);
        }

    }
}


