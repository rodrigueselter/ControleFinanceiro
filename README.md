# 💰 Controle Financeiro - API REST (.NET 9.0 + EF Core + SQLite)

Projeto desenvolvido para a disciplina **Desenvolvimento de Sistemas**, com o objetivo de criar uma **API REST** para controle financeiro pessoal.
A API permite **CRUD de transações (receitas e despesas)** e fornece um **endpoint de resumo** com totais de receitas e despesas e o saldo.

---

## ⚙️ Configuração do Projeto
**Comandos Necessários para Configuração do Projeto**
Abaixo estão todos os comandos utilizados para configurar o ambiente, instalar dependências, criar o banco de dados e executar a aplicação.


Instalar pacotes do Entity Framework Core

`dotnet add package Microsoft.EntityFrameworkCore`

`dotnet add package Microsoft.EntityFrameworkCore.Sqlite`

`dotnet add package Microsoft.EntityFrameworkCore.Design`

Esses pacotes permitem o uso do Entity Framework Core e do banco de dados SQLite no projeto.

**Criar e Atualizar o Banco de Dados (rodar somente uma vez)**

`dotnet ef migrations add InitialCreate`

`dotnet ef database update`

O primeiro comando cria a migração inicial com as tabelas.

O segundo aplica a migração e gera o arquivo do banco Controle.db.



**Adicionar biblioteca para documentação Swagger**

`dotnet add package Swashbuckle.AspNetCore`

Permite habilitar o Swagger, que gera uma interface visual para testar e documentar a API.



**Executar o Projeto**

`dotnet run`

Inicia a API.

Se estiver configurado, o Swagger ficará disponível em:
http://localhost:5099/swagger



Principais tecnologias:
- ASP.NET Core Web API
- EF Core 9 com provedor SQLite
- Ferramenta `dotnet-ef` configurada em `.config/dotnet-tools.json`



- **Models**: classes de domínio (ex.: `Transaction`).
- **Data**: `AppDbContext` com mapeamentos e provedor de banco.
- **Controllers**: endpoints REST (ex.: `TransactionsController`).
- **Migrations**: histórico de migrações do EF Core.
- **Program.cs**: bootstrap da aplicação (DI, rotas, DbContext, etc.).



### `Transaction`
Campos (conforme `Models/Transaction.cs` e `AppDbContext`):

| Campo      | Tipo .NET | Banco/Restrição                          | Observações                         |
|------------|-----------|------------------------------------------|-------------------------------------|
| `Id`       | int       | PK                                       | Identificador da transação          |
| `Descricao`| string    | `IsRequired()`, `HasMaxLength(100)`      | Descrição curta                     |
| `Valor`    | decimal   | `IsRequired()`, `decimal(10,2)`          | Valor monetário                     |
| `Tipo`     | string    | `IsRequired()`, `HasMaxLength(20)`       | **"Receita"** ou **"Despesa"**      |
| `Categoria`| string    | `IsRequired()`, `HasMaxLength(20)`       | Ex.: Alimentação, Salário, etc.     |
| `Data`     | DateTime  | `IsRequired()`, `DEFAULT CURRENT_TIMESTAMP` | UTC por padrão                     |

### Pré‑requisitos
- .NET SDK 9.0+ instalado.
- (Opcional) EF Core CLI: será restaurado via `dotnet tool restore`.

### Passos
1. **Restaurar ferramentas e dependências**
   ```bash
   dotnet tool restore
   dotnet restore
   ```

2. **Aplicar migrações (criar `Controle.db`)**
   ```bash
   cd Controle
   dotnet ef database update
   ```

3. **Executar a API**
   ```bash
   dotnet run --project Controle
   ```
   A aplicação inicia por padrão em **http://localhost:5099** (definido em `Program.cs`).

---

## 📁 Estrutura do Projeto

```plaintext
ControleFinanceiro/
├── Controllers/
│   └── TransactionsController.cs
├── Models/
│   └── Transaction.cs
├── Data/
│   └── AppDbContext.cs
├── Program.cs
└── ControleFinanceiro.csproj
```
---

## 🔗 Endpoints

Base route do controller: `api/v1/[controller]` → resolve para `api/v1/Transactions`.

### 1) Listar transações
**GET** `/api/v1/Transactions`  
Retorna a lista completa de transações.

Exemplo cURL:
```bash
curl -X GET http://localhost:5099/api/v1/Transactions
```

### 2) Obter por ID
**GET** `/api/v1/Transactions/{id}`  
Retorna uma transação específica.

```bash
curl -X GET http://localhost:5099/api/v1/Transactions/1
```

### 3) Criar transação
**POST** `/api/v1/Transactions`  
Body (JSON):
```json
{
  "descricao": "Salário",
  "valor": 4500.00,
  "tipo": "Receita",
  "categoria": "Trabalho"
}
```

```bash
curl -X POST http://localhost:5099/api/v1/Transactions   -H "Content-Type: application/json"   -d '{"descricao":"Salário","valor":4500.00,"tipo":"Receita","categoria":"Trabalho"}'
```

### 4) Atualizar transação
**PUT** `/api/v1/Transactions/{id}`  
Body (JSON):
```json
{
  "descricao": "Salário",
  "valor": 4500.00,
  "tipo": "Receita",
  "categoria": "Trabalho"
}
```

```bash
curl -X PUT http://localhost:5099/api/v1/Transactions/1   -H "Content-Type: application/json"   -d '{"descricao":"Salário Outubro","valor":4600.00,"tipo":"Receita","categoria":"Trabalho"}'
```

### 5) Remover transação
**DELETE** `/api/v1/Transactions/{id}`

```bash
curl -X DELETE http://localhost:5099/api/v1/Transactions/1
```

### 6) Resumo financeiro
**GET** `/api/v1/Transactions/saldo`  
Calcula totais e saldo com base nos registros. O controller retorna algo no formato:

```json
{
  "receitas": 5000.00,
  "despesas": 3200.50,
  "saldo": 1799.50,
  "status": "Positivo" // ou "Negativo", caso as despesas sejam maiores que as receitas
}
```

```bash
curl -X GET http://localhost:5099/api/v1/Transactions/saldo
```
---

## 🧠 Regras de Negócio e Validações
- `Tipo` deve ser **"Receita"** ou **"Despesa"** (padronize na camada de serviço/validação).
- `Descricao`, `Tipo` e `Categoria` são obrigatórios e com limites de tamanho (100/20/20).
- `Valor` deve ser **>= 0**.
- `Data` padrão em UTC; o banco usa `CURRENT_TIMESTAMP` como default.

## 🪵 Logs
- Utilize o `ILogger` (injeção no controller) para registrar eventos de CRUD e cálculos do resumo.
- Em produção, considere um provider persistente (Serilog, Seq, etc.).

## 📦 Dependências (do `.csproj`)
- Microsoft.EntityFrameworkCore 9.0.10
- Microsoft.EntityFrameworkCore.Design 9.0.10
- Microsoft.EntityFrameworkCore.Sqlite 9.0.10
- Swashbuckle.AspNetCore 9.0.6

## 👥 Autores
- Elter Rodrigues
- Kauanne Julia

## 🧾 Licença
Uso acadêmico. Todos os direitos reservados aos autores.
