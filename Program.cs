using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Controle.Data;
using Controle.Models;

var builder = WebApplication.CreateBuilder(args);

// Porta fixa (opcional, facilita testes)
builder.WebHost.UseUrls("http://localhost:5099");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=Controle.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

var webTask = app.RunAsync();
Console.WriteLine("API online em http://localhost:5099 (Swagger em /swagger)");

Console.WriteLine("== FinanceControlDb ==");
Console.WriteLine("Console + API executando juntos!");

while (true)
{
    // Menu de opções
    Console.WriteLine();
    Console.WriteLine("Escolha uma opção:");
    Console.WriteLine("1 - Cadastrar transação");
    Console.WriteLine("2 - Listar transações");
    Console.WriteLine("3 - Atualizar transação (por Id)");
    Console.WriteLine("4 - Remover transação (por Id)");
    Console.WriteLine("5 - Ver saldo atual");
    Console.WriteLine("0 - Sair");
    Console.Write("> ");

    var opt = Console.ReadLine();

    if (opt == "0") break;

    switch (opt)
    {
        // switch para chamar a função correspondente 
        case "1": await CreateTransactionAsync(); break;
        case "2": await ListTransactionsAsync(); break;
        case "3": await UpdateTransactionAsync(); break;
        case "4": await DeleteTransactionAsync(); break;
        case "5": await ShowSaldoAsync(); break;
        default: Console.WriteLine("Opção inválida."); break;
    }
}

await app.StopAsync();
await webTask;

// função para criar transaçção
async Task CreateTransactionAsync()
{
    Console.Write("Descrição: ");
    var descricao = (Console.ReadLine() ?? "").Trim();

    Console.Write("Valor: ");
    if (!decimal.TryParse(Console.ReadLine(), out var valor) || valor <= 0)
    {
        Console.WriteLine("Valor inválido!");
        return;
    }

    Console.Write("Tipo (Receita/Despesa): ");
    var tipo = (Console.ReadLine() ?? "").Trim();
    if (tipo != "Receita" && tipo != "Despesa")
    {
        Console.WriteLine("Tipo inválido!");
        return;
    }

    Console.Write("Categoria: ");
    var categoria = (Console.ReadLine() ?? "").Trim();
    if (string.IsNullOrWhiteSpace(categoria)) categoria = "Outros";

    using var db = new AppDbContext();
    var t = new Transaction
    {
        Descricao = descricao,
        Valor = valor,
        Tipo = tipo,
        Categoria = categoria,
        Data = DateTime.UtcNow
    };

    db.Transactions.Add(t);
    await db.SaveChangesAsync();
    Console.WriteLine($"Transação cadastrada com sucesso! Id: {t.Id}");
}

// função para listar transações
async Task ListTransactionsAsync()
{
    using var db = new AppDbContext();
    var Transactions = await db.Transactions.OrderBy(t => t.Id).ToListAsync();

    if (Transactions.Count == 0)
    {
        Console.WriteLine("Nenhuma transação encontrada.");
        return; 
    }

    Console.WriteLine("ID | Tipo     | Valor      | Categoria        | Descrição");
    foreach (var t in Transactions)
        Console.WriteLine($"{t.Id,2} | {t.Tipo,-8} | {t.Valor,10:C} | {t.Categoria,-15} | {t.Descricao}");
}

// função para atualizar transação
async Task UpdateTransactionAsync()
{
    Console.Write("Informe o Id da transação a atualizar: ");
    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Id inválido.");
        return;
    }

    using var db = new AppDbContext();
    var t = await db.Transactions.FirstOrDefaultAsync(x => x.Id == id);
    // validação para id não encontrado
    if (t is null)
    {
        Console.WriteLine("Transação não encontrada.");
        return; 
    }

    Console.WriteLine($"Atualizando transação Id {t.Id}. Deixe em branco para manter os valores atuais.");

    Console.WriteLine($"Descrição atual: {t.Descricao}");
    Console.Write("Nova descrição: ");
    var novaDesc = (Console.ReadLine() ?? "").Trim();
    if (!string.IsNullOrWhiteSpace(novaDesc)) t.Descricao = novaDesc;

    Console.WriteLine($"Valor atual: {t.Valor}");
    Console.Write("Novo valor: ");
    var valorInput = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(valorInput) && decimal.TryParse(valorInput, out var novoValor))
        t.Valor = novoValor;

    Console.WriteLine($"Tipo atual: {t.Tipo}");
    Console.Write("Novo tipo (Receita/Despesa): ");
    var novoTipo = (Console.ReadLine() ?? "").Trim();
    if (novoTipo == "Receita" || novoTipo == "Despesa") t.Tipo = novoTipo;

    Console.WriteLine($"Categoria atual: {t.Categoria}");
    Console.Write("Nova categoria: ");
    var novaCat = (Console.ReadLine() ?? "").Trim();
    if (!string.IsNullOrWhiteSpace(novaCat)) t.Categoria = novaCat;

    await db.SaveChangesAsync();
    Console.WriteLine("Transação atualizada com sucesso.");
}

// função para excluir transação
async Task DeleteTransactionAsync()
{
    Console.Write("Informe o Id da transação a remover: ");
    if (!int.TryParse(Console.ReadLine(), out var id))
    {
        Console.WriteLine("Id inválido.");
        return;
    }

    using var db = new AppDbContext();
    var t = await db.Transactions.FirstOrDefaultAsync(x => x.Id == id);
    if (t is null)
    {
        Console.WriteLine("Transação não encontrada.");
        return;
    }

    db.Transactions.Remove(t);
    await db.SaveChangesAsync();
    Console.WriteLine("Transação removida com sucesso.");
}

// função para calcular o saldo
async Task ShowSaldoAsync()
{
    using var db = new AppDbContext();

    var receitas = await db.Transactions
        .Where(t => t.Tipo == "Receita")
        .SumAsync(t => (decimal?)t.Valor) ?? 0;

    var despesas = await db.Transactions
        .Where(t => t.Tipo == "Despesa")
        .SumAsync(t => (decimal?)t.Valor) ?? 0;

    var saldo = receitas - despesas;

    Console.WriteLine("------ SALDO ATUAL ------");
    Console.WriteLine($"Receitas: {receitas:C}");
    Console.WriteLine($"Despesas: {despesas:C}");
    Console.WriteLine($"Saldo: {saldo:C}");
    Console.WriteLine(saldo >= 0 ? "Você está no positivo!" : "Cuidado! Saldo negativo!");
}
