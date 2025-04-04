"use strict";

// Khởi tạo kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Người dùng hiện tại từ dữ liệu trong DOM
const currentUserId = document.getElementById('current-user-id')?.value;

// Bắt sự kiện ReceiveNotification
connection.on("ReceiveNotification", (message) => {
    // Hiển thị thông báo
    toastr.info(message);
    
    // Nếu đang ở trang thông báo, thì cập nhật danh sách
    if (window.location.pathname.includes('/Notifications')) {
        location.reload();
    } else {
        // Cập nhật số lượng thông báo chưa đọc
        updateUnreadNotificationCount();
    }
});

// Bắt sự kiện trạng thái đặt phòng
connection.on("ReceiveBookingApproval", (message, bookingId) => {
    toastr.success(message);
    
    // Nếu đang ở trang BookingRoom, thì cập nhật danh sách
    if (window.location.pathname.includes('/BookingRoom')) {
        location.reload();
    }
});

connection.on("ReceiveBookingRejection", (message, bookingId) => {
    toastr.warning(message);
    
    // Nếu đang ở trang BookingRoom, thì cập nhật danh sách
    if (window.location.pathname.includes('/BookingRoom')) {
        location.reload();
    }
});

connection.on("ReceiveBookingCompletion", (message, bookingId) => {
    toastr.info(message);
    
    // Nếu đang ở trang BookingRoom, thì cập nhật danh sách
    if (window.location.pathname.includes('/BookingRoom')) {
        location.reload();
    }
});

// Khởi động kết nối
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected");
        
        // Nếu có userId, join vào nhóm riêng
        if (currentUserId) {
            await connection.invoke("JoinGroup", currentUserId);
            console.log("Joined user group: " + currentUserId);
        }
    } catch (err) {
        console.error(err);
        setTimeout(startConnection, 5000);
    }
}

// Nếu kết nối bị ngắt, thử kết nối lại
connection.onclose(async () => {
    await startConnection();
});

// Hàm cập nhật số lượng thông báo chưa đọc
function updateUnreadNotificationCount() {
    if (currentUserId) {
        $.ajax({
            url: '/Notifications?handler=UnreadCount',
            type: 'GET',
            success: function (count) {
                const badge = document.getElementById('notification-badge');
                if (badge) {
                    badge.innerText = count;
                    badge.style.display = count > 0 ? 'inline-block' : 'none';
                }
            }
        });
    }
}

// Khởi động kết nối khi trang được tải
$(document).ready(function () {
    startConnection();
    
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
