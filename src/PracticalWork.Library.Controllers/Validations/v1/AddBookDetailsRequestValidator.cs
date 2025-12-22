using FluentValidation;
using PracticalWork.Library.Contracts.v1.Books.Request;

namespace PracticalWork.Library.Controllers.Validations.v1;

public sealed class AddBookDetailsRequestValidator : AbstractValidator<AddBookDetailsRequest>
{
    public AddBookDetailsRequestValidator()
    {
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов.")
            .When(x => !string.IsNullOrEmpty(x.Description));
        
        RuleFor(x => x.Photo)
            .NotNull().WithMessage("Фото не может быть пустым.")
            .When(x => x.Photo != null);
    }
}