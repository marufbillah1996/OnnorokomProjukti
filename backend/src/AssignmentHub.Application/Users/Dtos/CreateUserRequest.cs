namespace AssignmentHub.Application.Users.Dtos;

public record CreateUserRequest(string Name, string Email, string Password, string Role);
