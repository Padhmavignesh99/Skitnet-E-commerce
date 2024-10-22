using API.RequestHelper;
using Core.Entities;
using Core.Interfaces;
using Core.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController(IUnitOfWork unit) : BaseApiController
{
          [HttpGet]
          public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts([FromQuery]ProductSpecParams specParams)
          {
                    var spec = new ProductSpecification(specParams);
                    
                    return await CreatePagedResult(unit.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize);
          }
          [HttpGet("{id:int}")]
          public async Task<ActionResult<Product>> GetProduct(int id)
          {
                    var product = await unit.Repository<Product>().GetByIdAsync(id);
                    if(product == null) return BadRequest("Product Not Found");
                    return product;
          }
          [Authorize(Roles = "Admin")]
          [HttpPost]
          public async Task<ActionResult<Product>> CreateProduct(Product product)
          {
                    unit.Repository<Product>().Add(product);
                    if(await unit.Complete())
                    {
                              return CreatedAtAction("GetProduct",new{id = product.Id},product);
                    }
                    return BadRequest("Problem in Creating the Product");
          }
          [Authorize(Roles = "Admin")]
          [HttpPut("{id:int}")]
          public async Task<ActionResult<Product>> UpdateProduct(int id, Product product)
          {
                    if(product.Id != id || !ProductExists(id)) 
                              return BadRequest("Cannot Update the Product");
                    
                    unit.Repository<Product>().Update(product);
                    if(await unit.Complete())
                    {
                              return NoContent();
                    }

                    return BadRequest("Problem in Updating Product");
          }
          [Authorize(Roles = "Admin")]
          [HttpDelete("{id:int}")]
          public async Task<ActionResult<Product>> DeleteProduct(int id)
          {
                    var product = await unit.Repository<Product>().GetByIdAsync(id);
                    if(product == null) return NotFound();

                    unit.Repository<Product>().Remove(product);
                    if(await unit.Complete())
                    {
                              return NoContent();
                    }

                    return BadRequest("Problem in Deleting Product");
          }
          [HttpGet("brands")]
          public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
          {
                    var spec = new BrandListSpecification();
                    return Ok(await unit.Repository<Product>().ListAsync(spec));
          }
          [HttpGet("types")]
          public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
          {
                    var spec = new TypeListSpecification();
                    return Ok(await unit.Repository<Product>().ListAsync(spec));
          }
          private bool ProductExists(int id)
          {
                    return unit.Repository<Product>().Exists(id);
          }
}
