"use strict";

// Khởi tạo kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/bookingHub")
    .withAutomaticReconnect()
    .build();

// Bắt sự kiện ReceiveNewBooking
connection.on("ReceiveNewBooking", (message, bookingId) => {
    console.log(`Received new booking notification: ${message} for booking ID: ${bookingId}`);
    
    // Hiển thị thông báo
    toastr.info(message);
    
    // Nếu đang ở trang quản lý đặt phòng, thì cập nhật danh sách
    if (window.location.pathname.includes('/Booking')) {
        // Thêm phần tử vào đầu danh sách
        loadNewBookings();
    }
});

// Khởi động kết nối
function startConnection() {
    console.log("Attempting to connect to SignalR hub...");
    connection.start()
        .then(() => {
            console.log("SignalR Connected successfully!");
        })
        .catch(err => {
            console.error("Error connecting to SignalR:", err);
            setTimeout(startConnection, 5000);
        });
}

// Nếu kết nối bị ngắt, thử kết nối lại
connection.onclose(async () => {
    await startConnection();
});

// Hàm tải mới danh sách booking
function loadNewBookings() {
    // Tải lại trang để cập nhật danh sách
    location.reload();
}

// Auto refresh cho trang Booking
function setupAutoRefresh() {
    if (window.location.pathname.includes('/Booking')) {
        console.log("Setting up auto refresh for Booking page...");
        // Tự động refresh mỗi 3 giây
        setInterval(() => {
            console.log("Checking for new bookings...");
            // Kiểm tra xem có booking mới không
            $.ajax({
                url: '/Booking/CheckNewBookings',
                type: 'GET',
                success: function(hasNewBookings) {
                    if (hasNewBookings) {
                        console.log("New bookings found!");
                        // Nếu có booking mới, hiển thị thông báo và tự động refresh
                        toastr.info("Có yêu cầu đặt phòng mới!");
                        setTimeout(() => {
                            loadNewBookings();
                        }, 1000);
                    }
                },
                error: function(xhr, status, error) {
                    console.error("Error checking for new bookings:", error);
                }
            });
        }, 3000); // 3 giây
    }
}

// Khởi động kết nối khi trang được tải
$(document).ready(function () {
    startConnection();
    setupAutoRefresh();
    
    // Khởi tạo toastr
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
});
