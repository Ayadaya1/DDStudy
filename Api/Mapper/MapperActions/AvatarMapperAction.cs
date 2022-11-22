using Api.Models.Attaches;
using Api.Models.User;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperActions
{
    public class AvatarMapperAction : IMappingAction<User, UserModel>
    {
        private LinkGeneratorService _links;
        public AvatarMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _links = linkGeneratorService;
        }
        public void Process(User source, UserModel destination, ResolutionContext context) =>
            _links.FixAvatar(source, destination);

    }

    public class LikeableAvatarMapperAction : IMappingAction<Avatar, LikeableAvatarModel>
    {
        private LinkGeneratorService _links;
        public LikeableAvatarMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _links = linkGeneratorService;
        }
        public void Process(Avatar source, LikeableAvatarModel destination, ResolutionContext context) =>
            _links.FixLikeableAvatar(source, destination);

    }
}