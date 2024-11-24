using Application.Interfaces;
using Domain.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Documents;
public record DocumentFile(DocumentId Id, IFile File);
