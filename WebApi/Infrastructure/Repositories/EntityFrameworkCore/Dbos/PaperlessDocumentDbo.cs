using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Repositories.EntityFrameworkCore.Dbos;

public class PaperlessDocumentDbo
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Path { get; set; } = null!;

    [Required]
    public long Size { get; set; }

    [Required]
    public DateTimeOffset UploadTime { get; set; }

    [Required]
    public string FileName { get; set; } = null!;

    [Required]
    public string Title { get; set; } = null!;

    public string? Author { get; set; }
}
