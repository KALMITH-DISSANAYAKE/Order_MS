namespace Order_MS.DTOs;

public class CreateOrderRequestDTO
{
    public int RequestedBy { get; set; }

    public List<OrderRequestLineDTO> Items { get; set; }
}

public class OrderRequestLineDTO
{
    public int ItemId { get; set; }
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
    public int ApprovedBy { get; set; }
}