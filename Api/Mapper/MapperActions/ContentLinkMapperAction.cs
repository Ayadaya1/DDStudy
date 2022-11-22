using Api.Models.Attaches;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperActions
{
    public class ContentLinkMapperAction : IMappingAction<PostAttach, AttachExternalModel>
    {
        private LinkGeneratorService _links;
        public ContentLinkMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _links = linkGeneratorService;
        }
        public void Process(PostAttach source, AttachExternalModel destination, ResolutionContext context)
            => _links.FixContent(source, destination);
    }
}