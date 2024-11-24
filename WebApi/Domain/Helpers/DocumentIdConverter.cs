using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.Documents;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Helpers;

[ExcludeFromCodeCoverage]
public class DocumentIdConverter : JsonConverter<DocumentId>
{
    public override DocumentId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetGuid());
    public override void Write(Utf8JsonWriter writer, DocumentId value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value);
}
