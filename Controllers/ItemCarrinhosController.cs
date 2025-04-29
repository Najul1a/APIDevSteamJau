using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIDevSteamJau.Data;
using APIDevSteamJau.Models;

namespace APIDevSteamJau.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItensCarrinhosController : ControllerBase
    {
        private readonly APIContext _context;

        public ItensCarrinhosController(APIContext context)
        {
            _context = context;
        }

        // GET: api/ItensCarrinhos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCarrinho>>> GetItensCarrinhos()
        {
            return await _context.ItemCarrinhos.ToListAsync();
        }

        // GET: api/ItensCarrinhos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemCarrinho>> GetItemCarrinho(Guid id)
        {
            var itemCarrinho = await _context.ItemCarrinhos.FindAsync(id);

            if (itemCarrinho == null)
            {
                return NotFound();
            }

            return itemCarrinho;
        }

        // PUT: api/ItensCarrinhos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemCarrinho(Guid id, ItemCarrinho itemCarrinho)
        {
            if (id != itemCarrinho.ItemCarrinhoId)
            {
                return BadRequest();
            }

            _context.Entry(itemCarrinho).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemCarrinhoExists(id))
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

        // POST: api/ItensCarrinhos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ItemCarrinho>> PostItemCarrinho(ItemCarrinho itemCarrinho)
        {
            // Verifica se o carrinho existe
            var carrinho = await _context.Carrinhos.FindAsync(itemCarrinho.CarrinhoId);
            if (carrinho == null)
            {
                return NotFound("Carrinho não encontrado.");
            }

            // Verifica se o jogo existe
            var jogo = await _context.Jogos.FindAsync(itemCarrinho.JogoId);
            if (jogo == null)
            {
                return NotFound("Jogo não encontrado.");
            }

            // Calcula o valor total
            itemCarrinho.ValorTotal = itemCarrinho.Quantidade * jogo.Preco;

            // Adiciona o valor total ao carrinho
            carrinho.ValorTotal += itemCarrinho.ValorTotal;

            _context.ItemCarrinhos.Add(itemCarrinho);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetItemCarrinho", new { id = itemCarrinho.ItemCarrinhoId }, itemCarrinho);
        }

        // DELETE: api/ItensCarrinhos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemCarrinho(Guid id)
        {
            var itemCarrinho = await _context.ItemCarrinhos.FindAsync(id);
            if (itemCarrinho == null)
            {
                return NotFound();
            }

            //verifica se o carrinho existe
            var carrinho = await _context.Carrinhos.FindAsync(itemCarrinho.CarrinhoId);
            if (carrinho == null)
            {
                return NotFound("Carrinho não encontrado.");
            }

            // Remove o valor total do carrinho
            carrinho.ValorTotal -= itemCarrinho.ValorTotal;

            //verificar se o valor total do carrinho é menor que 0
            if (carrinho.ValorTotal < 0)
            {
                carrinho.ValorTotal = 0;
            }



            _context.ItemCarrinhos.Remove(itemCarrinho);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemCarrinhoExists(Guid id)
        {
            return _context.ItemCarrinhos.Any(e => e.ItemCarrinhoId == id);
        }
    }
}