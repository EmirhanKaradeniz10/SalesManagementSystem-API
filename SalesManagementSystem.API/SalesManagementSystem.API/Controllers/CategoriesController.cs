using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.API.DTOs;
using SalesManagementSystem.API.DTOs.Categories;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models;

namespace SalesManagementSystem.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>A list of all categories.</returns>
    /// <response code="200">Categories retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public IActionResult GetAll()
    {
        // Retrieves a list of all categories mapped to DTOs

        var categories = _context.Categories
    .Select(c => new CategoryDto
    {
        Id = c.Id,
        Name = c.Name
    })
    .ToList();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Categories retrieved successfully",
            Data = categories
        });
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">Category information.</param>
    /// <returns>The created category.</returns>
    /// <response code="200">Category created successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Add(CreateCategoryDto dto)
    {
        // Validates and adds a new unique category to the database

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = ModelState
            });
        }

        if (_context.Categories.Any(c => c.Name == dto.Name))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Category already exists",
                Data = null
            });
        }

        var category = new Category
        {
            Name = dto.Name
        };

        _context.Categories.Add(category);
        _context.SaveChanges();

        return Ok(new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Category created successfully",
            Data = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            }
        });
    }

    /// <summary>
    /// Retrieves a category by its identifier.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <returns>The requested category.</returns>
    /// <response code="200">Category retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">Category not found.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public IActionResult GetById(int id)
    {
        // Finds and returns a specific category by its ID

        var category = _context.Categories.Find(id);

        if (category == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Category not found",
                Data = null
            });
        }

        return Ok(new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Category retrieved successfully",
            Data = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            }
        });
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <param name="dto">Updated category information.</param>
    /// <returns>The updated category.</returns>
    /// <response code="200">Category updated successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    /// <response code="404">Category not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Update(int id, UpdateCategoryDto dto)
    {
        // Validates and updates the name of an existing category

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = ModelState
            });
        }

        var category = _context.Categories.Find(id);

        if (category == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Category not found",
                Data = null
            });
        }

        if (_context.Categories.Any(c => c.Name == dto.Name && c.Id != id))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Category already exists",
                Data = null
            });
        }

        category.Name = dto.Name;

        _context.SaveChanges();

        return Ok(new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Category updated successfully",
            Data = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            }
        });
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">Category identifier.</param>
    /// <returns>Confirmation of the deletion operation.</returns>
    /// <response code="200">Category deleted successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    /// <response code="404">Category not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Delete(int id)
    {
        // Removes a specific category from the database by its ID

        var category = _context.Categories.Find(id);

        if (category == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Category not found",
                Data = null
            });
        }

        _context.Categories.Remove(category);
        _context.SaveChanges();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Category deleted successfully",
            Data = null
        });
    }


}