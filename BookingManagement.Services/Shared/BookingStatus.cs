namespace BookingManagement.Services.Shared
{
    public static class BookingStatus
    {
        public const int Pending = 1;      // Chờ duyệt
        public const int Approved = 2;     // Đã duyệt
        public const int Rejected = 3;     // Từ chối
        public const int Completed = 4;    // Đã hoàn thành
        public const int Cancelled = 5;    // Đã hủy

        public static string GetStatusText(int status)
        {
            return status switch
            {
                Pending => "Chờ duyệt",
                Approved => "Đã duyệt",
                Rejected => "Từ chối",
                Completed => "Đã hoàn thành",
                Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }
    }
}
