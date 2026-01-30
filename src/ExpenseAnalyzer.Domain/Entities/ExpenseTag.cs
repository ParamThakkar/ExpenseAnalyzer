public class ExpenseTag
{
    public Guid ExpenseId { get; set; }
    public virtual Expense Expense { get; set; } = null!;

    public Guid TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;
}