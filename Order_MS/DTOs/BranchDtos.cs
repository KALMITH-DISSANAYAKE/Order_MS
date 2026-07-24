namespace Order_MS.DTOs;

public class BranchDto
{
    public int BranchId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}