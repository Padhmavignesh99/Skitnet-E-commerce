using System;
using System.Reflection.Metadata;
using Core.Entities;

namespace Core.Interfaces;

public interface ICartService
{
          Task<ShoppingCart?> GetCartAsync(string key);
          Task<ShoppingCart?> SetCartAsync(ShoppingCart cart);
          Task<bool> DeleteCartAsync(string key);
}
