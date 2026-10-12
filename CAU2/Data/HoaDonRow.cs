namespace CAU2.Data;

// Du lieu hien thi tren luoi ben trai; TongTien khong luu trong database.
public sealed class HoaDonRow
{
    public int MaHd { get; set; }
    public string SoHoaDon => $"HD{MaHd:D6}";
    public DateTime NgayLap { get; set; }
    public int SoBan { get; set; }
    public string HinhThucTt { get; set; } = "";
    public string GhiChu { get; set; } = "";
    public decimal TongTien { get; set; }
}
