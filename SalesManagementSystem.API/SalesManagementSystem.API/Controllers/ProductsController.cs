using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.API.DTOs;
using SalesManagementSystem.API.Services;
using SalesManagementSystem.Models;

namespace SalesManagementSystem.API.Controllers;

using ClosedXML.Excel;
using CsvHelper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.API.DTOs.Products;
using SalesManagementSystem.Data;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    private readonly AppDbContext _context;

    public ProductsController(ProductService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }



    /// <summary>
    /// Retrieves a filtered and paginated list of products.
    /// </summary>
    /// <param name="query">Filtering, sorting and pagination parameters.</param>
    /// <returns>Returns a filtered collection of products.</returns>
    /// <response code="200">Products retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    [EnableRateLimiting("products-read")]
    public IActionResult GetAll([FromQuery] ProductQueryParameters query)
    {
        // Retrieves a filtered, sorted, and paginated list of products

        var result = _service.GetFiltered(
            query.PageNumber,
            query.PageSize,
            query.Search,
            query.Category,
            query.MinPrice,
            query.MaxPrice,
            query.SortBy,
            query.IsDescending);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Filtered products retrieved",
            Data = result
        });
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">Product information.</param>
    /// <returns>Returns the newly created product.</returns>
    /// <response code="200">Product created successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Add(CreateProductDto dto)
    {
        // Validates and adds a new unique product to the database

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = ModelState
            });
        }

        if (!_service.CategoryExists(dto.CategoryId))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid CategoryId",
                Data = null
            });
        }

        if (_service.ProductNameExists(dto.Name))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Product name already exists",
                Data = null
            });
        }

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        };

        var created = _service.Add(product);

        return Ok(new ApiResponse<ProductDto>
        {
            Success = true,
            Message = "Product created successfully",
            Data = new ProductDto
            {
                Id = created.Id,
                Name = created.Name,
                Price = created.Price,
                Stock = created.Stock,
                CategoryId = created.CategoryId,
                CategoryName = created.Category?.Name
            }
        });
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="dto">Updated product information.</param>
    /// <returns>Returns the updated product.</returns>
    /// <response code="200">Product updated successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    /// <response code="404">Product not found.</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Update(int id, UpdateProductDto dto)
    {
        // Validates and updates the details of an existing product

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = ModelState
            });
        }

        if (!_service.CategoryExists(dto.CategoryId))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid CategoryId",
                Data = null
            });
        }

        if (_service.ProductNameExists(dto.Name) &&
            _service.GetById(id)?.Name != dto.Name)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Product name already exists",
                Data = null
            });
        }

        var updated = _service.Update(id, new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            CategoryId = dto.CategoryId
        });

        if (updated == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Product not found",
                Data = null
            });
        }

        return Ok(new ApiResponse<ProductDto>
        {
            Success = true,
            Message = "Product updated successfully",
            Data = new ProductDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Price = updated.Price,
                Stock = updated.Stock,
                CategoryId = updated.CategoryId,
                CategoryName = updated.Category?.Name
            }
        });
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <returns>Returns a confirmation of the deletion operation.</returns>
    /// <response code="200">Product deleted successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    /// <response code="404">Product not found.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Delete(int id)
    {
        // Removes a specific product from the database by its ID

        var result = _service.Delete(id);

        if (!result)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Product not found",
                Data = null
            });
        }

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Product deleted successfully",
            Data = null
        });
    }

    /// <summary>
    /// Imports products from a CSV file.
    /// </summary>
    /// <param name="file">CSV file containing product records.</param>
    /// <returns>Returns an import summary including successful and failed rows.</returns>
    /// <response code="200">Products imported successfully.</response>
    /// <response code="400">Invalid or empty CSV file.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    [HttpPost("import")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult Import(IFormFile file)
    {
        // Processes bulk product data from uploaded CSV or Excel files

        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a CSV or Excel file.");
        }

        var extension = Path.GetExtension(file.FileName).ToLower();

        if (extension != ".csv" && extension != ".xlsx")
        {
            return BadRequest("Only .csv and .xlsx files are supported.");
        }

        var products = new List<ProductImportDto>();

        if (extension == ".csv")
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            products = csv.GetRecords<ProductImportDto>().ToList();
        }
        else if (extension == ".xlsx")
        {
            using var workbook = new XLWorkbook(file.OpenReadStream());

            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RowsUsed().Skip(1); // Skips the header row

            foreach (var row in rows)
            {
                var name = row.Cell(1).GetString();

                var priceText = row.Cell(2).GetString();
                decimal? price = decimal.TryParse(priceText, out var parsedPrice)
                    ? parsedPrice
                    : null;

                var stockText = row.Cell(3).GetString();
                int? stock = int.TryParse(stockText, out var parsedStock)
                    ? parsedStock
                    : null;

                var categoryText = row.Cell(4).GetString();
                int? categoryId = int.TryParse(categoryText, out var parsedCategory)
                    ? parsedCategory
                    : null;

                products.Add(new ProductImportDto
                {
                    Name = string.IsNullOrWhiteSpace(name) ? null : name,
                    Price = price,
                    Stock = stock,
                    CategoryId = categoryId
                });
            }
        }


        // Validates fields for blank entries, non-positive prices, or negative stock
        var imported = 0;

        // Tracks product entries with updated stock levels
        var updated = 0;

        // Tracks failed records to provide feedback to the user
        var totalRows = products.Count;
        var failed = 0;
        var importErrors = new List<ImportErrorDto>();

        var importedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Rows start at index 2 since the first row contains column headers

        var rowNumber = 2;

        foreach (var dto in products)
        {
            var validationErrors = new List<string>();

            //Name Null
            if (dto.Name == null)
            {
                validationErrors.Add("Product name is required.");
            }

            //Price Null or invalid
            if (dto.Price == null)
            {
                validationErrors.Add("Price is required.");
            }
            else if (dto.Price <= 0)
            {
                validationErrors.Add("Invalid price.");
            }

            //stock Null or invalid
            if (dto.Stock == null)
            {
                validationErrors.Add("Stock is required.");
            }
            else if (dto.Stock < 0)
            {
                validationErrors.Add("Invalid stock.");
            }

            //category Null or invalid
            if (dto.CategoryId == null)
            {
                validationErrors.Add("Category is required.");
            }
            else if (!_context.Categories.Any(c => c.Id == dto.CategoryId))
            {
                validationErrors.Add("Category not found.");
            }

            if (validationErrors.Any())
            {
                failed++;

                importErrors.Add(new ImportErrorDto
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    CategoryId = dto.CategoryId,
                    Error = string.Join(" | ", validationErrors)
                });

                rowNumber++;
                continue;
            }

            // Checks if product exists to dynamically update stock counts
            var existingProduct = _context.Products.FirstOrDefault(p =>p.Name == dto.Name);

            if (existingProduct != null)
            {
                if (existingProduct.Price == dto.Price &&
                    existingProduct.CategoryId == dto.CategoryId)
                {
                    existingProduct.Stock += dto.Stock!.Value;

                    updated++;
                    rowNumber++;

                    continue;
                }

                failed++;

                importErrors.Add(new ImportErrorDto
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Stock = dto.Stock,
                    CategoryId = dto.CategoryId,
                    Error = "Product name already exists with different price or category."
                });

                rowNumber++;

                continue;
            }

            var product = new Product
            {
                Name = dto.Name!,
                Price = dto.Price!.Value,
                Stock = dto.Stock!.Value,
                CategoryId = dto.CategoryId!.Value
            };

            _context.Products.Add(product);

            importedNames.Add(dto.Name!);

            imported++;
            rowNumber++;
        }

        // Generates an Excel spreadsheet detailing all validation failures
        if (importErrors.Any())
        {
            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Import Errors");

            // Summary
            worksheet.Cell(1, 1).Value = "Import Summary";
            worksheet.Cell(2, 1).Value = "Total Rows";
            worksheet.Cell(2, 2).Value = totalRows;

            worksheet.Cell(3, 1).Value = "Imported";
            worksheet.Cell(3, 2).Value = imported;

            worksheet.Cell(4, 1).Value = "Updated";
            worksheet.Cell(4, 2).Value = updated;

            worksheet.Cell(5, 1).Value = "Failed";
            worksheet.Cell(5, 2).Value = failed;

            // Table Header
            worksheet.Cell(6, 1).Value = "Name";
            worksheet.Cell(6, 2).Value = "Price";
            worksheet.Cell(6, 3).Value = "Stock";
            worksheet.Cell(6, 4).Value = "CategoryId";
            worksheet.Cell(6, 5).Value = "Error";

            var row = 7;

            foreach (var error in importErrors)
            {
                worksheet.Cell(row, 1).Value = error.Name;
                worksheet.Cell(row, 2).Value = error.Price?.ToString();
                worksheet.Cell(row, 3).Value = error.Stock?.ToString();
                worksheet.Cell(row, 4).Value = error.CategoryId?.ToString();
                worksheet.Cell(row, 5).Value = error.Error;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ImportErrors.xlsx");
        }

        _context.SaveChanges();

        _service.InvalidateProductsCache();

        return Ok(new
        {
            TotalRows = totalRows,
            Imported = imported,
            Updated = updated,
            Failed = failed,
            Errors = importErrors
        });
    }

    /// <summary>
    /// Exports all products to an Excel file.
    /// </summary>
    /// <returns>Excel file containing all products.</returns>
    /// <response code="200">Products exported successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    [HttpGet("export")]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public IActionResult Export()
    {
        // Generates and downloads an Excel file containing all products

        var products = _context.Products
            .Include(p => p.Category)
            .ToList();

        using var workbook = new XLWorkbook();

        var worksheet = workbook.Worksheets.Add("Products");

        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Price";
        worksheet.Cell(1, 4).Value = "Stock";
        worksheet.Cell(1, 5).Value = "Category";

        var row = 2;

        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.Id;
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Price;
            worksheet.Cell(row, 4).Value = product.Stock;
            worksheet.Cell(row, 5).Value = product.Category?.Name;

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();

        workbook.SaveAs(stream);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Products_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}