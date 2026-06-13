using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Baitaptuan5.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal Price { get; set; }

        [ValidateNever]
        public Order Order { get; set; } = null!;
        
        [ValidateNever]
        public Product Product { get; set; } = null!;
    }
}
