namespace Controle.Models;

public class Transaction{
    public int Id {get;set;}
    public string Descricao { get; set; } = "";
    public decimal Valor { get; set; } = 0;
    public string Tipo { get; set; } = "";
    public string Categoria { get; set; } = "";
    public DateTime Data {get;set;} = DateTime.UtcNow;
}