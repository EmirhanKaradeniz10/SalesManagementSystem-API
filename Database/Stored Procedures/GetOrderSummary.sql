CREATE PROCEDURE GetOrderSummary
AS
BEGIN
    SELECT
        o.Id AS OrderId,
        c.Name AS CustomerName,
        o.OrderDate,
        p.Name AS ProductName,
        od.Quantity,
        p.Price,
        (od.Quantity * p.Price) AS TotalPrice
    FROM Orders o
    INNER JOIN Customers c ON o.CustomerId = c.Id
    INNER JOIN OrderDetails od ON o.Id = od.OrderId
    INNER JOIN Products p ON od.ProductId = p.Id
    WHERE o.IsCancelled = 0
END