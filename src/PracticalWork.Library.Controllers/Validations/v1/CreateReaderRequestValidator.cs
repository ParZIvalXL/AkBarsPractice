using System.Text.RegularExpressions;
using FluentValidation;
using PracticalWork.Library.Contracts.v1.Readers.Request;

namespace PracticalWork.Library.Controllers.Validations.v1;

public class CreateReaderRequestValidator : AbstractValidator<CreateReaderRequest>
{
    public CreateReaderRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("ФИО не может быть пустым")
            .MaximumLength(200)
            .WithMessage("ФИО не может превышать 200 символов")
            .MinimumLength(5)
            .WithMessage("ФИО не может быть короче 5 символов");
        
        RuleFor(x => x.ExpiryDate)
            .NotEmpty()
            .WithMessage("Дата истечения срока действия не может быть пустой")
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Дата истечения срока действия должна быть в будущем");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .NotNull()
            .WithMessage("Номер телефона обязателен")
            .MinimumLength(10)
            .WithMessage("Длинна номера должна быть больше 10 символов")
            .MaximumLength(20)
            .WithMessage("Длинна номера должна быть меньше 20 символов")
            .Matches(new Regex(@"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$"))
            .WithMessage("Неправильный номер");
    }
    
}