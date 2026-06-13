using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Baitaptuan5.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(50, ErrorMessage = "Tên danh mục không được vượt quá 50 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        // For Product CRUD (Lab 3/4)
        public List<Product>? Products { get; set; }
    }
}

