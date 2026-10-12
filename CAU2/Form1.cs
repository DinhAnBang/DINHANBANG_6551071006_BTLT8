using System.Diagnostics;
using System.Globalization;
using CAU2.Data;
using CAU2.Models;
using FastReport;
using FastReport.Export.Pdf;
using Microsoft.EntityFrameworkCore;

namespace CAU2;

public partial class Form1 : Form
{
    private readonly CultureInfo _vi = CultureInfo.GetCultureInfo("vi-VN");
    private readonly System.Windows.Forms.Timer _searchDelay = new() { Interval = 350 };
    private int? _selectedInvoice;
    private int? _selectedDetail;
    private bool _bindingMaster;
    private bool _bindingDetail;
    private bool _initializing = true;
    private int _masterLoadId;
    private int _detailLoadId;
    private List<HoaDonRow> _masterRows = [];
    private List<ChiTietRow> _detailRows = [];

    public Form1()
    {
        InitializeComponent();
        cboLocBan.Items.Add("Tất cả");
        cboSoBan.Items.Clear();
        for (int i = 1; i <= 10; i++)
        {
            cboLocBan.Items.Add(i.ToString());
            cboSoBan.Items.Add(i.ToString());
        }
        cboLocBan.SelectedIndex = 0;
        cboLocTT.Items.AddRange(["Tất cả", "Tiền mặt", "Chuyển khoản"]);
        cboLocTT.SelectedIndex = 0;
        cboSoBan.SelectedItem = "1";
        dtLocTu.Value = DateTime.Today.AddMonths(-1);
        dtLocDen.Value = DateTime.Today;
        dtNgayLap.Value = DateTime.Now;

        btnTim.Click += async (_, _) => await SafeAsync("tìm kiếm hóa đơn", () => LoadInvoicesAsync());
        dtLocTu.ValueChanged += async (_, _) => await FilterChangedAsync();
        dtLocDen.ValueChanged += async (_, _) => await FilterChangedAsync();
        cboLocBan.SelectedIndexChanged += async (_, _) => await FilterChangedAsync();
        cboLocTT.SelectedIndexChanged += async (_, _) => await FilterChangedAsync();
        txtLocMon.TextChanged += (_, _) =>
        {
            if (_initializing) return;
            _searchDelay.Stop();
            _searchDelay.Start();
        };
        _searchDelay.Tick += async (_, _) =>
        {
            _searchDelay.Stop();
            await SafeAsync("tìm theo tên món", () => LoadInvoicesAsync());
        };
        dgvHoaDon.SelectionChanged += async (_, _) =>
        {
            if (_bindingMaster || _initializing || dgvHoaDon.SelectedRows.Count == 0) return;
            if (dgvHoaDon.SelectedRows[0].DataBoundItem is HoaDonRow item)
                await SafeAsync("tải chi tiết hóa đơn", () => SelectInvoiceAsync(item));
        };
        dgvChiTiet.SelectionChanged += (_, _) =>
        {
            if (_bindingDetail || dgvChiTiet.SelectedRows.Count == 0) return;
            if (dgvChiTiet.SelectedRows[0].DataBoundItem is ChiTietRow item)
            {
                _selectedDetail = item.MaCt;
                numSoLuong.Value = Math.Clamp(item.SoLuong, (int)numSoLuong.Minimum, (int)numSoLuong.Maximum);
            }
        };
        btnHoaDonMoi.Click += async (_, _) => await SafeAsync("tạo hóa đơn", AddInvoiceAsync);
        btnSuaHoaDon.Click += async (_, _) => await SafeAsync("sửa hóa đơn", EditInvoiceAsync);
        btnXoaHoaDon.Click += async (_, _) => await SafeAsync("xóa hóa đơn", DeleteInvoiceAsync);
        btnThemMon.Click += async (_, _) => await SafeAsync("thêm món", AddDishAsync);
        btnSuaSL.Click += async (_, _) => await SafeAsync("sửa số lượng", EditQuantityAsync);
        btnXoaMon.Click += async (_, _) => await SafeAsync("xóa món", DeleteDishAsync);
        btnXemHD.Click += async (_, _) => await SafeAsync("xem trước hóa đơn", PreviewAsync);
        btnXuatPDF.Click += async (_, _) => await SafeAsync("xuất hóa đơn PDF", ExportPdfAsync);
        btnIn.Click += async (_, _) => await SafeAsync("in hóa đơn", PrintAsync);
        Load += async (_, _) =>
        {
            await SafeAsync("khởi động", async () =>
            {
                await LoadMenuAsync();
                _initializing = false;
                await LoadInvoicesAsync();
            });
        };
        FormClosed += (_, _) => _searchDelay.Dispose();
    }

    private static string Code(int id) => $"HD{id:D6}";
    private string Money(decimal money) => money.ToString("N0", _vi) + " đ";

    // Tat ca thao tac bat dong bo deu co try-catch va MessageBox.
    private async Task SafeAsync(string action, Func<Task> operation)
    {
        try { await operation(); }
        catch (DbUpdateException ex) { ShowError(action, ex.InnerException ?? ex); }
        catch (IOException ex) { ShowFileError(ex); }
        catch (UnauthorizedAccessException ex) { ShowFileError(ex); }
        catch (Exception ex) { ShowError(action, ex); }
    }

    private void ShowError(string action, Exception ex)
    {
        string hint = ex.Message.Contains("Login failed", StringComparison.OrdinalIgnoreCase) ||
                      ex.Message.Contains("Cannot open database", StringComparison.OrdinalIgnoreCase)
            ? "\n\nKiểm tra SQL Server .\\SQLEXPRESS và database QuanCaPhe."
            : "";
        MessageBox.Show(this, $"Không thể {action}.\n\nChi tiết: {ex.Message}{hint}", "Lỗi",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        lblStatus.Text = $"Có lỗi khi {action}.";
    }

    private void ShowFileError(Exception ex) => MessageBox.Show(this,
        "Không thể ghi file. Nếu file đang mở trong ứng dụng khác, hãy đóng file rồi thử lại.\n\n" + ex.Message,
        "Lỗi ghi file", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void ShowNotice(string message) => MessageBox.Show(this, message, "Thông báo", MessageBoxButtons.OK,
        MessageBoxIcon.Information);

    private async Task FilterChangedAsync()
    {
        if (!_initializing) await SafeAsync("lọc hóa đơn", () => LoadInvoicesAsync());
    }

    private async Task LoadMenuAsync()
    {
        using var db = DbFactory.Create();
        var items = await db.Mons.AsNoTracking().OrderBy(x => x.MaMon)
            .Select(x => new MonChoice { MaMon = x.MaMon, TenMon = x.TenMon, DonGia = x.DonGia })
            .ToListAsync();
        cboMon.DisplayMember = nameof(MonChoice.HienThi);
        cboMon.ValueMember = nameof(MonChoice.MaMon);
        cboMon.DataSource = items;
        if (items.Count == 0) lblStatus.Text = "Chưa có món. Kiểm tra dữ liệu bảng Mon.";
    }

    // Loc theo thoi gian, ban, hinh thuc va ten mon qua ChiTietHoaDon -> Mon.
    // Tong tien truy van tinh truc tiep; KHONG luu cot TongTien trong bang HoaDon.
    private async Task LoadInvoicesAsync(int? focusInvoice = null, bool selectFirst = true)
    {
        if (dtLocTu.Value.Date > dtLocDen.Value.Date)
        {
            MessageBox.Show(this, "Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Bộ lọc không hợp lệ",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var loadId = ++_masterLoadId;
        var from = dtLocTu.Value.Date;
        var untilExclusive = dtLocDen.Value.Date.AddDays(1);
        var ban = cboLocBan.SelectedItem?.ToString() ?? "Tất cả";
        var hinhThuc = cboLocTT.SelectedItem?.ToString() ?? "Tất cả";
        var mon = txtLocMon.Text.Trim();
        using var db = DbFactory.Create();
        IQueryable<HoaDon> query = db.HoaDons.AsNoTracking()
            .Where(h => h.NgayLap >= from && h.NgayLap < untilExclusive);
        if (ban != "Tất cả" && int.TryParse(ban, out int soBan)) query = query.Where(h => h.SoBan == soBan);
        if (hinhThuc != "Tất cả") query = query.Where(h => h.HinhThucTt == hinhThuc);
        if (mon.Length != 0)
            query = query.Where(h => h.ChiTietHoaDons.Any(ct => EF.Functions.Like(ct.MaMonNavigation.TenMon, "%" + mon + "%")));

        var list = await query.OrderByDescending(h => h.NgayLap).ThenByDescending(h => h.MaHd)
            .Select(h => new HoaDonRow
            {
                MaHd = h.MaHd,
                NgayLap = h.NgayLap,
                SoBan = h.SoBan,
                HinhThucTt = h.HinhThucTt,
                GhiChu = h.GhiChu ?? "",
                TongTien = h.ChiTietHoaDons.Sum(ct => (decimal?)(ct.SoLuong * ct.DonGia)) ?? 0m
            }).ToListAsync();
        if (loadId != _masterLoadId || IsDisposed) return;

        _masterRows = list;
        _bindingMaster = true;
        try
        {
            dgvHoaDon.DataSource = null;
            dgvHoaDon.DataSource = list;
            dgvHoaDon.ClearSelection();
        }
        finally { _bindingMaster = false; }

        lblDoanhThu.Text = "Tổng doanh thu đang hiển thị: " + Money(list.Sum(i => i.TongTien));
        lblStatus.Text = $"Tìm thấy {list.Count} hóa đơn";
        int? target = focusInvoice ?? _selectedInvoice;
        var selected = target is null ? null : list.FirstOrDefault(h => h.MaHd == target.Value);
        if (selected is null && selectFirst) selected = list.FirstOrDefault();
        if (selected is not null)
        {
            foreach (DataGridViewRow row in dgvHoaDon.Rows)
            {
                if (row.DataBoundItem is HoaDonRow item && item.MaHd == selected.MaHd)
                {
                    _bindingMaster = true;
                    try
                    {
                        row.Selected = true;
                        dgvHoaDon.CurrentCell = row.Cells[0];
                    }
                    finally { _bindingMaster = false; }
                    break;
                }
            }
            await SelectInvoiceAsync(selected);
        }
        else ClearInvoiceSelection();
    }

    private async Task SelectInvoiceAsync(HoaDonRow row)
    {
        _selectedInvoice = row.MaHd;
        lblHoaDonDangChon.Text = $"Hóa đơn: {row.SoHoaDon}";
        dtNgayLap.Value = row.NgayLap;
        cboSoBan.SelectedItem = row.SoBan.ToString();
        rdTienMat.Checked = row.HinhThucTt == "Tiền mặt";
        rdChuyenKhoan.Checked = row.HinhThucTt == "Chuyển khoản";
        txtGhiChu.Text = row.GhiChu;
        await LoadDetailsAsync(row.MaHd);
    }

    private async Task LoadDetailsAsync(int invoiceId, int? focusDetail = null)
    {
        int version = ++_detailLoadId;
        using var db = DbFactory.Create();
        var rows = await db.ChiTietHoaDons.AsNoTracking()
            .Where(x => x.MaHd == invoiceId)
            .OrderBy(x => x.MaCt)
            .Select(x => new ChiTietRow
            {
                MaCt = x.MaCt, TenMon = x.MaMonNavigation.TenMon,
                SoLuong = x.SoLuong, DonGia = x.DonGia
            }).ToListAsync();
        if (version != _detailLoadId || _selectedInvoice != invoiceId || IsDisposed) return;
        for (int i = 0; i < rows.Count; i++) rows[i].Stt = i + 1;
        _detailRows = rows;
        _selectedDetail = null;
        _bindingDetail = true;
        try
        {
            dgvChiTiet.DataSource = null;
            dgvChiTiet.DataSource = rows;
            dgvChiTiet.ClearSelection();
        }
        finally { _bindingDetail = false; }
        lblTongCong.Text = "Tổng cộng: " + Money(rows.Sum(x => x.ThanhTien));
        if (focusDetail.HasValue)
        {
            foreach (DataGridViewRow gridRow in dgvChiTiet.Rows)
            {
                if (gridRow.DataBoundItem is ChiTietRow item && item.MaCt == focusDetail.Value)
                {
                    _bindingDetail = true;
                    try
                    {
                        gridRow.Selected = true;
                        dgvChiTiet.CurrentCell = gridRow.Cells[0];
                    }
                    finally { _bindingDetail = false; }
                    _selectedDetail = item.MaCt;
                    break;
                }
            }
        }
    }

    private void ClearInvoiceSelection()
    {
        ++_detailLoadId;
        _selectedInvoice = null;
        _selectedDetail = null;
        _detailRows = [];
        lblHoaDonDangChon.Text = "Hóa đơn: Chưa chọn";
        lblTongCong.Text = "Tổng cộng: 0 đ";
        _bindingDetail = true;
        try { dgvChiTiet.DataSource = null; }
        finally { _bindingDetail = false; }
        dtNgayLap.Value = DateTime.Now;
        cboSoBan.SelectedItem = "1";
        rdTienMat.Checked = true;
        txtGhiChu.Clear();
        numSoLuong.Value = 1;
    }

    private int? RequireInvoice()
    {
        if (_selectedInvoice is not null) return _selectedInvoice;
        MessageBox.Show(this, "Vui lòng chọn hóa đơn ở bảng bên trái trước.", "Chưa chọn hóa đơn",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return null;
    }

    private int? RequireDetail()
    {
        if (RequireInvoice() is null) return null;
        if (_selectedDetail is not null) return _selectedDetail;
        MessageBox.Show(this, "Vui lòng chọn món ở bảng chi tiết trước.", "Chưa chọn món",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return null;
    }

    private void WriteInvoice(HoaDon h)
    {
        h.NgayLap = dtNgayLap.Value;
        h.SoBan = int.Parse(cboSoBan.SelectedItem?.ToString() ?? "1", CultureInfo.InvariantCulture);
        h.HinhThucTt = rdChuyenKhoan.Checked ? "Chuyển khoản" : "Tiền mặt";
        h.GhiChu = string.IsNullOrWhiteSpace(txtGhiChu.Text) ? null! : txtGhiChu.Text.Trim();
    }

    private async Task AddInvoiceAsync()
    {
        using var db = DbFactory.Create();
        var h = new HoaDon();
        WriteInvoice(h);
        db.HoaDons.Add(h);
        await db.SaveChangesAsync();
        await LoadInvoicesAsync(h.MaHd, selectFirst: false);
        ShowNotice($"Đã tạo {Code(h.MaHd)}.\nHãy chọn hóa đơn và thêm món vào hóa đơn.");
    }

    private async Task EditInvoiceAsync()
    {
        int? ma = RequireInvoice();
        if (ma is null) return;
        using var db = DbFactory.Create();
        var entity = await db.HoaDons.FindAsync(ma.Value);
        if (entity is null) throw new InvalidOperationException("Hóa đơn này không còn trong cơ sở dữ liệu.");
        WriteInvoice(entity);
        await db.SaveChangesAsync();
        await LoadInvoicesAsync(ma.Value, selectFirst: false);
        ShowNotice("Đã sửa hóa đơn " + Code(ma.Value));
    }

    // Giao dich dam bao xoa chi tiet truoc, hoa don sau (FK NO ACTION).
    private async Task DeleteInvoiceAsync()
    {
        int? ma = RequireInvoice();
        if (ma is null) return;
        if (MessageBox.Show(this, $"Xóa {Code(ma.Value)} và tất cả món thuộc hóa đơn?",
            "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        using var db = DbFactory.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        var entity = await db.HoaDons.FindAsync(ma.Value);
        if (entity is null) throw new InvalidOperationException("Hóa đơn không còn tồn tại.");
        var details = await db.ChiTietHoaDons.Where(x => x.MaHd == ma.Value).ToListAsync();
        db.ChiTietHoaDons.RemoveRange(details);
        await db.SaveChangesAsync();
        db.HoaDons.Remove(entity);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        _selectedInvoice = null;
        await LoadInvoicesAsync();
        ShowNotice("Đã xóa " + Code(ma.Value) + " và các dòng chi tiết của nó.");
    }

    private async Task AddDishAsync()
    {
        int? ma = RequireInvoice();
        if (ma is null) return;
        if (cboMon.SelectedItem is not MonChoice mon)
        {
            MessageBox.Show(this, "Danh mục món đang trống.");
            return;
        }
        using var db = DbFactory.Create();
        var invoiceExists = await db.HoaDons.AnyAsync(h => h.MaHd == ma.Value);
        if (!invoiceExists) throw new InvalidOperationException("Hóa đơn không còn tồn tại.");
        // Luon lay gia moi nhat tu Mon vao luc them; sua Mon sau nay khong thay doi don gia da ban.
        var currentPrice = await db.Mons.Where(m => m.MaMon == mon.MaMon).Select(m => (decimal?)m.DonGia).FirstOrDefaultAsync();
        if (currentPrice is null) throw new InvalidOperationException("Món vừa chọn không còn tồn tại.");
        var row = new ChiTietHoaDon
        {
            MaHd = ma.Value, MaMon = mon.MaMon,
            SoLuong = (int)numSoLuong.Value, DonGia = currentPrice.Value
        };
        db.ChiTietHoaDons.Add(row);
        await db.SaveChangesAsync();
        await LoadInvoicesAsync(ma.Value, selectFirst: false);
        await LoadDetailsAsync(ma.Value, row.MaCt);
        lblStatus.Text = "Đã thêm món; tổng tiền được tính lại từ các dòng chi tiết.";
    }

    private async Task EditQuantityAsync()
    {
        int? maCt = RequireDetail();
        if (maCt is null) return;
        int maHd = _selectedInvoice!.Value;
        using var db = DbFactory.Create();
        var row = await db.ChiTietHoaDons.FirstOrDefaultAsync(x => x.MaCt == maCt.Value && x.MaHd == maHd);
        if (row is null) throw new InvalidOperationException("Món đã bị xóa ở cơ sở dữ liệu.");
        row.SoLuong = (int)numSoLuong.Value;
        await db.SaveChangesAsync();
        await LoadInvoicesAsync(maHd, selectFirst: false);
        await LoadDetailsAsync(maHd, maCt);
    }

    private async Task DeleteDishAsync()
    {
        int? maCt = RequireDetail();
        if (maCt is null) return;
        if (MessageBox.Show(this, "Xóa món đang chọn khỏi hóa đơn?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        int maHd = _selectedInvoice!.Value;
        using var db = DbFactory.Create();
        var row = await db.ChiTietHoaDons.FirstOrDefaultAsync(x => x.MaCt == maCt.Value && x.MaHd == maHd);
        if (row is null) throw new InvalidOperationException("Món đã bị xóa ở cơ sở dữ liệu.");
        db.ChiTietHoaDons.Remove(row);
        await db.SaveChangesAsync();
        await LoadInvoicesAsync(maHd, selectFirst: false);
    }

    // Nguon FastReport: List<ChiTietIn> va cac tham so rieng theo de bai.
    private async Task<Report?> CreateReportAsync()
    {
        int? ma = RequireInvoice();
        if (ma is null) return null;
        using var db = DbFactory.Create();
        var header = await db.HoaDons.AsNoTracking().FirstOrDefaultAsync(h => h.MaHd == ma.Value);
        if (header is null) throw new InvalidOperationException("Hóa đơn đã bị xóa.");
        var data = await db.ChiTietHoaDons.AsNoTracking().Where(ct => ct.MaHd == ma.Value)
            .OrderBy(ct => ct.MaCt)
            .Select(ct => new ChiTietIn
            {
                TenMon = ct.MaMonNavigation.TenMon,
                SoLuong = ct.SoLuong, DonGia = ct.DonGia,
                ThanhTien = ct.SoLuong * ct.DonGia
            }).ToListAsync();
        if (data.Count == 0)
        {
            MessageBox.Show(this, "Hóa đơn chưa có món, không thể in hoặc xuất PDF.", "Chưa có dữ liệu",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }
        for (int i = 0; i < data.Count; i++) data[i].STT = i + 1;
        string path = Path.Combine(AppContext.BaseDirectory, "Reports", "HoaDon.frx");
        if (!File.Exists(path)) throw new FileNotFoundException("Thiếu mẫu FastReport HoaDon.frx", path);
        var report = new Report();
        try
        {
            report.Load(path);
            report.RegisterData(data, "ChiTiet");
            var source = report.GetDataSource("ChiTiet") ?? throw new InvalidOperationException("Nguồn report ChiTiet không tồn tại.");
            source.Enabled = true;
            report.SetParameterValue("TenQuan", "QUÁN CÀ PHÊ GÓC PHỐ");
            report.SetParameterValue("SoHoaDon", Code(header.MaHd));
            report.SetParameterValue("NgayLap", header.NgayLap.ToString("dd/MM/yyyy HH:mm", _vi));
            report.SetParameterValue("SoBan", header.SoBan.ToString(CultureInfo.InvariantCulture));
            report.SetParameterValue("HinhThucTT", header.HinhThucTt);
            if (!report.Prepare()) throw new InvalidOperationException("Không thể chuẩn bị report hóa đơn.");
            return report;
        }
        catch { report.Dispose(); throw; }
    }

    private async Task PreviewAsync()
    {
        using var report = await CreateReportAsync();
        if (report is null) return;
        report.ShowPrepared(true);
    }

    private async Task ExportPdfAsync()
    {
        if (RequireInvoice() is not int ma) return;
        // Khong cho luu file trang khi hoa don chua co mon.
        using var report = await CreateReportAsync();
        if (report is null) return;
        using var dialog = new SaveFileDialog
        {
            Title = "Lưu hóa đơn dạng PDF",
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = $"HoaDon_{Code(ma)}.pdf",
            DefaultExt = "pdf",
            AddExtension = true,
            OverwritePrompt = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var dest = dialog.FileName;
        await Task.Run(() =>
        {
            using var pdf = new PDFExport();
            report.Export(pdf, dest);
        });
        if (MessageBox.Show(this, "Xuất PDF thành công. Bạn có muốn mở file?", "Hoàn tất",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            Process.Start(new ProcessStartInfo(dest) { UseShellExecute = true });
        }
    }

    private async Task PrintAsync()
    {
        using var report = await CreateReportAsync();
        if (report is null) return;
        report.PrintSettings.ShowDialog = true;
        report.PrintPrepared();
    }
}
