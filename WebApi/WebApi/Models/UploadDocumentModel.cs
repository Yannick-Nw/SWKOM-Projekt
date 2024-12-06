using System.ComponentModel.DataAnnotations;

namespace WebApi.Models;

public record UploadDocumentModel([Required] IFormFile File, string FileName, string? Title = null, string? Author = null);