using System.ComponentModel.DataAnnotations;

namespace aspnetserver.Data.Dtos;

public record ZoneDto(int Id, string Name);
public record CreateZoneDto([Required] string Name);
public record UpdateZoneDto(string? Name);