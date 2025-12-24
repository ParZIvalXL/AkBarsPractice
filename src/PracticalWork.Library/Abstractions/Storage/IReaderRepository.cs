using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Storage;

public interface IReaderRepository
{
    /// <summary>
    /// Создать карточку читателя
    /// </summary>
    /// <param name="reader">Данные читателя</param>
    /// <returns>ID созданной карточки</returns>
    Task<Guid> CreateReader(Reader reader);
    
    /// <summary>
    /// Обновить данные читателя
    /// </summary>
    /// <param name="id">ID целевой карточки</param>
    /// <param name="reader">Новые данные</param>
    Task UpdateReader(Guid id, Reader reader);
    
    /// <summary>
    /// Получить данные читателя
    /// </summary>
    /// <param name="id">ID карточки</param>
    /// <returns>Данные карточки</returns>
    Reader GetReaderById(Guid id);
    
    /// <summary>
    /// Получить список карточек читателей
    /// </summary>
    /// <param name="readersPerPage">Количество карточек на странице</param>
    /// <param name="page">Страница</param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<List<Reader>> GetReaders(int? readersPerPage, int? page, string name = null);
    
    /// <summary>
    /// Проверка номера в базе данных
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <returns>Есть ли указанный номер в базе</returns>
    bool IsPhoneNumberExistsInDatabase(string phoneNumber);
}