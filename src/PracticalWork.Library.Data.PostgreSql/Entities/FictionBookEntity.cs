using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Entities;

/// <summary>
/// Художественная литература
/// </summary>
public sealed class FictionBookEntity : AbstractBookEntity
{
    /// <summary>Категория художественной прозы</summary>
    public string CategoriesOfFiction { get; set; }
}