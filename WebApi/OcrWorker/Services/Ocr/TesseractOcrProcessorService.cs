using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OcrWorker.Services.Ocr;
internal class TesseractOcrProcessorService : IOcrProcessorService
{

    public Task<string> ProcessAsync(IFile file, CancellationToken ct = default)
    {
        var result = file.ContentType switch
        {
            "application/pdf" => $"It was a pdf file with name \"{file.Name}\"",
            _ => throw new NotSupportedException($"Unsupported file type: {file.ContentType}")
        };


        return Task.FromResult(result);
    }
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
