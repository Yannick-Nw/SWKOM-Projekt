using Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities.Documents;

[JsonConverter(typeof(DocumentIdConverter))]
public record DocumentId(Guid Value)
{
    public static DocumentId New() => new(Guid.NewGuid());
}
