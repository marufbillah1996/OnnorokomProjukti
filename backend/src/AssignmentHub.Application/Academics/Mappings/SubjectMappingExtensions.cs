using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Mappings;

public static class SubjectMappingExtensions
{
    public static SubjectDto ToDto(this Subject s) => new(s.Id, s.Name, s.Code);
}
