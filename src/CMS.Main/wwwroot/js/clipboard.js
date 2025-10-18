// Universal clipboard copy functionality
// Automatically handles click-to-copy for elements with 'tag' class
// Copies the value from data-clipboardvalue attribute

// Global reference to Blazor notification bridge
window.notificationBridgeReference = null;

window.setNotificationBridge = function(dotNetReference) {
    window.notificationBridgeReference = dotNetReference;
};

window.clearNotificationBridge = function() {
    window.notificationBridgeReference = null;
};

document.addEventListener('DOMContentLoaded', function() {
    initializeClipboard();
});

function initializeClipboard() {
    // Use event delegation for dynamic elements
    document.addEventListener('click', function(e) {
        const target = e.target.closest('.tag');
        if (!target) return;

        const clipboardValue = target.getAttribute('data-clipboardvalue');
        if (!clipboardValue) return;

        // Copy to clipboard
        navigator.clipboard.writeText(clipboardValue).then(function() {
            // Show notification via Blazor
            if (window.notificationBridgeReference) {
                window.notificationBridgeReference.invokeMethodAsync('ShowNotification', 'Copied to clipboard', 'info');
            }
        }).catch(function(err) {
            console.error('Failed to copy to clipboard:', err);
            // Show error notification
            if (window.notificationBridgeReference) {
                window.notificationBridgeReference.invokeMethodAsync('ShowNotification', 'Failed to copy to clipboard', 'error');
            }
        });
    });
}

// Export for Blazor if needed
window.clipboard = {
    initialize: initializeClipboard
};
