using System.Globalization;

namespace CAU1;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Hien thi ngay thang, tien te theo dinh dang Viet Nam.
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
