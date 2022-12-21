using AutoMapper;
using Common;
using Api.Mapper.MapperActions;
using Api.Models.Posts;
using Api.Models.Attaches;
using Api.Models.User;
using Api.Models.Comments;
using DAL.Entities;

namespace Api.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                .ForMember(d => d.PrivacySettings, m => m.MapFrom(s => new PrivacySettings()));

            CreateMap<User, UserModel>()
                .ForMember(d => d.subscriptionCount, m => m.MapFrom(s => s.Subscriptions.Count))
                .ForMember(d => d.subscriberCount, m => m.MapFrom(s => s.Subscribers.Count))
                .AfterMap<AvatarMapperAction>();

            CreateMap<Avatar, AttachModel>();

            CreateMap<PostAttach, AttachModel>();

            CreateMap<ChangePrivacySettingsModel, PrivacySettings>();

            CreateMap<Post, ILikeable>();

            CreateMap<Comment, ILikeable>();

            CreateMap<Avatar, ILikeable>();

            CreateMap<Attach, AttachModel>();

            CreateMap<MetadataModel, PostAttach>();

            CreateMap<Post, PostModel>()
                .ForMember(d => d.Likes, m => m.MapFrom(s => s.Likes.Count))
                .ForMember(d => d.Comments, m => m.MapFrom(s => s.Comments.Count))
                .ForMember(d => d.Attaches, m => m.MapFrom(s => s.Attaches))
                .ForMember(d=>d.Id, m=>m.MapFrom(s=>s.Id));

            CreateMap<Avatar, LikeableAvatarModel>()
                .ForMember(d => d.Likes, m => m.MapFrom(s => s.Likes.Count))
                .ForMember(d => d.User, m => m.MapFrom(s => s.User))
                .AfterMap<LikeableAvatarMapperAction>();

            CreateMap<PostAttach, AttachExternalModel>().AfterMap<ContentLinkMapperAction>();

            CreateMap<Comment, CommentOutputModel>()
                .ForMember(d => d.Author, m => m.MapFrom(s => s.User))
                .ForMember(d=>d.Likes,m=>m.MapFrom(s=>s.Likes.Count));


        }
    }
}
