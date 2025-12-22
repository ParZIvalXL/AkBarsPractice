using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

public interface IReaderService
{
    /// <summary>
    /// Создает карточку читателя
    /// </summary>
    /// <param name="reader">Данные читателя</param>
    /// <returns>Индификатор карточки</returns>
    Task<Guid> CreateReader(Reader reader);
    
    /// <summary>
    /// Продление каточки
    /// </summary>
    /// <param name="id">ID карточки читателя</param>
    /// <param name="newExpiryDate">Новая дата</param>
    /// <returns></returns>
    Task ExtendCard(Guid id, DateOnly newExpiryDate);
    
    /// <summary>
    /// Закрытие карточки
    /// </summary>
    /// <param name="id">ID карточки читателя</param>
    /// <returns></returns>
    Task CloseCard(Guid id);
    
    /// <summary>
    /// Получение списка ID взятых книг читателя
    /// </summary>
    /// <param name="id">ID карточки читателя</param>
    /// <returns>Список ID книг</returns>
    Task<List<Guid>> GetReaderBooksIds(Guid id);
    
    /// <summary>
    /// Получение списка взятых книг читателя
    /// </summary>
    /// <param name="id">ID карточки читателя</param>
    /// <returns>Список книг</returns>
    Task<List<Book>> GetReaderBooks(Guid id);
    
    /// <summary>
    /// Проверка номера в базе
    /// </summary>
    /// <param name="number">Номер читателя</param>
    /// <returns>Существует ли номер в базе</returns>
    bool IsNumberExistsInDatabase(string number);

}