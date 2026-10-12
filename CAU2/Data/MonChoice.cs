namespace CAU2.Data;

public sealed class MonChoice
{
    public int MaMon { get; set; }
    public string TenMon { get; set; } = "";
    public decimal DonGia { get; set; }
    public string HienThi => $"{TenMon}  –  {DonGia:N0} đ";
}
