using Application.Interfaces.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrWorker.Services.Ocr;
public class FakeOcrProcessorService : IOcrProcessorService
{
    public Task<string> ProcessAsync(IFile file, CancellationToken ct = default)
    {
        return Task.FromResult("Fake OCR result and random num: " + new Random().Next());
    }
    public void Dispose()
    {

    }
}
