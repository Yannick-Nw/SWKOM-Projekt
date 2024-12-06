using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Repositories.EfCore.Dbos;

public class DocumentDbo
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DateTimeOffset UploadTime { get; set; }

    [Required]
    public string FileName { get; set; } = null!;

    [Required]
    public string Title { get; set; } = null!;

    public string? Author { get; set; }
}
