namespace CAU2.Data;

// Chi tiet da mua, gia ban la gia luu trong ChiTietHoaDon.
public sealed class ChiTietRow
{
    public int MaCt { get; set; }
    public int Stt { get; set; }
    public string TenMon { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}
