using Common.Enums;

namespace Api.Models.User
{
    public class ChangePrivacySettingsModel
    {
        public Privacy AvatarAccess { get; set; }
        public Privacy PostAccess { get; set; }
        public Privacy MessageAccess { get; set; }
        public Privacy CommentAccess { get; set; }

        public bool Validate()
        {
            return Enum.IsDefined(typeof(Privacy), AvatarAccess)
                && Enum.IsDefined(typeof(Privacy), PostAccess)
                && Enum.IsDefined(typeof(Privacy), MessageAccess)
                && Enum.IsDefined(typeof(Privacy), CommentAccess);
        }
    }
}
