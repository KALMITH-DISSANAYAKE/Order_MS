using System.ComponentModel.DataAnnotations;

namespace Order_MS.DTOs
{
    public class OrderUpdateDtos
    {
        [Required]
        public string? OrderRemark { get; set; }
    }
}
