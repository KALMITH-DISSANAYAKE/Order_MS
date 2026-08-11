namespace Order_MS.DTOs;

public class BranchDto
{
    public int BranchId { get; set; }
    public string BranchCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

// NEW: What user sends when creating a branch
public class CreateBranchDto
{
    public string BranchCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

// NEW: What user sends when updating a branch
public class UpdateBranchDto
{
    public string BranchCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}
