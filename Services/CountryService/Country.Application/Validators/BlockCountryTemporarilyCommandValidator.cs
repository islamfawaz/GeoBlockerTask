using Country.Application.Commands;
using FluentValidation;

namespace Country.Application.Validators
{

    public sealed class BlockCountryTemporarilyCommandValidators
        : AbstractValidator<BlockCountryTemporarilyCommand>
    {
        public BlockCountryTemporarilyCommandValidators()
        {
            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage("Country code is required")
                .Length(2).WithMessage("Country code must be 2 characters")
                .Matches("^[A-Z]{2}$").WithMessage("Country code must be 2 uppercase letters");

            RuleFor(x => x.CountryName)
                .NotEmpty().WithMessage("Country name is required")
                .MaximumLength(100).WithMessage("Country name must not exceed 100 characters");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0")
                .LessThanOrEqualTo(1440).WithMessage("Duration must not exceed 24 hours (1440 minutes)");
        }
    }
}