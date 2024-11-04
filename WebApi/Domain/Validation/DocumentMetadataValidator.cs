using Domain.Entities;
using FluentValidation;
using FluentValidation.Validators;

namespace Domain.Validation;
public class DocumentMetadataValidator : AbstractValidator<DocumentMetadata>
{
    public DocumentMetadataValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name must not be empty.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title must not be empty.");
        RuleFor(x => x.Author).NotEmpty().WithMessage("Author must not be empty.");
    }
}
