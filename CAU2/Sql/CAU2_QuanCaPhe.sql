-- =============================================================
-- CAU 2 - QUAN CA PHE GOC PHO: QUAN LY HOA DON
-- CSDL: QuanCaPhe | SQL Server Express / LocalDB
-- Gom 3 bang: Mon, HoaDon, ChiTietHoaDon
-- Chay toan bo script trong SSMS bang phim F5.
-- Chay lai an toan: khong xoa du lieu va khong chen trung bo mau.
-- =============================================================

-- 1. Tao co so du lieu neu chua co
use master;
go

if db_id(N'QuanCaPhe') is null
begin
    exec(N'create database [QuanCaPhe]');
end;
go

use QuanCaPhe;
go

-- 2. Danh muc mon
if object_id(N'dbo.Mon', N'U') is null
begin
    create table dbo.Mon (
        MaMon int not null,
        TenMon nvarchar(100) not null,
        NhomMon nvarchar(30) not null,
        DonGia decimal(18,0) not null,
        constraint PK_Mon primary key (MaMon),
        constraint CK_Mon_TenMon check (len(ltrim(rtrim(TenMon))) > 0),
        constraint CK_Mon_DonGia check (DonGia > 0)
    );
end;
go

-- 3. Hoa don (khong luu TongTien, tong tien tinh tu chi tiet)
if object_id(N'dbo.HoaDon', N'U') is null
begin
    create table dbo.HoaDon (
        MaHD int identity(1,1) not null,
        NgayLap datetime not null,
        SoBan int not null,
        HinhThucTT nvarchar(20) not null,
        GhiChu nvarchar(200) null,
        constraint PK_HoaDon primary key (MaHD),
        constraint CK_HoaDon_SoBan check (SoBan between 1 and 10),
        constraint CK_HoaDon_HinhThucTT
            check (HinhThucTT in (N'Tiền mặt', N'Chuyển khoản'))
    );
end;
go

-- 4. Chi tiet hoa don (luu lai don gia tai thoi diem ban)
if object_id(N'dbo.ChiTietHoaDon', N'U') is null
begin
    create table dbo.ChiTietHoaDon (
        MaCT int identity(1,1) not null,
        MaHD int not null,
        MaMon int not null,
        SoLuong int not null,
        DonGia decimal(18,0) not null,
        constraint PK_ChiTietHoaDon primary key (MaCT),
        constraint FK_ChiTietHoaDon_HoaDon
            foreign key (MaHD) references dbo.HoaDon(MaHD),
        constraint FK_ChiTietHoaDon_Mon
            foreign key (MaMon) references dbo.Mon(MaMon),
        constraint CK_ChiTietHoaDon_SoLuong check (SoLuong > 0),
        constraint CK_ChiTietHoaDon_DonGia check (DonGia > 0)
    );
end;
go

-- 5. Chi muc ho tro tim kiem hoa don va mon
if not exists (
    select 1 from sys.indexes
    where object_id = object_id(N'dbo.HoaDon')
      and name = N'IX_HoaDon_NgayLap'
)
    create index IX_HoaDon_NgayLap on dbo.HoaDon(NgayLap);
go

if not exists (
    select 1 from sys.indexes
    where object_id = object_id(N'dbo.ChiTietHoaDon')
      and name = N'IX_ChiTietHoaDon_MaHD'
)
    create index IX_ChiTietHoaDon_MaHD on dbo.ChiTietHoaDon(MaHD);
go

if not exists (
    select 1 from sys.indexes
    where object_id = object_id(N'dbo.ChiTietHoaDon')
      and name = N'IX_ChiTietHoaDon_MaMon'
)
    create index IX_ChiTietHoaDon_MaMon on dbo.ChiTietHoaDon(MaMon);
go

-- 6. Chen du 5 mon, 2 hoa don, 5 dong chi tiet theo de bai
-- Chi chen bo mau khi ca 3 bang dang rong, tranh sua du lieu da nhap.
set xact_abort on;
go

begin try
    begin transaction;

    if not exists (select 1 from dbo.Mon)
       and not exists (select 1 from dbo.HoaDon)
       and not exists (select 1 from dbo.ChiTietHoaDon)
    begin
        insert into dbo.Mon (MaMon, TenMon, NhomMon, DonGia)
        values
            (1, N'Cà phê sữa đá', N'Cà phê', 29000),
            (2, N'Bạc xỉu', N'Cà phê', 32000),
            (3, N'Trà đào cam sả', N'Trà', 45000),
            (4, N'Bánh tiramisu', N'Bánh', 39000),
            (5, N'Nước suối', N'Nước đóng chai', 12000);

        insert into dbo.HoaDon (NgayLap, SoBan, HinhThucTT, GhiChu)
        values ('2026-10-09T08:15:00', 3, N'Tiền mặt', null);
        declare @MaHD1 int = convert(int, scope_identity());

        insert into dbo.HoaDon (NgayLap, SoBan, HinhThucTT, GhiChu)
        values ('2026-10-09T09:40:00', 5, N'Chuyển khoản', null);
        declare @MaHD2 int = convert(int, scope_identity());

        insert into dbo.ChiTietHoaDon (MaHD, MaMon, SoLuong, DonGia)
        values
            (@MaHD1, 1, 2, 29000), -- 2 Ca phe sua da = 58.000
            (@MaHD1, 3, 1, 45000), -- 1 Tra dao cam sa = 45.000
            (@MaHD1, 4, 1, 39000), -- 1 Banh tiramisu = 39.000
            (@MaHD2, 2, 1, 32000), -- 1 Bac xiu = 32.000
            (@MaHD2, 5, 2, 12000);-- 2 Nuoc suoi = 24.000

        print N'Da tao du lieu mau: 5 mon, 2 hoa don, 5 chi tiet.';
    end
    else
    begin
        print N'Da co du lieu. Bo qua buoc chen du lieu mau de khong chen trung.';
    end;

    commit transaction;
end try
begin catch
    if @@trancount > 0 rollback transaction;
    throw;
end catch;
go

-- =============================================================
-- 7. TRUY VAN KIEM TRA (khong thay doi du lieu)
-- =============================================================

-- 7.1 Danh sach mon: mong doi 5 dong
select MaMon, TenMon, NhomMon, DonGia
from dbo.Mon
order by MaMon;

-- 7.2 Danh sach hoa don va tong tien, khong co cot TongTien trong DB
-- Mong doi HD000001 = 142000, HD000002 = 56000
select
    N'HD' + right('000000' + cast(h.MaHD as varchar(10)), 6) as SoHoaDon,
    h.NgayLap,
    h.SoBan,
    h.HinhThucTT,
    h.GhiChu,
    coalesce(sum(ct.SoLuong * ct.DonGia), 0) as TongTien
from dbo.HoaDon as h
left join dbo.ChiTietHoaDon as ct on ct.MaHD = h.MaHD
group by h.MaHD, h.NgayLap, h.SoBan, h.HinhThucTT, h.GhiChu
order by h.MaHD;

-- 7.3 Chi tiet hoa don, don gia lay tu chi tiet (gia luc ban)
select
    N'HD' + right('000000' + cast(h.MaHD as varchar(10)), 6) as SoHoaDon,
    ct.MaCT,
    m.TenMon,
    ct.SoLuong,
    ct.DonGia,
    ct.SoLuong * ct.DonGia as ThanhTien
from dbo.ChiTietHoaDon as ct
join dbo.HoaDon as h on h.MaHD = ct.MaHD
join dbo.Mon as m on m.MaMon = ct.MaMon
order by h.MaHD, ct.MaCT;

-- 7.4 Loc hoa don co mon tiramisu: mong doi chi co HD000001
select distinct h.MaHD, h.NgayLap, h.SoBan, h.HinhThucTT
from dbo.HoaDon as h
join dbo.ChiTietHoaDon as ct on ct.MaHD = h.MaHD
join dbo.Mon as m on m.MaMon = ct.MaMon
where m.TenMon like N'%tiramisu%';

-- 7.5 Loc hoa don chuyen khoan: mong doi chi co HD000002
select MaHD, NgayLap, SoBan, HinhThucTT
from dbo.HoaDon
where HinhThucTT = N'Chuyển khoản';

-- 7.6 Dem ban ghi
select
    (select count(*) from dbo.Mon) as SoMon,
    (select count(*) from dbo.HoaDon) as SoHoaDon,
    (select count(*) from dbo.ChiTietHoaDon) as SoDongChiTiet;
