using Domain.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Validation;
public class PaperlessDocumentValidator : AbstractValidator<PaperlessDocument>
{
    public PaperlessDocumentValidator()
    {
        // Add messages
        RuleFor(x => x.Id).NotNull().WithMessage("Document ID cannot be null.");
        RuleFor(x => x.Path).NotEmpty().WithMessage("Document path cannot be empty.");
        RuleFor(x => x.Size).GreaterThan(0).WithMessage("Document size must be greater than zero.");
        RuleFor(x => x.UploadTime).LessThanOrEqualTo(DateTimeOffset.Now).WithMessage("Upload time cannot be in the future.");
        RuleFor(x => x.Metadata)
            .NotNull().WithMessage("Document metadata cannot be null.")
            .SetValidator(new DocumentMetadataValidator());
    }
}
