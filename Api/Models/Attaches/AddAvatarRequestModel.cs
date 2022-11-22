namespace Api.Models.Attaches
{
    public class AddAvatarRequestModel
    {
        public MetadataModel Avatar { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}
