using System;
using System.Collections.Generic;
using System.Text;

namespace SalesManagementSystem.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // navigation 
    public List<Product>? Products { get; set; }
}