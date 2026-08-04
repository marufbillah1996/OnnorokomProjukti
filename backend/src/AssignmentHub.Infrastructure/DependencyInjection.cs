using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Application.Assignments.Interfaces;
using AssignmentHub.Application.Auth.Interfaces;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Notifications.Interfaces;
using AssignmentHub.Application.Settings.Interfaces;
using AssignmentHub.Application.Submissions.Interfaces;
using AssignmentHub.Application.Users.Interfaces;
using AssignmentHub.Infrastructure.Authorization;
using AssignmentHub.Infrastructure.Identity;
using AssignmentHub.Infrastructure.Persistence;
using AssignmentHub.Infrastructure.Persistence.Repositories;
using AssignmentHub.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddHttpContextAccessor();
        services.AddSignalR();
        services.AddMemoryCache();

        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IClassSubjectRepository, ClassSubjectRepository>();
        services.AddScoped<IStudentClassRepository, StudentClassRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IGradeAuditLogRepository, GradeAuditLogRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();

        services.AddScoped<IAuthorizationHandler, AssignmentOwnershipHandler>();

        return services;
    }
}
