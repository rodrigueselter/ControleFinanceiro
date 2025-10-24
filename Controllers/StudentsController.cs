using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Controle.Data;
using Controle.Models;

namespace Escola.Controllers;

[ApiController]
[Route("api/v1/[controller]")] // Utiliza nome  do arquivo/classe
public class TransactionsController : ControllerBase{
    private readonly AppDbContext _db;
    public TransactionsController(AppDbContext db)=>_db =db;

    //GET /api/v1/Transactions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetAll()
        => Ok(await _db.Transactions.OrderBy(t=>t.Id).ToListAsync());
    
    //GET /api/v1/Transactions/1(id)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Transaction>> GetById(int id)
        => await _db.Transactions.FindAsync(id) is { } t ? Ok(t) : NotFound();

    //POST  /api/v1/Transactions
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transaction t){
        // Validações
        // Descrição obrigatória
        if (string.IsNullOrWhiteSpace(t.Descricao))
            return BadRequest(new { error = "A descrição é obrigatória." });

        // Valor maior que 0
        if (t.Valor <= 0)
            return BadRequest(new { error = "O valor tem que ser maior que 0." });

        // Tipo obrigatório e deve ser Receita ou Despesa
        if (string.IsNullOrWhiteSpace(t.Tipo) || (t.Tipo != "Receita" && t.Tipo != "Despesa"))
            return BadRequest(new { error = "O tipo deve ser 'Receita' ou 'Despesa'." });

        // Caso não coloque a categoria, adiciona 'Outros' por padrão
        if (string.IsNullOrWhiteSpace(t.Categoria))
            t.Categoria = "Outros";

        _db.Transactions.Add(t);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof (GetById), new {id = t.Id}, t);
    }

    //PUT /api/v1/Transactions/1(id)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Transaction t){
        t.Id = id;

        // Validações
        // Descrição obrigatória
        if (string.IsNullOrWhiteSpace(t.Descricao))
            return BadRequest(new { error = "A descrição é obrigatória." });

        // Valor maior que 0
        if (t.Valor <= 0)
            return BadRequest(new { error = "O valor tem que ser maior que 0." });

        // Tipo obrigatório e deve ser Receita ou Despesa
        if (string.IsNullOrWhiteSpace(t.Tipo) || (t.Tipo != "Receita" && t.Tipo != "Despesa"))
            return BadRequest(new { error = "O tipo deve ser 'Receita' ou 'Despesa'." });

        if(!await _db.Transactions.AnyAsync(x=> x.Id == id)) return NotFound();

        _db.Entry(t).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // DELETE /api/v1/Transactions/1(id)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.Transactions.FindAsync(id);
        if (t is null) return NotFound();

        _db.Transactions.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/v1/Transactions/saldo
    [HttpGet("saldo")]
    public async Task<IActionResult> GetSaldo()
    {
        var receitas = await _db.Transactions
            .Where(t => t.Tipo == "Receita")
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var despesas = await _db.Transactions
            .Where(t => t.Tipo == "Despesa")
            .SumAsync(t => (decimal?)t.Valor) ?? 0;

        var saldo = receitas - despesas;

        return Ok(new
        {
            receitas,
            despesas,
            saldo,
            status = saldo >= 0 ? "Positivo" : "Negativo"
        });
    }
}
