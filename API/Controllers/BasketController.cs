using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class BasketController : BaseApiController
{
  private readonly StoreContext _context;

  public BasketController(StoreContext context)
  {
    _context = context;
  }

  [HttpGet("GetBasket")]
  public async Task<ActionResult<BasketDto>> GetBasket()
  {
    var basket = await RetrieveBasket();

    if (basket == null)
    {
      return NotFound();
    }

    return MapBaskedToDTO(basket);
  }



  [HttpPost] // api/basket/productId=1/quantity=1
  public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
  {
    var basket = await RetrieveBasket();
    if (basket == null)
    {
      basket = CreateBasket();
    }
    var product = await _context.Products.FindAsync(productId);

    if (product == null)
    {
      return NotFound();
    }

    basket.AddItem(product, quantity);

    var result = await _context.SaveChangesAsync() > 0;

    if (result)
    {
      return CreatedAtRoute("GetBasket", MapBaskedToDTO(basket));
    }

    return BadRequest(new ProblemDetails { Title = "Problem saving item to the basket" });
  }


  [HttpDelete]
  public async Task<ActionResult> DeleteBasketItem(int productId, int quantity)
  {
    var basket = await RetrieveBasket();
    if (basket == null)
    {
      return NotFound();
    }

    basket.RemoveItem(productId, quantity);
    var result = await _context.SaveChangesAsync() > 0;

    if (result)
    {
      return Ok();
    }

    return BadRequest(new ProblemDetails { Title = "Problem removing item from the basket" });
  }

  private async Task<Basket> RetrieveBasket()
  {
    return await _context.Baskets
      .Include(i => i.Items).ThenInclude(p => p.Product)
      .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["buyerId"]);
  }
  private Basket CreateBasket()
  {
    var buyerId = Guid.NewGuid().ToString();
    var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
    Response.Cookies.Append("buyerId", buyerId, cookieOptions);
    var basket = new Basket { BuyerId = buyerId };
    _context.Baskets.Add(basket);
    return basket;
  }

  private BasketDto MapBaskedToDTO(Basket basket)
  {
    return new BasketDto
    {
      Id = basket.Id,
      BuyerId = basket.BuyerId,
      Items = basket.Items.Select(x => new BasketItemDto
      {
        ProductId = x.ProductId,
        Name = x.Product.Name,
        Price = x.Product.Price,
        PictureUrl = x.Product.PictureUrl,
        Brand = x.Product.Brand,
        Type = x.Product.Type,
        Quantity = x.Quantity
      }).ToList()
    };
  }
}
