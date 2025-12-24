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
    Task<bool> IsReaderHasBorrowedBooks(Guid readerId);
    
    /// <summary>
    /// Создать запись о передаче книги читателю
    /// </summary>
    /// <param name="readerId">ID карточки читателя</param>
    /// <param name="bookId">ID книги</param>
    Task<Guid> BorrowBook(Guid readerId, Guid bookId);
    
    /// <summary>
    /// Изменить статус взятия книги на возврат
    /// </summary>
    /// <param name="bookId">Книга</param>
    /// <returns></returns>
    Task ReturnBook(Guid bookId);
}