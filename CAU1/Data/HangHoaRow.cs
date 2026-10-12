namespace CAU1.Data;

// Dong hien thi tren DataGridView; khong thay doi entity duoc scaffold.
public sealed class HangHoaRow
{
    public int MaHang { get; init; }
    public string TenHang { get; init; } = "";
    public string LoaiHang { get; init; } = "";
    public string DonVi { get; init; } = "";
    public decimal GiaBan { get; init; }
    public int SoLuongTon { get; init; }
    public DateTime NgayNhap { get; init; }
    public bool ConKinhDoanh { get; init; }
}
