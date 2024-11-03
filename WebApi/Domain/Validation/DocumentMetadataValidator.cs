using Domain.Entities;
using FluentValidation;
using FluentValidation.Validators;

namespace Domain.Validation;
public class DocumentMetadataValidator : AbstractValidator<DocumentMetadata>
{
    public DocumentMetadataValidator()
    {
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Author).NotEmpty();
    }
}