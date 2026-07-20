using System;
using System.Collections.Generic;
using System.Text;

namespace SalesManagementSystem.Models;

public class Customer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    //User-Customer
    public ICollection<User> Users { get; set; } = new List<User>();
}