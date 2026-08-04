using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Users.Dtos;
using AssignmentHub.Application.Users.Interfaces;
using AssignmentHub.Application.Users.Mappings;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;

namespace AssignmentHub.Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PaginatedList<UserDto>> GetAllAsync(PaginationParams pagination, string? roleFilter, CancellationToken cancellationToken = default)
    {
        var query = _userRepository.Query();

        if (!string.IsNullOrWhiteSpace(roleFilter) && Enum.TryParse<UserRole>(roleFilter, true, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole);
        }

        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            // Portable case-insensitive search (ToLower on both sides) rather than
            // EF.Functions.ILike, so this bounded context isn't coupled to Postgres.
            var search = pagination.Search.ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }

        query = pagination.SortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => pagination.IsDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
            "email" => pagination.IsDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "createdat" => pagination.IsDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            _ => pagination.IsDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var paginated = await PaginatedList<User>.CreateAsync(query, pagination.Page, pagination.PageSize, cancellationToken);
        return paginated.Map(u => u.ToDto());
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{id}' was not found.");

        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsWithEmailAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new InvalidOperationException("Invalid role.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{id}' was not found.");

        if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && await _userRepository.ExistsWithEmailAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new InvalidOperationException("Invalid role.");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Role = role;
        user.IsActive = request.IsActive;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{id}' was not found.");

        user.IsDeleted = true;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
