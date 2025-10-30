// BookEntityExtensions.cs

using PracticalWork.Library.Data.PostgreSql.Entities;
using PracticalWork.Library.Enums;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Data.PostgreSql.Extenssions
{
    public static class BookEntityExtensions
    {
        public static Book ToBook(this AbstractBookEntity entity)
        {
            if (entity == null) return null;

            return new Book
            {
                Title = entity.Title,
                Authors = entity.Authors,
                Description = entity.Description,
                Year = entity.Year,
                Category = entity.GetBookCategory(),
                Status = entity.Status,
                CoverImagePath = entity.CoverImagePath,
                IsArchived = entity.Status == BookStatus.Archived
            };
        }

        public static AbstractBookEntity ToEntity(this Book book)
        {
            AbstractBookEntity entity = book.Category switch
            {
                BookCategory.ScientificBook => new ScientificBookEntity(),
                BookCategory.EducationalBook => new EducationalBookEntity(),
                BookCategory.FictionBook => new FictionBookEntity(),
                _ => throw new ArgumentException($"Неподдерживаемая категория книги: {book.Category}")
            };

            entity.Id = Guid.NewGuid(); // или из книги, если есть ID
            entity.Title = book.Title;
            entity.Authors = book.Authors;
            entity.Description = book.Description;
            entity.Year = book.Year;
            entity.Status = book.Status;
            entity.CoverImagePath = book.CoverImagePath;

            return entity;
        }

        private static BookCategory GetBookCategory(this AbstractBookEntity entity)
        {
            return entity switch
            {
                ScientificBookEntity => BookCategory.ScientificBook,
                EducationalBookEntity => BookCategory.EducationalBook,
                FictionBookEntity => BookCategory.FictionBook,
                _ => throw new InvalidOperationException($"Неизвестный тип entity: {entity.GetType()}")
            };
        }
    }
}