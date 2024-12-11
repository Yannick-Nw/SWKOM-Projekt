using Application.Interfaces;
using Application.Interfaces.Files;
using AutoMapper;
using System.Diagnostics.CodeAnalysis;
using WebApi.Models;

namespace WebApi.Mappings;

[ExcludeFromCodeCoverage]
public class PaperlessProfile : Profile
{
    public PaperlessProfile()
    {
        // Map IFormFile to IFile
        CreateMap<IFormFile, IFile>()
            .ConvertUsing(src => new ApplicationFile(src));
    }
}
