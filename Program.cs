﻿using Microsoft.EntityFrameworkCore;
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

Console.WriteLine("== SchoolDbLab ==");
Console.WriteLine("Console + API executando juntos!");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Escolha uma opção:");
    Console.WriteLine("1 - Cadastrar transação");
    Console.WriteLine("2 - Listar transações");
    Console.WriteLine("3 - Atualizar transação (por Id)");
    Console.WriteLine("4 - Remover transação (por Id)");
    Console.WriteLine("5 - Ver saldo atual");
    Console.WriteLine("6 - Relatório por categoria");
    Console.WriteLine("7 - Últimas transações");
    Console.WriteLine("0 - Sair");
    Console.Write("> ");

    var opt = Console.ReadLine();

    if (opt == "0") break;

    switch (opt)
    {
        case "1": await CreateStudentAsync(); break;
        case "2": await ListStudentsAsync(); break;
        case "3": await UpdateStudentAsync(); break;
        case "4": await DeleteStudentAsync(); break;
        default: Console.WriteLine("Opção inválida."); break;
    }
}

await app.StopAsync();
await webTask;

async Task CreateStudentAsync()
{
    Console.Write("Nome: ");
    var name = (Console.ReadLine() ?? "").Trim();

    Console.Write("Email: ");
    var email = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
    {
        Console.WriteLine("Nome e Email são obrigatórios.");
        return;
    }

    using var db = new AppDbContext();
    var exists = await db.Students.AnyAsync(s => s.Email == email);
    if (exists) { Console.WriteLine("Já existe um estudante com esse email."); return; }

    var student = new Student { Name = name, Email = email, EnrollmentDate = DateTime.UtcNow };
    db.Students.Add(student);
    await db.SaveChangesAsync();
    Console.WriteLine($"Cadastrado com sucesso! Id: {student.Id}");
}

async Task ListStudentsAsync()
{
    using var db = new AppDbContext();
    var students = await db.Students.OrderBy(s => s.Id).ToListAsync();

    if (students.Count == 0) { Console.WriteLine("Nenhum estudante encontrado."); return; }

    Console.WriteLine("Id | Name                 | Email                    | EnrollmentDate (UTC)");
    foreach (var s in students)
        Console.WriteLine($"{s.Id,2} | {s.Name,-20} | {s.Email,-24} | {s.EnrollmentDate:yyyy-MM-dd HH:mm:ss}");
}

async Task UpdateStudentAsync()
{
    Console.Write("Informe o Id do estudante a atualizar: ");
    if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Id inválido."); return; }

    using var db = new AppDbContext();
    var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
    if (student is null) { Console.WriteLine("Estudante não encontrado."); return; }

    Console.WriteLine($"Atualizando Id {student.Id}. Deixe em branco para manter.");
    Console.WriteLine($"Nome atual : {student.Name}");
    Console.Write("Novo nome  : ");
    var newName = (Console.ReadLine() ?? "").Trim();

    Console.WriteLine($"Email atual: {student.Email}");
    Console.Write("Novo email : ");
    var newEmail = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

    if (!string.IsNullOrWhiteSpace(newName)) student.Name = newName;
    if (!string.IsNullOrWhiteSpace(newEmail))
    {
        var emailTaken = await db.Students.AnyAsync(s => s.Email == newEmail && s.Id != id);
        if (emailTaken) { Console.WriteLine("Já existe outro estudante com esse email."); return; }
        student.Email = newEmail;
    }

    await db.SaveChangesAsync();
    Console.WriteLine("Estudante atualizado com sucesso.");
}

async Task DeleteStudentAsync()
{
    Console.Write("Informe o Id do estudante a remover: ");
    if (!int.TryParse(Console.ReadLine(), out var id)) { Console.WriteLine("Id inválido."); return; }

    using var db = new AppDbContext();
    var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
    if (student is null) { Console.WriteLine("Estudante não encontrado."); return; }

    db.Students.Remove(student);
    await db.SaveChangesAsync();
    Console.WriteLine("Estudante removido com sucesso.");

}
