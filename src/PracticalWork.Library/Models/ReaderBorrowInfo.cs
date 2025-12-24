namespace PracticalWork.Library.Models;

public class ReaderBorrowInfo
{
    /// <summary>
    /// ID Книги
    /// </summary>
    public Guid BookId { get; set; }
    
    /// <summary>
    /// Дата получения книги
    /// </summary>
    public DateTime BorrowDate { get; set; }
    
    /// <summary>
    /// Срок сдачи книги
    /// </summary>
    public DateTime DueDate { get; set; }
    public bool IsExtended { get; set; }
}