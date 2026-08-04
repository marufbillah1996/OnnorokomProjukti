using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Academics.Mappings;

public static class ClassMappingExtensions
{
    public static ClassDto ToDto(this Class c) => new(c.Id, c.Name, c.Description);
}
