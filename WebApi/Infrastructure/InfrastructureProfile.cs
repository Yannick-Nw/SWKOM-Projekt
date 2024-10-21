using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories.EntityFrameworkCore.Dbos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        CreateMap<PaperlessDocument, PaperlessDocumentDbo>()
            .ForMember(dbo => dbo.Id, opt => opt.MapFrom(doc => doc.Id.Value))
            .ForMember(dbo => dbo.Path, opt => opt.MapFrom(doc => doc.Path))
            .ForMember(dbo => dbo.Size, opt => opt.MapFrom(doc => doc.Size))
            .ForMember(dbo => dbo.UploadTime, opt => opt.MapFrom(doc => doc.UploadTime))
            .ForMember(dbo => dbo.FileName, opt => opt.MapFrom(doc => doc.Metadata.FileName))
            .ForMember(dbo => dbo.Title, opt => opt.MapFrom(doc => doc.Metadata.Title))
            .ForMember(dbo => dbo.Author, opt => opt.MapFrom(doc => doc.Metadata.Author));

        CreateMap<PaperlessDocumentDbo, PaperlessDocument>()
            .ConvertUsing(dbo => new PaperlessDocument(new DocumentId(dbo.Id), dbo.Path, dbo.Size, dbo.UploadTime, new DocumentMetadata(dbo.FileName, dbo.Title, dbo.Author)));
    }
}
