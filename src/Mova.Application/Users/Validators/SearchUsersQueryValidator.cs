using System.Text.RegularExpressions;
using FluentValidation;
using Mova.Application.Users.Queries;

namespace Mova.Application.Users.Validators;

public sealed class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    private const int MinimumSearchLength = 2;
    private const int MaximumSearchLength = 100;

    private static readonly Regex AllowedSearchCharacters = new(
        @"^[a-zA-Z0-9\s\.\+\-_@()]+$",
        RegexOptions.Compiled);

    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.SportsComplexId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);

        RuleFor(x => x.Search)
            .MinimumLength(MinimumSearchLength)
            .WithMessage($"Search query must be at least {MinimumSearchLength} characters.")
            .MaximumLength(MaximumSearchLength)
            .WithMessage($"Search query must not exceed {MaximumSearchLength} characters.")
            .Must(ContainOnlyAllowedCharacters)
            .WithMessage("Search query contains invalid characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }

    private static bool ContainOnlyAllowedCharacters(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        return AllowedSearchCharacters.IsMatch(search.Trim());
    }
}
