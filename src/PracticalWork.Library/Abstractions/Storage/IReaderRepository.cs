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
    Task<Reader> GetReaderById(Guid id);
    
    /// <summary>
    /// Проверка номера в базе данных
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <returns>Есть ли указанный номер в базе</returns>
    Task<bool> IsPhoneNumberExistsInDatabase(string phoneNumber);
}