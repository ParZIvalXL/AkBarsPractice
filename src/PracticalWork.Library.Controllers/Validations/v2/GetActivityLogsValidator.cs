using FluentValidation;
using PracticalWork.Library.Contracts.v2.Requests;

namespace PracticalWork.Library.Controllers.Validations.v2;

public sealed class GetActivityLogsRequestValidator
    : AbstractValidator<GetActivityLogsRequest>
{
    public GetActivityLogsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Номер страницы должен быть больше 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Размер страницы должен быть от 1 до 100");

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("Дата начала не может быть больше даты окончания");
    }
}