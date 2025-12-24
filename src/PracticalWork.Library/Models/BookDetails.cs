namespace PracticalWork.Library.Models;

public class BookDetails
{
    /// <summary>
    /// Краткое описание книги
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Путь к изображению обложки книги
    /// </summary>
    public string CoverImagePath { get; set; }
}