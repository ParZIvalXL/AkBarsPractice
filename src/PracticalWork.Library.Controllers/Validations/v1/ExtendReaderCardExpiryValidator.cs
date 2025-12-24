using FluentValidation;
using PracticalWork.Library.Contracts.v1.Readers.Request;

namespace PracticalWork.Library.Controllers.Validations.v1;

public class ExtendReaderCardExpiryValidator : AbstractValidator<ExtendReaderCardRequest>
{
    public ExtendReaderCardExpiryValidator()
    {
        RuleFor(x => x.NewExpiryDate)
            .NotEmpty()
            .WithMessage("Дата истечения срока действия не может быть пустой")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Дата истечения срока действия должна быть в будущем");
    }
    
}