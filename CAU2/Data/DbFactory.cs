using CAU2.Models;
using Microsoft.EntityFrameworkCore;

namespace CAU2.Data;

public static class DbFactory
{
    // Dung SQLEXPRESS nhu co so du lieu nguoi dung da tao (khong phai LocalDB).
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=QuanCaPhe;Trusted_Connection=True;TrustServerCertificate=True;";

    public static QuanCaPheContext Create()
    {
        var options = new DbContextOptionsBuilder<QuanCaPheContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new QuanCaPheContext(options);
    }
}
