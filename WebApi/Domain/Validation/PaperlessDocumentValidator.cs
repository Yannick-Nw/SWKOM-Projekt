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
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Path).NotEmpty();
        RuleFor(x => x.Size).GreaterThan(0);
        RuleFor(x => x.UploadTime).LessThanOrEqualTo(DateTimeOffset.Now);
        RuleFor(x => x.Metadata)
            .NotNull()
            .SetValidator(new DocumentMetadataValidator());
    }
}
