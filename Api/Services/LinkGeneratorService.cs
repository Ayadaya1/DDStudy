using Api.Models.Attaches;
using Api.Models.User;
using DAL.Entities;

namespace Api.Services
{
    public class LinkGeneratorService
    {
        public Func<PostAttach, string?>? LinkContentGenerator;
        public Func<User, string?>? LinkAvatarGenerator;
        public Func<Avatar, string?>? LinkLikeableAvatarGenerator;

        public void FixAvatar(User s, UserModel d)
        {
            d.Avatar = s.Avatar == null ? null : LinkAvatarGenerator?.Invoke(s);
        }
        public void FixContent(PostAttach s, AttachExternalModel d)
        {
            d.ContentLink = LinkContentGenerator?.Invoke(s);
        }
        public void FixLikeableAvatar(Avatar s, LikeableAvatarModel d)
        {
            d.Avatar = s == null ? null : LinkLikeableAvatarGenerator?.Invoke(s);
        }
    }
}