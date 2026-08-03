using System.ComponentModel.DataAnnotations;

namespace Order_MS.DTOs;

public class CreateOrderRequestDTO
{
    [Range(1, int.MaxValue,
    ErrorMessage = "RequestedBy must be greater than 0.")]
    public int RequestedBy { get; set; }

    [Required(ErrorMessage = "Items are required.")]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<OrderRequestLineDTO> Items { get; set; } = new();
}

public class OrderRequestLineDTO
{
    [Range(1, int.MaxValue,
    ErrorMessage = "ItemId must be greater than 0.")]
    public int ItemId { get; set; }

    [Range(1, int.MaxValue,
    ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }
}


public class OrderRequestLineResponseDTO
{
    public int ItemId { get; set; }

    public string ItemName { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

}


public class OrderRequestResponseDTO
{
    public int OrderReqId { get; set; }

    public string Status { get; set; }

    public int TotalQuantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime RequestedOn { get; set; }

    public List<OrderRequestLineResponseDTO> Items { get; set; }
}

public class OrderRequestListDTO
{
    public int OrderReqId { get; set; }

    public string Status { get; set; }

    public int TotalQuantity { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime RequestedOn { get; set; }

    public string RequestedBy { get; set; }
}

public class ApproveOrderRequestDTO
{
    [Range(1, int.MaxValue,
        ErrorMessage = "ApprovedBy must be greater than 0.")]
    public int ApprovedBy { get; set; }
}