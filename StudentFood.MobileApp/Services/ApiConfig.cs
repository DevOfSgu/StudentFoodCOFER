namespace StudentFood.MobileApp.Services;

public static class ApiConfig
{
    public const string LocalHost = "10.0.2.2"; 

    // Đặt thành false để ứng dụng chuyển sang sử dụng tên miền ngrok bên dưới!
    public const bool UseLocal = false; 

    // Đã sửa lại đúng định dạng (loại bỏ khoảng trắng thừa và "https://")
    public const string NgrokHost = "nonstereotyped-biometrical-amir.ngrok-free.dev"; 

    public static string Host => UseLocal ? LocalHost : NgrokHost;
    public static string Scheme => UseLocal ? "http" : "https";
    public static string Port => UseLocal ? ":5148" : "";

    public static string BaseUrl => $"{Scheme}://{Host}{Port}/api";
    public static string ImageBaseUrl => $"{Scheme}://{Host}{Port}";
    public static string LoginUrl => $"{BaseUrl}/Auth/Login";
    public static string RegisterUrl => $"{BaseUrl}/Auth/Register";
    public static string FoodsUrl => $"{BaseUrl}/Foods";
    public static string TrendingFoodsUrl => $"{BaseUrl}/Foods/Trending";
    public static string RecommendedFoodsUrl => $"{BaseUrl}/Foods/Recommended";
    public static string OrdersUrl => $"{BaseUrl}/Orders";
    public static string UsersUrl => $"{BaseUrl}/Users";
}
