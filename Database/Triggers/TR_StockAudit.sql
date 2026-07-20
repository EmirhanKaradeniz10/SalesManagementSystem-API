CREATE TRIGGER TR_StockAudit
ON Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO StockAuditLog
    (
        ProductId,
        OldStock,
        NewStock,
        ChangeDate
    )
    SELECT
        d.Id,
        d.Stock,
        i.Stock,
        GETDATE()
    FROM deleted d
    INNER JOIN inserted i
        ON d.Id = i.Id
    WHERE d.Stock <> i.Stock;
END;