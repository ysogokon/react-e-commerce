﻿using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
  private readonly StoreContext _context;

  public ProductsController(StoreContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<List<Product>>> GetProducts()
  {
    return await _context.Products.ToListAsync();
  }

  [HttpGet("{id:int}")]
  public async Task<ActionResult<Product>> GetProduct(int id)
  {
    return await _context.Products.FindAsync(id);
  }
}
