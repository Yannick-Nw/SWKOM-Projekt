using Application.Interfaces;
using Application.Services.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrWorker.Services.Ocr;
public interface IOcrProcessorService : IDisposable
{
    Task<string> ProcessAsync(IFile file, CancellationToken ct = default);
}
