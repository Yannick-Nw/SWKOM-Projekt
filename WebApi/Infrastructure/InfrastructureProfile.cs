using AutoMapper;
using Domain.Entities;
using Domain.Entities.Documents;
using Infrastructure.Repositories.EfCore.Dbos;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

[ExcludeFromCodeCoverage]
public class InfrastructureProfile : Profile
{
    public InfrastructureProfile()
    {
        CreateMap<Document, DocumentDbo>()
            .ForMember(dbo => dbo.Id, opt => opt.MapFrom(doc => doc.Id.Value))
            .ForMember(dbo => dbo.UploadTime, opt => opt.MapFrom(doc => doc.UploadTime))
            .ForMember(dbo => dbo.FileName, opt => opt.MapFrom(doc => doc.Metadata.FileName))
            .ForMember(dbo => dbo.Title, opt => opt.MapFrom(doc => doc.Metadata.Title))
            .ForMember(dbo => dbo.Author, opt => opt.MapFrom(doc => doc.Metadata.Author));

        CreateMap<DocumentDbo, Document>()
            .ConvertUsing(dbo => new Document(new DocumentId(dbo.Id), dbo.UploadTime, new DocumentMetadata(dbo.FileName, dbo.Title, dbo.Author)));
    }
}
