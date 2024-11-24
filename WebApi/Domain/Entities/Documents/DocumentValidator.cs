using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Documents;
public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.UploadTime).LessThanOrEqualTo(DateTimeOffset.Now);
        RuleFor(x => x.Metadata)
            .NotNull()
            .SetValidator(new DocumentMetadataValidator());
    }
}
