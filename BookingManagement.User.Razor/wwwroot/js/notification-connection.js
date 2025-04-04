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
    console.log("Received notification: " + message);
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
    console.log("Received booking approval: " + message);
    toastr.success(message);
    
    // Nếu đang ở trang BookingRoom, thì cập nhật danh sách
    if (window.location.pathname.includes('/BookingRoom')) {
        // Delay reload để đảm bảo toản thông báo được hiển thị
        setTimeout(() => {
            console.log("Reloading page after booking approval");
            location.reload();
        }, 1000);
    }
    
    // Phát âm thanh thông báo
    playNotificationSound();
});

connection.on("ReceiveBookingRejection", (message, bookingId) => {
    console.log("Received booking rejection: " + message);
    toastr.warning(message);
    
    // Nếu đang ở trang BookingRoom, thì cập nhật danh sách
    if (window.location.pathname.includes('/BookingRoom')) {
        // Delay reload để đảm bảo toản thông báo được hiển thị
        setTimeout(() => {
            console.log("Reloading page after booking rejection");
            location.reload();
        }, 1000);
    }
    
    // Phát âm thanh thông báo
    playNotificationSound();
});

connection.on("ReceiveBookingCompletion", (message, bookingId) => {
    console.log("Received booking completion: " + message);
    toastr.info(message);
    
    // Nếu đang ở trang BookingRoom, thì cập nhật danh sách
    if (window.location.pathname.includes('/BookingRoom')) {
        // Delay reload để đảm bảo toản thông báo được hiển thị
        setTimeout(() => {
            console.log("Reloading page after booking completion");
            location.reload();
        }, 1000);
    }
    
    // Phát âm thanh thông báo
    playNotificationSound();
});

// Khởi động kết nối
async function startConnection() {
    try {
        await connection.start();
        console.log("SignalR Connected");
        
        // Nếu có userId, join vào nhóm riêng
        if (currentUserId) {
            try {
                await connection.invoke("JoinGroup", currentUserId);
                console.log("Joined user group: " + currentUserId);
            } catch (err) {
                console.error("Error joining group: ", err);
            }
        }
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
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

// Hàm phát âm thanh thông báo
function playNotificationSound() {
    try {
        // Tạo một âm thanh đơn giản
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.type = 'sine';
        oscillator.frequency.value = 800;
        gainNode.gain.value = 0.3;

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        oscillator.start(0);
        oscillator.stop(0.1);
        
        console.log("Notification sound played");
    } catch (err) {
        console.error("Error playing notification sound:", err);
    }
}

// Khởi động kết nối khi trang được tải
$(document).ready(function () {
    startConnection();
    
    // Khởi tạo toastr
    toastr.options = {
        "closeButton": true,
        "debug": true,
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
