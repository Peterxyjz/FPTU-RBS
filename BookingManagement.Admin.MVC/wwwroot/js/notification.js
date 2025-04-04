// SignalR connection for real-time notifications
let connection = null;

// Initialize the SignalR connection
function initializeSignalRConnection() {
    console.log('Initializing SignalR connection...');
    
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    // Start the connection
    startConnection();

    // Handle connection events
    connection.onreconnecting(error => {
        console.log('Connection lost due to error. Reconnecting...', error);
    });

    connection.onreconnected(connectionId => {
        console.log('Connection reestablished. Connected with ID', connectionId);
        joinAdminGroup();
    });

    connection.onclose(error => {
        console.log('Connection closed due to error:', error);
    });

    // Register event handlers
    registerEventHandlers();
}

// Start the SignalR connection
function startConnection() {
    console.log('Starting SignalR connection...');
    
    connection.start()
        .then(() => {
            console.log('Connected to SignalR hub with ID:', connection.connectionId);
            joinAdminGroup();
        })
        .catch(err => {
            console.error('Error connecting to SignalR hub:', err);
            // Retry connection after a delay
            setTimeout(startConnection, 5000);
        });
}

// Join the admin group to receive admin notifications
function joinAdminGroup() {
    console.log('Attempting to join admin group...');
    
    connection.invoke('JoinAdminGroup')
        .then(() => {
            console.log('Successfully joined admin group');
        })
        .catch(err => {
            console.error('Error joining admin group:', err);
        });
}

// Register all event handlers for incoming notifications
function registerEventHandlers() {
    console.log('Registering event handlers...');
    
    // General notification handler
    connection.on('ReceiveNotification', (title, message, notificationId) => {
        console.log('Received notification:', { title, message, notificationId });
        showNotification(title, message);
        updateUnreadCount();
    });

    // New booking notification handler
    connection.on('NewBookingReceived', (bookingId, userName) => {
        console.log('New booking received:', { bookingId, userName });
        const message = `New booking request from ${userName}`;
        showNotification('New Booking Request', message);
        updateUnreadCount();
        refreshPendingBookingsCount();
    });

    // Booking status change notification handler
    connection.on('BookingStatusChanged', (bookingId, status, title, message) => {
        console.log('Booking status changed:', { bookingId, status, title, message });
        showNotification(title || `Booking Status Changed: ${status}`, message || `Booking #${bookingId} status changed to ${status}`);
        updateUnreadCount();
    });
}

// Display a notification to the user
function showNotification(title, message) {
    console.log('Showing notification:', { title, message });
    
    if (!("Notification" in window)) {
        console.log("This browser does not support desktop notifications");
        return;
    }

    // Check if we have permission to show notifications
    if (Notification.permission === "granted") {
        createNotification(title, message);
    } else if (Notification.permission !== "denied") {
        // Request permission
        Notification.requestPermission().then(permission => {
            if (permission === "granted") {
                createNotification(title, message);
            }
        });
    }

    // Also show an in-app toast notification
    showToast(title, message);
}

// Create a browser notification
function createNotification(title, message) {
    console.log('Creating browser notification:', { title, message });
    
    const notification = new Notification(title, {
        body: message,
        icon: '/shared-images/fptu-logo.png'
    });

    notification.onclick = function () {
        window.focus();
        notification.close();
    };

    // Auto close after 5 seconds
    setTimeout(() => notification.close(), 5000);
}

// Show a toast notification
function showToast(title, message) {
    console.log('Creating toast notification:', { title, message });
    
    // Create a toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    // Create a unique ID for this toast
    const toastId = 'toast-' + Date.now();

    // Create the toast HTML
    toastContainer.innerHTML += `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header bg-primary text-white">
                <strong class="me-auto">${title}</strong>
                <small>Just now</small>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    // Initialize and show the toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement, { delay: 5000 });
    toast.show();

    // Remove the toast element after it's hidden
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// Update the unread notifications count in the UI
function updateUnreadCount() {
    console.log('Updating unread count...');
    
    // Make an AJAX request to get the current unread count
    fetch('/Notification/GetUnreadCount')
        .then(response => response.json())
        .then(count => {
            console.log('Unread count retrieved:', count);
            const badge = document.getElementById('notification-badge');
            if (badge) {
                badge.textContent = count;
                badge.style.display = count > 0 ? 'inline-block' : 'none';
            }
        })
        .catch(error => console.error('Error updating notification count:', error));
}

// Refresh the pending bookings count
function refreshPendingBookingsCount() {
    console.log('Refreshing pending bookings count...');
    
    const pendingCountElement = document.querySelector('.pending-bookings-count');
    if (pendingCountElement) {
        fetch('/Booking/GetPendingCount')
            .then(response => response.json())
            .then(count => {
                console.log('Pending bookings count retrieved:', count);
                pendingCountElement.textContent = count;
                pendingCountElement.style.display = count > 0 ? 'inline-block' : 'none';
            })
            .catch(error => console.error('Error updating pending count:', error));
    }
}

// Initialize when the document is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('Document ready, initializing notification system...');
    
    // Request permission for notifications
    if ("Notification" in window && Notification.permission !== "granted" && Notification.permission !== "denied") {
        Notification.requestPermission();
    }

    // Initialize SignalR
    initializeSignalRConnection();

    // Initial unread count update
    updateUnreadCount();
});
