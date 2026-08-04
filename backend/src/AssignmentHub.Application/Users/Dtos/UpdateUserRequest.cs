namespace AssignmentHub.Application.Users.Dtos;

public record UpdateUserRequest(string Name, string Email, string Role, bool IsActive);
