namespace CAU2.Data;

// DTO rieng dang ky FastReport bang RegisterData(List<ChiTietIn>, "ChiTiet").
public sealed class ChiTietIn
{
    public int STT { get; set; }
    public string TenMon { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}
