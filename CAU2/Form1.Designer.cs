using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CAU2;

partial class Form1
{
    private IContainer? components;
    private DateTimePicker dtLocTu = null!;
    private DateTimePicker dtLocDen = null!;
    private ComboBox cboLocBan = null!;
    private ComboBox cboLocTT = null!;
    private TextBox txtLocMon = null!;
    private Button btnTim = null!;

    private DataGridView dgvHoaDon = null!;
    private DataGridView dgvChiTiet = null!;
    private Label lblDoanhThu = null!;
    private Label lblTongCong = null!;
    private Label lblHoaDonDangChon = null!;
    private Label lblStatus = null!;

    private DateTimePicker dtNgayLap = null!;
    private ComboBox cboSoBan = null!;
    private RadioButton rdTienMat = null!;
    private RadioButton rdChuyenKhoan = null!;
    private TextBox txtGhiChu = null!;
    private ComboBox cboMon = null!;
    private NumericUpDown numSoLuong = null!;

    private Button btnHoaDonMoi = null!;
    private Button btnSuaHoaDon = null!;
    private Button btnXoaHoaDon = null!;
    private Button btnThemMon = null!;
    private Button btnSuaSL = null!;
    private Button btnXoaMon = null!;
    private Button btnXemHD = null!;
    private Button btnXuatPDF = null!;
    private Button btnIn = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private static Label Lbl(string text) => new()
    {
        Text = text,
        TextAlign = ContentAlignment.MiddleLeft,
        Dock = DockStyle.Fill,
        Margin = new Padding(3),
        AutoSize = false
    };

    private static Button Btn(string text, int width, bool primary = false) => new()
    {
        Text = text,
        Width = width,
        Height = 37,
        Margin = new Padding(3, 6, 3, 3),
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderColor = Color.FromArgb(197, 208, 222), BorderSize = 1 },
        BackColor = primary ? Color.FromArgb(24, 93, 157) : Color.White,
        ForeColor = primary ? Color.White : Color.FromArgb(30, 43, 61),
        Cursor = Cursors.Hand
    };

    private static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            ColumnHeadersHeight = 44,
            RowTemplate = { Height = 42 },
            EnableHeadersVisualStyles = false,
            GridColor = Color.FromArgb(227, 233, 240),
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 233, 241);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 43, 61);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        grid.AllowUserToResizeColumns = true;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(38, 119, 193);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        return grid;
    }

    private static DataGridViewTextBoxColumn Col(string field, string header, int width, string? format = null)
    {
        var col = new DataGridViewTextBoxColumn
        {
            DataPropertyName = field,
            HeaderText = header,
            Name = "col" + field,
            Width = width,
            MinimumWidth = Math.Max(60, width / 2),
            SortMode = DataGridViewColumnSortMode.NotSortable,
            ReadOnly = true
        };
        if (format is not null) col.DefaultCellStyle.Format = format;
        return col;
    }

    private void InitializeComponent()
    {
        components = new Container();
        SuspendLayout();
        Text = "Quán cà phê Góc Phố – Quản lý hóa đơn";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(246, 248, 252);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1370, 795);
        MinimumSize = new Size(1100, 680);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12, 10, 12, 8),
        };
        // Chừa thêm khoảng trên để tiêu đề GroupBox không bị cắt/đè ở mép form.
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 124));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 61));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        Controls.Add(root);

        // Vung tim kiem giong Hinh 2.1, trang 9.
        var grpSearch = new GroupBox
        {
            Text = "Tìm kiếm hóa đơn",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 3, 0, 0),
            Padding = new Padding(10, 20, 10, 6)
        };
        root.Controls.Add(grpSearch, 0, 0);
        var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 2 };
        for (int i = 0; i < 6; i++) filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, i == 4 ? 24 : i == 5 ? 9 : 16.75f));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        filters.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        grpSearch.Controls.Add(filters);
        string[] labels = ["Từ ngày", "Đến ngày", "Số bàn", "Hình thức thanh toán", "Tên món", ""];
        for (int i = 0; i < 6; i++) filters.Controls.Add(Lbl(labels[i]), i, 0);
        dtLocTu = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Margin = new Padding(3, 4, 9, 6) };
        dtLocDen = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Margin = new Padding(3, 4, 9, 6) };
        cboLocBan = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 4, 9, 6) };
        cboLocTT = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 4, 9, 6) };
        txtLocMon = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Nhập tên món cần tìm...", Margin = new Padding(3, 4, 9, 6) };
        btnTim = Btn("Tìm", 95, true);
        btnTim.Dock = DockStyle.Fill;
        btnTim.Margin = new Padding(3, 3, 3, 6);
        filters.Controls.Add(dtLocTu, 0, 1);
        filters.Controls.Add(dtLocDen, 1, 1);
        filters.Controls.Add(cboLocBan, 2, 1);
        filters.Controls.Add(cboLocTT, 3, 1);
        filters.Controls.Add(txtLocMon, 4, 1);
        filters.Controls.Add(btnTim, 5, 1);

        // Hai khu vuc chinh - phu.
        var middle = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        root.Controls.Add(middle, 0, 1);

        var groupMaster = new GroupBox { Text = "Danh sách hóa đơn", Dock = DockStyle.Fill, Padding = new Padding(9, 16, 9, 9), Margin = new Padding(0, 3, 6, 4) };
        middle.Controls.Add(groupMaster, 0, 0);
        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        groupMaster.Controls.Add(left);
        dgvHoaDon = Grid();
        dgvHoaDon.Columns.Add(Col("SoHoaDon", "Mã HĐ", 110));
        dgvHoaDon.Columns.Add(Col("NgayLap", "Ngày lập", 150, "dd/MM/yyyy HH:mm"));
        dgvHoaDon.Columns.Add(Col("SoBan", "Bàn", 62));
        dgvHoaDon.Columns.Add(Col("HinhThucTt", "Hình thức TT", 128));
        var sumCol = Col("TongTien", "Tổng tiền", 130, "N0");
        sumCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        sumCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgvHoaDon.Columns.Add(sumCol);
        left.Controls.Add(dgvHoaDon, 0, 0);
        lblDoanhThu = new Label
        {
            Text = "Tổng doanh thu đang hiển thị: 0 đ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(21, 67, 115),
            Padding = new Padding(4, 4, 0, 0)
        };
        left.Controls.Add(lblDoanhThu, 0, 1);

        var groupDetail = new GroupBox { Text = "Chi tiết hóa đơn", Dock = DockStyle.Fill, Padding = new Padding(9, 16, 9, 9), Margin = new Padding(6, 3, 0, 4) };
        middle.Controls.Add(groupDetail, 1, 0);
        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 156));
        // Dành đủ chiều cao cho nhãn của khu chọn món, tránh chữ bị đè lên ô nhập.
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 53));
        groupDetail.Controls.Add(right);

        var input = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 4, Padding = new Padding(0, 0, 0, 5) };
        // Cột nhãn rộng hơn để các nhãn tiếng Việt không bị cắt ở DPI/font khác nhau.
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        input.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
        input.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
        input.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        input.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Controls.Add(input, 0, 0);

        lblHoaDonDangChon = new Label
        {
            Text = "Hóa đơn: Chưa chọn",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(21, 67, 115),
            TextAlign = ContentAlignment.MiddleLeft
        };
        input.Controls.Add(lblHoaDonDangChon, 0, 0);
        input.SetColumnSpan(lblHoaDonDangChon, 4);
        dtNgayLap = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowUpDown = true, Margin = new Padding(3, 3, 6, 3) };
        cboSoBan = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 3, 3, 3) };
        input.Controls.Add(Lbl("Ngày lập:"), 0, 1);
        input.Controls.Add(dtNgayLap, 1, 1);
        input.Controls.Add(Lbl("Số bàn:"), 2, 1);
        input.Controls.Add(cboSoBan, 3, 1);
        rdTienMat = new RadioButton { Text = "Tiền mặt", Checked = true, AutoSize = true, Margin = new Padding(3, 5, 8, 2) };
        rdChuyenKhoan = new RadioButton { Text = "Chuyển khoản", AutoSize = true, Margin = new Padding(3, 5, 3, 2) };
        var payRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
        payRow.Controls.Add(rdTienMat);
        payRow.Controls.Add(rdChuyenKhoan);
        input.Controls.Add(Lbl("Thanh toán:"), 0, 2);
        input.Controls.Add(payRow, 1, 2);
        input.SetColumnSpan(payRow, 3);
        txtGhiChu = new TextBox { Dock = DockStyle.Fill, MaxLength = 200, Multiline = true, ScrollBars = ScrollBars.Vertical, Margin = new Padding(3, 2, 3, 2) };
        input.Controls.Add(Lbl("Ghi chú:"), 0, 3);
        input.Controls.Add(txtGhiChu, 1, 3);
        input.SetColumnSpan(txtGhiChu, 3);

        var order = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 2, Padding = new Padding(0, 2, 0, 4) };
        order.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        order.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        order.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 31));
        order.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        order.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5));
        order.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        order.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Controls.Add(order, 0, 1);
        order.Controls.Add(Lbl("Chọn món:"), 0, 0);
        order.Controls.Add(Lbl("Số lượng"), 3, 0);
        cboMon = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 2, 4, 2) };
        numSoLuong = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 1, Maximum = 50, Value = 1, Margin = new Padding(2, 2, 3, 2) };
        order.Controls.Add(cboMon, 1, 1);
        order.Controls.Add(Lbl("SL:"), 2, 1);
        order.Controls.Add(numSoLuong, 3, 1);

        dgvChiTiet = Grid();
        dgvChiTiet.Columns.Add(Col("Stt", "STT", 50));
        var dish = Col("TenMon", "Tên món", 180);
        dish.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgvChiTiet.Columns.Add(dish);
        dgvChiTiet.Columns.Add(Col("SoLuong", "SL", 48));
        var price = Col("DonGia", "Đơn giá", 102, "N0");
        price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvChiTiet.Columns.Add(price);
        var total = Col("ThanhTien", "Thành tiền", 116, "N0");
        total.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvChiTiet.Columns.Add(total);
        right.Controls.Add(dgvChiTiet, 0, 2);
        lblTongCong = new Label
        {
            Text = "Tổng cộng: 0 đ",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(21, 67, 115),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 5, 5, 0)
        };
        right.Controls.Add(lblTongCong, 0, 3);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
        btnHoaDonMoi = Btn("Hóa đơn mới", 119, true);
        btnSuaHoaDon = Btn("Sửa hóa đơn", 120);
        btnXoaHoaDon = Btn("Xóa hóa đơn", 120);
        btnThemMon = Btn("Thêm món", 106, true);
        btnSuaSL = Btn("Sửa SL", 91);
        btnXoaMon = Btn("Xóa món", 98);
        btnXemHD = Btn("Xem hóa đơn", 123);
        btnXuatPDF = Btn("Xuất PDF", 98);
        btnIn = Btn("In...", 78);
        buttons.Controls.AddRange([btnHoaDonMoi, btnSuaHoaDon, btnXoaHoaDon, btnThemMon, btnSuaSL, btnXoaMon, btnXemHD, btnXuatPDF, btnIn]);
        root.Controls.Add(buttons, 0, 2);
        lblStatus = new Label { Text = "Đang kết nối cơ sở dữ liệu...", Dock = DockStyle.Fill, ForeColor = Color.DimGray };
        root.Controls.Add(lblStatus, 0, 3);
        ResumeLayout(false);
    }
}
