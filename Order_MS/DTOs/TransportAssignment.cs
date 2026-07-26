namespace Order_MS.DTOs;

public class TransportAssignmentResponseDTO
{
    public int AssignmentId { get; set; }

    public int OrderReqId { get; set; }

    public string? Status { get; set; }
}
