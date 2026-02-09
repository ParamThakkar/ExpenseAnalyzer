public class Expense
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Comment { get; set; }
    public virtual ICollection<ExpenseTag> ExpenseTags { get; set; } = new List<ExpenseTag>();
}