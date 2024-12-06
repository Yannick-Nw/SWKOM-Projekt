using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Documents;
public record DocumentMetadata(string FileName, string Title, string? Author);
