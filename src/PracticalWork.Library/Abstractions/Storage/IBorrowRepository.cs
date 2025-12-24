using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IBorrowRepository
{
    /// <summary>
    /// Получить список книг читателя
    /// </summary>
    /// <param name="readerId">ID карточки читателя</param>
    /// <returns>Список книг</returns>
    Task<List<Guid>> GetReaderBorrowedBooks(Guid readerId);
    
    /// <summary>
    /// Проверка книг читателя
    /// </summary>
    /// <param name="readerId">ID карточки
    /// </param>
    /// <returns>
    /// Есть ли у читателя книги
    /// </returns>
    bool IsReaderHasBorrowedBooks(Guid readerId);
}