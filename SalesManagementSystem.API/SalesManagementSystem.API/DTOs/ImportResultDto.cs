namespace SalesManagementSystem.API.DTOs;

public class ImportResultDto
{
    public int TotalRows { get; set; }

    public int Imported { get; set; }

    public int Failed { get; set; }

    public List<string> Errors { get; set; } = new();
}