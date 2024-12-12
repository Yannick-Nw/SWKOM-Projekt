using Application.Interfaces;
using Application.Interfaces.Files;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace OcrWorker.Services.Ocr;

public class TesseractOcrProcessorService : IOcrProcessorService
{
    public Task<string> ProcessAsync(IFile file, CancellationToken ct = default)
    {
        return file.ContentType switch
        {
            "application/pdf" => ProcessPdfAsync(file, ct),
            _ => throw new NotSupportedException($"Unsupported file type: {file.ContentType}")
        };
    }

    private async Task<string> ProcessPdfAsync(IFile file, CancellationToken ct = default)
    {
        using var images = new MagickImageCollection();
        using (var stream = await file.OpenAsync())
        {
            images.Read(stream);
            var maxFileSizeInBytes = 50 * 1024 * 1024; // 50MB limit
            if (stream.Length > maxFileSizeInBytes)
            {
                throw new InvalidOperationException($"File size exceeds the limit of {maxFileSizeInBytes} bytes.");
            }
        }

        var ocrText = new StringBuilder();
        var ocrEngine = new TesseractEngine(@"./tessdata", "eng+deu", EngineMode.Default);

        foreach (var page in images)
        {
            using var memoryStream = new MemoryStream();
            page.Format = MagickFormat.Png;
            page.Write(memoryStream); // Convert PDF page to image
            memoryStream.Position = 0;

            using var img = Pix.LoadFromMemory(memoryStream.ToArray());
            using var pageText = ocrEngine.Process(img);
            ocrText.Append(pageText.GetText());
        }

        var resultText = ocrText.ToString();

        return resultText;
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}