namespace AssignmentHub.Application.Users.Dtos;

public record UserDto(Guid Id, string Name, string Email, string Role, bool IsActive, DateTime CreatedAt);
