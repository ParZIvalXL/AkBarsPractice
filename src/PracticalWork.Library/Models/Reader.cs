namespace PracticalWork.Library.Models;

/// <summary>
/// Карточка читателя
/// </summary>
public sealed class Reader
{ 
    /// <summary>
    /// ФИО читателя
    /// </summary>
    public string FullName { get; set; }
    
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; set; }
    
    /// <summary>
    /// Дата истечения срока действия
    /// </summary>
    public DateTime ExpiryDate { get; set; }
    
    /// <summary>
    /// Активность карточки
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Дата создания карточки
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата обновления карточки
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}