using FluentValidation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Files;

[ExcludeFromCodeCoverage]
public class PdfFileValidator : FileValidator
{
    public PdfFileValidator() : base("application/pdf") {
    }
}

[ExcludeFromCodeCoverage]
public class FileValidator : AbstractValidator<IFile>
{
    public FileValidator(string contentType)
    {
        RuleFor(x => x.ContentType).Equal(contentType, StringComparer.OrdinalIgnoreCase);
    }
}
