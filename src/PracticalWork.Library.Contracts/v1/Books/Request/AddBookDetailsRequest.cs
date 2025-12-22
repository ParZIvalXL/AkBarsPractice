using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PracticalWork.Library.Contracts.v1.Books.Request;

/// <summary>
/// Запрос на добавления деталей по книге
/// </summary>
/// <param name="Description">Краткое описание книги</param>
/// <param name="Photo">Фото обложки книги</param>
public sealed record AddBookDetailsRequest(string Description, IFormFile Photo);