using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Order_MS.DTOs;

public class OrderCreateDtos
{
    [Required]
    public int? OrderReqId { get; set; }
    [Required]
    public int? ConnectionId { get; set; }
    [Required]
    public decimal? Price { get; set; }
    [Required]
    public string? OrderStatus { get; set; }
    [Required]
    public int? CreatedBy { get; set; }
    [Required]
    public DateTime? CreatedOn { get; set; }
    [Required]
    public int? ModifiedBy { get; set; }
    [Required]
    public DateTime? ModifiedOn { get; set; }
    public string? OrderRemark { get; set; }
}