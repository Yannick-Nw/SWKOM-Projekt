using FluentValidation;
using FluentValidation.Validators;

namespace Domain.Entities.Documents;
public class DocumentMetadataValidator : AbstractValidator<DocumentMetadata>
{
    public DocumentMetadataValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().WithMessage("FileName must not be empty.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title must not be empty.");
    }
}
