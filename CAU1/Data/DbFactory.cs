using CAU1.Models;
using Microsoft.EntityFrameworkCore;

namespace CAU1.Data;

public static class DbFactory
{
    // Doi Server neu SQL Server LocalDB o may ban dung ten khac.
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=TiemTapHoa;Trusted_Connection=True;TrustServerCertificate=True;";

    public static TiemTapHoaContext Create()
    {
        var options = new DbContextOptionsBuilder<TiemTapHoaContext>()
            .UseSqlServer(ConnectionString).Options;
        return new TiemTapHoaContext(options);
    }
}
