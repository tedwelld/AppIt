namespace AppIt.Core.DTOs
{
    public record CreateUserProfileDto(string DisplayName);

    public record UpdateUserProfileDto(int Id, string DisplayName, bool IsActive);

    public record UserProfileReadDto(
        int Id,
        string DisplayName,
        bool IsActive
    );
}
