using System;
using System.ComponentModel.DataAnnotations;

namespace API.Dtos;

public class CreateProductDto
{
          [Required]
          public string Name { get; set; } = string.Empty;
          [Required]
          public string Description { get; set; } = string.Empty;
          [Range(0.01, double.MaxValue,ErrorMessage ="Price Value is 1")]
          public decimal Price { get; set; }
          [Required]
          public string PictureUrl { get; set; } = string.Empty;
          [Required]
          public string Type { get; set; } = string.Empty;
          [Required]
          public string Brand { get; set; } = string.Empty;
          [Range(1,int.MaxValue, ErrorMessage = "Quantity in Stock Enter atleast 1")]
          public int QuantityInStock { get; set; }
}
