using MediaHub.Domain.Common;

namespace MediaHub.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<UserMediaItem> Library { get; set; } = new List<UserMediaItem>();
}