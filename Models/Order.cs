using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Baitaptuan5.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [ValidateNever]
        public string UserId { get; set; } = null!;
        
        public DateTime OrderDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = null!;
        
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; } = null!;
        
        [ValidateNever]
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
