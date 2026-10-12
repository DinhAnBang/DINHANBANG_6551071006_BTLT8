-- ============================================================
-- CAU 1 - QUAN LY TIEM TAP HOA NHA TRUONG UTC2
-- SQL Server LocalDB | Database First (EF Core)
-- ============================================================

-- 1. Tao CSDL neu chua ton tai.
if db_id(N'TiemTapHoa') is null
begin
    exec(N'create database [TiemTapHoa]');
end;
go

use [TiemTapHoa];
go

-- 2. Tao bang HangHoa neu chua ton tai.
if object_id(N'dbo.HangHoa', N'U') is null
begin
    create table dbo.HangHoa (
        MaHang int identity(1,1) not null,
        TenHang nvarchar(100) not null,
        LoaiHang nvarchar(50) null,
        DonVi nvarchar(20) null,
        GiaBan decimal(18,0) not null,
        SoLuongTon int not null,
        NgayNhap datetime not null,
        ConKinhDoanh bit not null,

        constraint PK_HangHoa primary key (MaHang),
        constraint CK_HangHoa_TenHang check (len(ltrim(rtrim(TenHang))) > 0),
        constraint CK_HangHoa_GiaBan check (GiaBan > 0),
        constraint CK_HangHoa_SoLuongTon check (SoLuongTon >= 0)
    );
end;
go

-- 3. Them 6 hang hoa mau khi bang dang rong.
if not exists (select 1 from dbo.HangHoa)
begin
    insert into dbo.HangHoa
        (TenHang, LoaiHang, DonVi, GiaBan, SoLuongTon, NgayNhap, ConKinhDoanh)
    values
        (N'Nước suối Aquafina 500ml', N'Đồ uống', N'Chai', 6000, 120, '20261002', 1),
        (N'Mì Hảo Hảo tôm chua cay', N'Thực phẩm khô', N'Gói', 4500, 200, '20261002', 1),
        (N'Dầu ăn Tường An 1L', N'Gia vị', N'Chai', 48000, 35, '20261005', 1),
        (N'Bột giặt OMO 800g', N'Đồ gia dụng', N'Túi', 52000, 40, '20261005', 1),
        (N'Sữa tươi Vinamilk 1L', N'Đồ uống', N'Hộp', 32000, 60, '20261008', 1),
        (N'Nước mắm Nam Ngư 500ml', N'Gia vị', N'Chai', 38000, 25, '20261001', 0);
end;
go

-- ============================================================
-- 4. CAC TRUY VAN KIEM TRA DU LIEU
-- ============================================================

-- 4.1. Tat ca hang hoa: mong doi 6 dong.
select MaHang, TenHang, LoaiHang, DonVi, GiaBan,
       SoLuongTon, NgayNhap, ConKinhDoanh
from dbo.HangHoa
order by MaHang;

-- 4.2. Bang gia dang kinh doanh: mong doi 5 dong.
select MaHang, TenHang, DonVi, GiaBan
from dbo.HangHoa
where ConKinhDoanh = 1
order by MaHang;

-- 4.3. Tong gia tri ton kho dang kinh doanh: 7.300.000 d.
select sum(GiaBan * SoLuongTon) as TongGiaTriTon
from dbo.HangHoa
where ConKinhDoanh = 1;

-- 4.4. Tim ten chua 'nuoc': mong doi 2 dong.
select MaHang, TenHang, LoaiHang, GiaBan, ConKinhDoanh
from dbo.HangHoa
where TenHang like N'%nước%'
order by MaHang;

-- 4.5. Tim 'nuoc' va chi hang dang kinh doanh: 1 dong.
select MaHang, TenHang, GiaBan
from dbo.HangHoa
where TenHang like N'%nước%' and ConKinhDoanh = 1
order by MaHang;
