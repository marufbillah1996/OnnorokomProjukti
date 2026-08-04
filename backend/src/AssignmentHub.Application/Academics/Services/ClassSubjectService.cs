using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Application.Academics.Mappings;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Users.Interfaces;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Application.Academics.Services;

public class ClassSubjectService : IClassSubjectService
{
    private readonly IClassRepository _classRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IClassSubjectRepository _classSubjectRepository;
    private readonly IStudentClassRepository _studentClassRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ClassSubjectService(
        IClassRepository classRepository,
        ISubjectRepository subjectRepository,
        IClassSubjectRepository classSubjectRepository,
        IStudentClassRepository studentClassRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _classSubjectRepository = classSubjectRepository;
        _studentClassRepository = studentClassRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ClassSubjectDto> AssignTeacherAsync(Guid classId, Guid subjectId, Guid teacherId, CancellationToken cancellationToken = default)
    {
        _ = await _classRepository.GetByIdAsync(classId, cancellationToken)
            ?? throw new KeyNotFoundException("Class not found.");

        _ = await _subjectRepository.GetByIdAsync(subjectId, cancellationToken)
            ?? throw new KeyNotFoundException("Subject not found.");

        var teacher = await _userRepository.GetByIdAsync(teacherId, cancellationToken)
            ?? throw new KeyNotFoundException("Teacher not found.");

        if (teacher.Role != UserRole.Teacher)
        {
            throw new InvalidOperationException("The specified user is not a Teacher.");
        }

        var existing = await _classSubjectRepository.GetAsync(classId, subjectId, cancellationToken);

        if (existing is not null)
        {
            existing.TeacherId = teacherId;
            _classSubjectRepository.Update(existing);
        }
        else
        {
            var classSubject = new ClassSubject
            {
                ClassId = classId,
                SubjectId = subjectId,
                TeacherId = teacherId
            };

            await _classSubjectRepository.AddAsync(classSubject, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _classSubjectRepository.Query()
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .FirstAsync(x => x.ClassId == classId && x.SubjectId == subjectId, cancellationToken);

        return result.ToDto();
    }

    public async Task<IReadOnlyList<ClassSubjectDto>> GetForClassAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        // Materialize the tracked-navigation entities first — EF Core cannot translate a call
        // to our custom ToDto() extension method into SQL inside Select(), so the DTO mapping
        // happens in memory after the query executes.
        var entities = await _classSubjectRepository.Query()
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .Where(x => x.ClassId == classId)
            .ToListAsync(cancellationToken);

        return entities.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ClassSubjectDto>> GetForTeacherAsync(Guid teacherId, CancellationToken cancellationToken = default)
    {
        var entities = await _classSubjectRepository.Query()
            .Include(x => x.Class)
            .Include(x => x.Subject)
            .Include(x => x.Teacher)
            .Where(x => x.TeacherId == teacherId)
            .ToListAsync(cancellationToken);

        return entities.Select(x => x.ToDto()).ToList();
    }

    public async Task EnrollStudentAsync(Guid classId, Guid studentId, CancellationToken cancellationToken = default)
    {
        _ = await _classRepository.GetByIdAsync(classId, cancellationToken)
            ?? throw new KeyNotFoundException("Class not found.");

        var student = await _userRepository.GetByIdAsync(studentId, cancellationToken)
            ?? throw new KeyNotFoundException("Student not found.");

        if (student.Role != UserRole.Student)
        {
            throw new InvalidOperationException("The specified user is not a Student.");
        }

        if (await _studentClassRepository.ExistsAsync(studentId, classId, cancellationToken))
        {
            throw new InvalidOperationException("Student is already enrolled in this class.");
        }

        await _studentClassRepository.AddAsync(
            new StudentClass
            {
                StudentId = studentId,
                ClassId = classId,
                EnrolledAt = _dateTimeProvider.UtcNow
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
