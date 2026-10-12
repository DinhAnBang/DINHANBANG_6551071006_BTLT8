using System.Data;
using System.Diagnostics;
using System.Globalization;
using CAU1.Data;
using CAU1.Models;
using ClosedXML.Excel;
using FastReport;
using FastReport.Export.Pdf;
using Microsoft.EntityFrameworkCore;

namespace CAU1;

public partial class Form1 : Form
{
    private readonly System.Windows.Forms.Timer _typingTimer = new() { Interval = 350 };
    private int? _selectedMaHang;
    private int _reloadVersion;
    private bool _bindingGrid;
    private bool _bindingFilters;

    public Form1()
    {
        InitializeComponent();

        cboLoaiHang.Items.AddRange(["Đồ uống", "Thực phẩm khô", "Gia vị", "Đồ gia dụng"]);
        cboDonVi.Items.AddRange(["Chai", "Gói", "Hộp", "Túi"]);
        cboLoaiHang.SelectedIndex = 0;
        cboDonVi.SelectedIndex = 0;
        dtNgayNhap.Value = DateTime.Today;

        _typingTimer.Tick += async (_, _) =>
        {
            _typingTimer.Stop();
            await LoadGridAsync();
        };
        txtSearch.TextChanged += (_, _) =>
        {
            if (_bindingFilters) return;
            _typingTimer.Stop();
            _typingTimer.Start();
        };
        cboFilterType.SelectedIndexChanged += async (_, _) =>
        {
            if (!_bindingFilters) await LoadGridAsync();
        };
        chkOnlyActive.CheckedChanged += async (_, _) =>
        {
            if (!_bindingFilters) await LoadGridAsync();
        };

        dgvHangHoa.SelectionChanged += DgvHangHoa_SelectionChanged;
        btnThem.Click += async (_, _) => await AddAsync();
        btnSua.Click += async (_, _) => await UpdateAsync();
        btnXoa.Click += async (_, _) => await DeleteAsync();
        btnLamMoi.Click += async (_, _) => await ResetAsync();
        btnPreview.Click += async (_, _) => await PreviewReportAsync();
        btnPdf.Click += async (_, _) => await ExportPdfAsync();
        btnExcelGrid.Click += async (_, _) => await ExportGridExcelAsync();
        btnExcelStock.Click += async (_, _) => await ExportStockExcelAsync();
        Load += async (_, _) =>
        {
            try
            {
                await LoadFilterTypesAsync();
                await LoadGridAsync();
            }
            catch (Exception ex) { ShowError("khởi động", ex); }
        };
        FormClosed += (_, _) => _typingTimer.Dispose();
    }

    // Lay danh sach loai hang tu CSDL, giu lua chon loc hien tai.
    private async Task LoadFilterTypesAsync()
    {
        using var db = DbFactory.Create();
        var types = await db.Set<HangHoa>().AsNoTracking()
            .Where(x => x.LoaiHang != null && x.LoaiHang != "")
            .Select(x => x.LoaiHang!).Distinct().OrderBy(x => x).ToListAsync();

        string previous = cboFilterType.SelectedItem?.ToString() ?? "Tất cả";
        _bindingFilters = true;
        try
        {
            cboFilterType.Items.Clear();
            cboFilterType.Items.Add("Tất cả");
            foreach (string type in types) cboFilterType.Items.Add(type);
            cboFilterType.SelectedItem = cboFilterType.Items.Contains(previous) ? previous : "Tất cả";
        }
        finally { _bindingFilters = false; }
    }

    // Tim kiem va loc ket hop, truy van bat dong bo bang EF Core.
    private async Task LoadGridAsync(int? selectMaHang = null)
    {
        int thisReload = ++_reloadVersion;
        string keyword = txtSearch.Text.Trim();
        string category = cboFilterType.SelectedItem?.ToString() ?? "Tất cả";
        bool onlyActive = chkOnlyActive.Checked;
        int? idToRestore = selectMaHang ?? _selectedMaHang;

        try
        {
            using var db = DbFactory.Create();
            IQueryable<HangHoa> query = db.Set<HangHoa>().AsNoTracking();
            if (keyword.Length > 0) query = query.Where(x => x.TenHang.Contains(keyword));
            if (category != "Tất cả") query = query.Where(x => x.LoaiHang == category);
            if (onlyActive) query = query.Where(x => x.ConKinhDoanh);

            var records = await query.OrderBy(x => x.MaHang)
                .Select(x => new HangHoaRow
                {
                    MaHang = x.MaHang,
                    TenHang = x.TenHang,
                    LoaiHang = x.LoaiHang ?? "",
                    DonVi = x.DonVi ?? "",
                    GiaBan = x.GiaBan,
                    SoLuongTon = x.SoLuongTon,
                    NgayNhap = x.NgayNhap,
                    ConKinhDoanh = x.ConKinhDoanh
                }).ToListAsync();

            // Khong cho truy van cham ghi de ket qua loc moi hon.
            if (thisReload != _reloadVersion || IsDisposed) return;
            _bindingGrid = true;
            try
            {
                dgvHangHoa.DataSource = records;
                dgvHangHoa.ClearSelection();
                _selectedMaHang = null;
            }
            finally { _bindingGrid = false; }

            lblStatus.Text = $"Tìm thấy {records.Count} mặt hàng";
            if (idToRestore.HasValue)
            {
                foreach (DataGridViewRow row in dgvHangHoa.Rows)
                {
                    if (row.DataBoundItem is HangHoaRow model && model.MaHang == idToRestore.Value)
                    {
                        row.Selected = true;
                        dgvHangHoa.CurrentCell = row.Cells[0];
                        _selectedMaHang = model.MaHang;
                        ApplyRowToInputs(model);
                        break;
                    }
                }
            }
        }
        catch (Exception ex) { ShowError("tải / tìm kiếm hàng hóa", ex); }
    }

    private void DgvHangHoa_SelectionChanged(object? sender, EventArgs e)
    {
        if (_bindingGrid || dgvHangHoa.SelectedRows.Count == 0) return;
        if (dgvHangHoa.SelectedRows[0].DataBoundItem is HangHoaRow row)
        {
            _selectedMaHang = row.MaHang;
            ApplyRowToInputs(row);
        }
    }

    private void ApplyRowToInputs(HangHoaRow row)
    {
        txtTenHang.Text = row.TenHang;
        if (cboLoaiHang.Items.Contains(row.LoaiHang)) cboLoaiHang.SelectedItem = row.LoaiHang;
        if (cboDonVi.Items.Contains(row.DonVi)) cboDonVi.SelectedItem = row.DonVi;
        numGiaBan.Value = Math.Clamp(row.GiaBan, numGiaBan.Minimum, numGiaBan.Maximum);
        numSoLuong.Value = Math.Clamp(row.SoLuongTon, (int)numSoLuong.Minimum, (int)numSoLuong.Maximum);
        dtNgayNhap.Value = row.NgayNhap;
        chkConKinhDoanh.Checked = row.ConKinhDoanh;
    }

    private void ResetInputs()
    {
        _selectedMaHang = null;
        dgvHangHoa.ClearSelection();
        txtTenHang.Clear();
        cboLoaiHang.SelectedIndex = 0;
        cboDonVi.SelectedIndex = 0;
        numGiaBan.Value = 0;
        numSoLuong.Value = 0;
        dtNgayNhap.Value = DateTime.Today;
        chkConKinhDoanh.Checked = true;
        txtTenHang.Focus();
    }

    // Kiem tra truoc khi luu; database van co CHECK constraint rieng.
    private bool IsValidInput()
    {
        if (string.IsNullOrWhiteSpace(txtTenHang.Text))
        {
            MessageBox.Show("Tên hàng không được để trống.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtTenHang.Focus();
            return false;
        }
        if (numGiaBan.Value <= 0)
        {
            MessageBox.Show("Giá bán phải lớn hơn 0.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            numGiaBan.Focus();
            return false;
        }
        return true;
    }

    private void FillEntity(HangHoa item)
    {
        item.TenHang = txtTenHang.Text.Trim();
        item.LoaiHang = cboLoaiHang.Text;
        item.DonVi = cboDonVi.Text;
        item.GiaBan = numGiaBan.Value;
        item.SoLuongTon = (int)numSoLuong.Value;
        item.NgayNhap = dtNgayNhap.Value.Date;
        item.ConKinhDoanh = chkConKinhDoanh.Checked;
    }

    private async Task AddAsync()
    {
        if (!IsValidInput()) return;
        try
        {
            using var db = DbFactory.Create();
            var entity = new HangHoa();
            FillEntity(entity);
            db.Set<HangHoa>().Add(entity);
            await db.SaveChangesAsync();
            await LoadFilterTypesAsync();
            ResetInputs();
            await LoadGridAsync();
            MessageBox.Show("Đã thêm hàng hóa.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("thêm hàng hóa", ex); }
    }

    private async Task UpdateAsync()
    {
        if (_selectedMaHang is null)
        {
            MessageBox.Show("Chọn một mặt hàng trên bảng trước khi sửa.");
            return;
        }
        if (!IsValidInput()) return;
        int id = _selectedMaHang.Value;
        try
        {
            using var db = DbFactory.Create();
            var item = await db.Set<HangHoa>().FindAsync(id);
            if (item is null) { MessageBox.Show("Hàng hóa không còn tồn tại."); await LoadGridAsync(); return; }
            FillEntity(item);
            await db.SaveChangesAsync();
            await LoadFilterTypesAsync();
            await LoadGridAsync(id);
            MessageBox.Show("Đã cập nhật hàng hóa.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("sửa hàng hóa", ex); }
    }

    private async Task DeleteAsync()
    {
        if (_selectedMaHang is null)
        {
            MessageBox.Show("Chọn một mặt hàng trên bảng trước khi xóa.");
            return;
        }
        if (MessageBox.Show("Bạn có chắc muốn xóa mặt hàng đã chọn?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            using var db = DbFactory.Create();
            var item = await db.Set<HangHoa>().FindAsync(_selectedMaHang.Value);
            if (item is not null)
            {
                db.Set<HangHoa>().Remove(item);
                await db.SaveChangesAsync();
            }
            ResetInputs();
            await LoadFilterTypesAsync();
            await LoadGridAsync();
            MessageBox.Show("Đã xóa mặt hàng.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError("xóa hàng hóa", ex); }
    }

    private async Task ResetAsync()
    {
        try
        {
            _typingTimer.Stop();
            _bindingFilters = true;
            try
            {
                txtSearch.Clear();
                cboFilterType.SelectedItem = "Tất cả";
                chkOnlyActive.Checked = false;
            }
            finally { _bindingFilters = false; }
            ResetInputs();
            await LoadFilterTypesAsync();
            await LoadGridAsync();
        }
        catch (Exception ex) { ShowError("làm mới", ex); }
    }

    // Report: lay lai hang con ban bang EF va chuyen thanh DataTable.
    private async Task<Report> CreatePriceReportAsync()
    {
        using var db = DbFactory.Create();
        var items = await db.Set<HangHoa>().AsNoTracking()
            .Where(x => x.ConKinhDoanh).OrderBy(x => x.MaHang).ToListAsync();
        if (items.Count == 0)
            throw new InvalidOperationException("Không có mặt hàng đang kinh doanh để in bảng giá.");

        var table = new DataTable("HangHoa");
        table.Columns.Add("TenHang", typeof(string));
        table.Columns.Add("DonVi", typeof(string));
        table.Columns.Add("GiaBan", typeof(decimal));
        foreach (var item in items)
            table.Rows.Add(item.TenHang, item.DonVi ?? "", item.GiaBan);

        string frxPath = Path.Combine(AppContext.BaseDirectory, "Reports", "BangGia.frx");
        if (!File.Exists(frxPath)) throw new FileNotFoundException("Không tìm thấy mẫu báo cáo BangGia.frx", frxPath);

        var report = new Report();
        try
        {
            report.Load(frxPath);
            report.RegisterData(table, "HangHoa");
            var dataSource = report.GetDataSource("HangHoa");
            if (dataSource is null) throw new InvalidOperationException("Báo cáo chưa đăng ký nguồn dữ liệu HangHoa.");
            dataSource.Enabled = true;
            report.SetParameterValue("NgayIn", DateTime.Today.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN")));
            report.Prepare();
            return report;
        }
        catch { report.Dispose(); throw; }
    }

    private async Task PreviewReportAsync()
    {
        try
        {
            using var report = await CreatePriceReportAsync();
            report.ShowPrepared();
        }
        catch (Exception ex) { ShowError("xem trước bảng giá", ex); }
    }

    private async Task ExportPdfAsync()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Lưu bảng giá dưới dạng PDF",
            Filter = "PDF (*.pdf)|*.pdf",
            FileName = "BangGia_UTC2.pdf",
            DefaultExt = "pdf",
            AddExtension = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            using var report = await CreatePriceReportAsync();
            string output = dialog.FileName;
            await Task.Run(() =>
            {
                using var exporter = new PDFExport();
                report.Export(exporter, output);
            });
            if (MessageBox.Show("Đã xuất PDF. Mở file ngay?", "Thành công",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo(output) { UseShellExecute = true });
            }
        }
        catch (IOException ex) { ShowLockedFileError(ex); }
        catch (UnauthorizedAccessException ex) { ShowLockedFileError(ex); }
        catch (Exception ex) { ShowError("xuất PDF", ex); }
    }

    // Excel 1: chi lay cac dong dang hien tren luoi.
    private async Task ExportGridExcelAsync()
    {
        var visible = dgvHangHoa.Rows.Cast<DataGridViewRow>()
            .Where(r => !r.IsNewRow && r.Visible && r.DataBoundItem is HangHoaRow)
            .Select(r => (HangHoaRow)r.DataBoundItem!).ToList();
        if (visible.Count == 0) { MessageBox.Show("Không có dòng nào đang hiển thị để xuất Excel."); return; }
        using var dialog = ExcelSaveDialog("HangHoa_DangHienThi.xlsx");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            using var book = new XLWorkbook();
            var sheet = book.Worksheets.Add("HangHoa");
            PutExcelHeaders(sheet);
            int r = 2;
            foreach (var item in visible)
            {
                sheet.Cell(r, 1).Value = item.MaHang;
                sheet.Cell(r, 2).Value = item.TenHang;
                sheet.Cell(r, 3).Value = item.LoaiHang;
                sheet.Cell(r, 4).Value = item.DonVi;
                sheet.Cell(r, 5).Value = item.GiaBan;
                sheet.Cell(r, 6).Value = item.SoLuongTon;
                sheet.Cell(r, 7).Value = item.NgayNhap;
                sheet.Cell(r, 8).Value = item.ConKinhDoanh ? "Còn bán" : "Ngừng bán";
                r++;
            }
            FormatSheet(sheet, r - 1);
            await Task.Run(() => book.SaveAs(dialog.FileName));
            MessageBox.Show($"Đã xuất {visible.Count} dòng từ bảng hiện tại.", "Thành công");
        }
        catch (IOException ex) { ShowLockedFileError(ex); }
        catch (UnauthorizedAccessException ex) { ShowLockedFileError(ex); }
        catch (Exception ex) { ShowError("xuất Excel từ lưới", ex); }
    }

    // Excel 2: truy van moi 5 hang con kinh doanh; dung cong thuc Excel.
    private async Task ExportStockExcelAsync()
    {
        using var dialog = ExcelSaveDialog("TonKho_UTC2.xlsx");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var db = DbFactory.Create();
            var active = await db.Set<HangHoa>().AsNoTracking()
                .Where(x => x.ConKinhDoanh).OrderBy(x => x.MaHang).ToListAsync();
            if (active.Count == 0) { MessageBox.Show("Không có hàng đang kinh doanh."); return; }

            using var book = new XLWorkbook();
            var ws = book.Worksheets.Add("TonKho");
            string[] titles = ["Mã hàng", "Tên hàng", "Loại hàng", "Đơn vị", "Giá bán", "Số lượng tồn", "Giá trị tồn", "Ngày nhập"];
            for (int i = 0; i < titles.Length; i++) ws.Cell(1, i + 1).Value = titles[i];
            int r = 2;
            foreach (var item in active)
            {
                ws.Cell(r, 1).Value = item.MaHang;
                ws.Cell(r, 2).Value = item.TenHang;
                ws.Cell(r, 3).Value = item.LoaiHang ?? "";
                ws.Cell(r, 4).Value = item.DonVi ?? "";
                ws.Cell(r, 5).Value = item.GiaBan;
                ws.Cell(r, 6).Value = item.SoLuongTon;
                ws.Cell(r, 7).FormulaA1 = $"E{r}*F{r}";
                ws.Cell(r, 8).Value = item.NgayNhap;
                r++;
            }
            int lastDataRow = r - 1;
            ws.Cell(r, 6).Value = "Tổng";
            ws.Cell(r, 7).FormulaA1 = $"SUM(G2:G{lastDataRow})";
            ws.Range(r, 1, r, 8).Style.Font.Bold = true;
            ws.Range(r, 1, r, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
            FormatSheet(ws, lastDataRow);
            ws.Range(2, 7, r, 7).Style.NumberFormat.Format = "#,##0";
            ws.Range(2, 7, r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Column(8).Style.DateFormat.Format = "dd/MM/yyyy";
            await Task.Run(() => book.SaveAs(dialog.FileName));
            MessageBox.Show($"Đã xuất {active.Count} mặt hàng còn kinh doanh và công thức tồn kho.", "Thành công");
        }
        catch (IOException ex) { ShowLockedFileError(ex); }
        catch (UnauthorizedAccessException ex) { ShowLockedFileError(ex); }
        catch (Exception ex) { ShowError("xuất Excel tồn kho", ex); }
    }

    private static SaveFileDialog ExcelSaveDialog(string fileName) => new()
    {
        Title = "Lưu Excel",
        Filter = "Excel Workbook (*.xlsx)|*.xlsx",
        FileName = fileName,
        DefaultExt = "xlsx",
        AddExtension = true
    };

    private static void PutExcelHeaders(IXLWorksheet sheet)
    {
        string[] headers = ["Mã hàng", "Tên hàng", "Loại hàng", "Đơn vị", "Giá bán", "Số lượng tồn", "Ngày nhập", "Kinh doanh"];
        for (int i = 0; i < headers.Length; i++) sheet.Cell(1, i + 1).Value = headers[i];
    }

    private static void FormatSheet(IXLWorksheet sheet, int lastDataRow)
    {
        sheet.Range(1, 1, 1, 8).Style.Font.Bold = true;
        sheet.Range(1, 1, 1, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#DEE8F2");
        sheet.Range(1, 1, 1, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        sheet.Range(1, 1, Math.Max(2, lastDataRow), 8).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        sheet.Range(1, 1, Math.Max(2, lastDataRow), 8).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        if (lastDataRow >= 2) sheet.Range(1, 1, lastDataRow, 8).SetAutoFilter();
        sheet.SheetView.FreezeRows(1);
        sheet.Columns(1, 8).AdjustToContents();
        sheet.Column(2).Width = Math.Max(sheet.Column(2).Width, 28);
        sheet.Column(3).Width = Math.Max(sheet.Column(3).Width, 17);
        sheet.Column(5).Style.NumberFormat.Format = "#,##0";
        sheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        sheet.Column(6).Style.NumberFormat.Format = "#,##0";
        sheet.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        sheet.Column(7).Style.DateFormat.Format = "dd/MM/yyyy";
    }

    private void ShowError(string action, Exception ex) =>
        MessageBox.Show($"Không thể {action}.\n\nChi tiết: {ex.GetBaseException().Message}",
            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void ShowLockedFileError(Exception ex) =>
        MessageBox.Show("Không thể ghi file. Nếu file đang mở bằng Excel hoặc PDF, " +
                        "hãy đóng file rồi thử lại.\n\n" + ex.Message,
            "Không thể lưu file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
