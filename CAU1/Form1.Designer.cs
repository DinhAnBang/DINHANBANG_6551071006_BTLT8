using System.Drawing;
using System.Windows.Forms;

namespace CAU1;

partial class Form1
{
    private System.ComponentModel.IContainer? components = null;

    private GroupBox grpInfo = null!;
    private GroupBox grpSearch = null!;
    private TextBox txtTenHang = null!;
    private ComboBox cboLoaiHang = null!;
    private ComboBox cboDonVi = null!;
    private NumericUpDown numGiaBan = null!;
    private NumericUpDown numSoLuong = null!;
    private DateTimePicker dtNgayNhap = null!;
    private CheckBox chkConKinhDoanh = null!;
    private TextBox txtSearch = null!;
    private ComboBox cboFilterType = null!;
    private CheckBox chkOnlyActive = null!;
    private DataGridView dgvHangHoa = null!;
    private Button btnThem = null!;
    private Button btnSua = null!;
    private Button btnXoa = null!;
    private Button btnLamMoi = null!;
    private Button btnPreview = null!;
    private Button btnPdf = null!;
    private Button btnExcelGrid = null!;
    private Button btnExcelStock = null!;
    private StatusStrip status = null!;
    private ToolStripStatusLabel lblStatus = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private static Label LabelOf(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft,
        AutoSize = false,
        Margin = new Padding(3)
    };

    private static Button ActionButton(string title, int width) => new()
    {
        Text = title,
        Width = width,
        Height = 36,
        Margin = new Padding(3, 6, 3, 3),
        FlatStyle = FlatStyle.System
    };

    private static DataGridViewTextBoxColumn TextColumn(string field, string title, int width) => new()
    {
        DataPropertyName = field,
        HeaderText = title,
        Name = "col" + field,
        Width = width,
        ReadOnly = true,
        SortMode = DataGridViewColumnSortMode.Automatic
    };

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        Text = "Quản lý hàng hóa – Tạp hóa UTC2";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1230, 710);
        MinimumSize = new Size(1040, 620);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);

        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12, 10, 12, 8),
            BackColor = BackColor
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 198));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        Controls.Add(main);

        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 0, 0, 7) };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 61));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 39));
        main.Controls.Add(top, 0, 0);

        grpInfo = new GroupBox { Text = "Thông tin hàng hóa", Dock = DockStyle.Fill, Padding = new Padding(10, 15, 10, 9) };
        grpSearch = new GroupBox { Text = "Tìm kiếm và lọc", Dock = DockStyle.Fill, Padding = new Padding(12, 15, 12, 9) };
        top.Controls.Add(grpInfo, 0, 0);
        top.Controls.Add(grpSearch, 1, 0);

        var info = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 49));
        for (int i = 0; i < 4; i++) info.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        grpInfo.Controls.Add(info);

        txtTenHang = new TextBox { Dock = DockStyle.Fill, MaxLength = 100, Margin = new Padding(3, 5, 8, 3) };
        cboLoaiHang = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 5, 3, 3) };
        cboDonVi = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 5, 8, 3) };
        numSoLuong = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100000000, ThousandsSeparator = true, Margin = new Padding(3, 5, 3, 3) };
        numGiaBan = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1000000000000m, Increment = 500, DecimalPlaces = 0, ThousandsSeparator = true, Margin = new Padding(3, 5, 8, 3) };
        dtNgayNhap = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Margin = new Padding(3, 5, 3, 3) };
        chkConKinhDoanh = new CheckBox { Text = "Còn kinh doanh", Dock = DockStyle.Fill, Checked = true, Margin = new Padding(3, 5, 3, 3) };
        info.Controls.Add(LabelOf("Tên hàng:"), 0, 0);
        info.Controls.Add(txtTenHang, 1, 0);
        info.Controls.Add(LabelOf("Loại hàng:"), 2, 0);
        info.Controls.Add(cboLoaiHang, 3, 0);
        info.Controls.Add(LabelOf("Đơn vị:"), 0, 1);
        info.Controls.Add(cboDonVi, 1, 1);
        info.Controls.Add(LabelOf("Số lượng tồn:"), 2, 1);
        info.Controls.Add(numSoLuong, 3, 1);
        info.Controls.Add(LabelOf("Giá bán:"), 0, 2);
        info.Controls.Add(numGiaBan, 1, 2);
        info.Controls.Add(LabelOf("Ngày nhập:"), 2, 2);
        info.Controls.Add(dtNgayNhap, 3, 2);
        info.Controls.Add(chkConKinhDoanh, 1, 3);
        info.SetColumnSpan(chkConKinhDoanh, 3);

        var search = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 4; i++) search.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        grpSearch.Controls.Add(search);
        txtSearch = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Gõ tên hàng cần tìm...", Margin = new Padding(3, 5, 3, 3) };
        cboFilterType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 5, 3, 3) };
        chkOnlyActive = new CheckBox { Text = "Chỉ hiện hàng còn kinh doanh", Dock = DockStyle.Fill, Margin = new Padding(3, 3, 3, 3) };
        search.Controls.Add(LabelOf("Tên hàng:"), 0, 0);
        search.Controls.Add(txtSearch, 1, 0);
        search.Controls.Add(LabelOf("Loại hàng:"), 0, 1);
        search.Controls.Add(cboFilterType, 1, 1);
        search.Controls.Add(chkOnlyActive, 1, 2);
        search.Controls.Add(new Label { Text = "Bộ lọc tên tự chạy sau 350 ms", ForeColor = Color.DimGray, Font = new Font("Segoe UI", 8F), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, 3);

        dgvHangHoa = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
            RowTemplate = { Height = 31 },
            ColumnHeadersHeight = 36,
            EnableHeadersVisualStyles = false
        };
        dgvHangHoa.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 232, 240);
        dgvHangHoa.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        dgvHangHoa.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        dgvHangHoa.Columns.Add(TextColumn("MaHang", "Mã", 60));
        var name = TextColumn("TenHang", "Tên hàng", 240);
        name.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        dgvHangHoa.Columns.Add(name);
        dgvHangHoa.Columns.Add(TextColumn("LoaiHang", "Loại hàng", 160));
        dgvHangHoa.Columns.Add(TextColumn("DonVi", "Đơn vị", 95));
        var price = TextColumn("GiaBan", "Giá bán", 120);
        price.DefaultCellStyle.Format = "N0";
        price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvHangHoa.Columns.Add(price);
        var stock = TextColumn("SoLuongTon", "Tồn", 80);
        stock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvHangHoa.Columns.Add(stock);
        var date = TextColumn("NgayNhap", "Ngày nhập", 120);
        date.DefaultCellStyle.Format = "dd/MM/yyyy";
        dgvHangHoa.Columns.Add(date);
        dgvHangHoa.Columns.Add(new DataGridViewCheckBoxColumn
        {
            Name = "colConKinhDoanh",
            DataPropertyName = "ConKinhDoanh",
            HeaderText = "Kinh doanh",
            Width = 105,
            ReadOnly = true
        });
        main.Controls.Add(dgvHangHoa, 0, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
        btnThem = ActionButton("Thêm", 92);
        btnSua = ActionButton("Sửa", 92);
        btnXoa = ActionButton("Xóa", 92);
        btnLamMoi = ActionButton("Làm mới", 104);
        btnPreview = ActionButton("Xem bảng giá", 136);
        btnPdf = ActionButton("Xuất PDF", 110);
        btnExcelGrid = ActionButton("Xuất Excel (lưới)", 155);
        btnExcelStock = ActionButton("Xuất Excel tồn kho", 165);
        actions.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi, btnPreview, btnPdf, btnExcelGrid, btnExcelStock });
        main.Controls.Add(actions, 0, 2);

        status = new StatusStrip { Dock = DockStyle.Fill, SizingGrip = false };
        lblStatus = new ToolStripStatusLabel { Text = "Đang tải dữ liệu..." };
        status.Items.Add(lblStatus);
        main.Controls.Add(status, 0, 3);
        ResumeLayout(false);
    }
}
