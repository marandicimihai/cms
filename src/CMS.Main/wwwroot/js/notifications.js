// Notification management with JS interop
window.notificationManager = {
    timers: {},

    /**
     * Schedule a notification to be removed after a delay
     * @param {string} notificationId - The GUID of the notification
     * @param {number} delayMs - Delay in milliseconds before removal
     * @param {object} dotNetReference - The .NET component reference
     */
    scheduleRemoval: function (notificationId, delayMs, dotNetReference) {
        // Clear any existing timer for this notification
        if (this.timers[notificationId]) {
            clearTimeout(this.timers[notificationId]);
        }

        // Schedule the removal
        this.timers[notificationId] = setTimeout(() => {
            dotNetReference.invokeMethodAsync('RemoveNotificationFromJs', notificationId);
            delete this.timers[notificationId];
        }, delayMs);
    },

    /**
     * Cancel a scheduled removal for a notification
     * @param {string} notificationId - The GUID of the notification
     */
    cancelRemoval: function (notificationId) {
        if (this.timers[notificationId]) {
            clearTimeout(this.timers[notificationId]);
            delete this.timers[notificationId];
        }
    },

    /**
     * Clear all scheduled removals
     */
    clearAll: function () {
        Object.keys(this.timers).forEach(id => {
            clearTimeout(this.timers[id]);
        });
        this.timers = {};
    }
};
