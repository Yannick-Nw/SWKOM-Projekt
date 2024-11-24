namespace OcrWorker;

public class OcrService
{
    public string PerformOcr(string filePath)
    {
        using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        using var image = Pix.LoadFromFile(filePath);
        using var page = engine.Process(image);
        return page.GetText();
    }
}