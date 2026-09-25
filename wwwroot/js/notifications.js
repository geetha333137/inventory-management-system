/*
    ============================================================
    NOTIFICATION JAVASCRIPT
    ============================================================

    The visible UI contains only the bell.

    This file:
        - opens/closes the panel
        - loads notifications
        - updates unread count
        - marks one/all as read
        - deletes one
        - clears all

    Team integration functions:
        window.notifyAssetAdded()
        window.notifyAssetUpdated()
        window.notifyAssetDeleted()

    Call them after the teammate's real asset operation succeeds.
*/

(function () {

    "use strict";

    const config =
        window.notificationConfig;

    let notifications = [];

    function getToken() {

        return document.querySelector(
            'input[name="__RequestVerificationToken"]'
        )?.value || "";
    }

    async function post(
        url,
        body = null
    ) {

        const options = {

            method:
                "POST",

            headers: {

                RequestVerificationToken:
                    getToken()
            }
        };

        if (body !== null) {

            options.headers[
                "Content-Type"
            ] =
                "application/json";

            options.body =
                JSON.stringify(body);
        }

        const response =
            await fetch(
                url,
                options
            );

        if (!response.ok) {

            let message =
                "Notification request failed.";

            try {

                const data =
                    await response.json();

                message =
                    data.message ||
                    message;

            } catch {
                // Keep default message.
            }

            throw new Error(
                message
            );
        }

        return response;
    }

    /*
        Load notification data from C#.
    */
    async function loadNotifications() {

        const response =
            await fetch(
                config.urls.getAll,
                {
                    cache:
                        "no-store"
                }
            );

        if (!response.ok) {

            throw new Error(
                "Unable to load notifications."
            );
        }

        const data =
            await response.json();

        notifications =
            data.notifications ||
            [];

        renderNotifications();

        updateCount(
            data.unreadCount ||
            0
        );
    }

    /*
        Build HTML rows inside the dropdown.
    */
    function renderNotifications() {

        const list =
            document.getElementById(
                "notificationList"
            );

        if (!list) {
            return;
        }

        list.innerHTML = "";

        if (!notifications.length) {

            list.innerHTML = `
                <div class="empty-notification">
                    No notifications
                </div>
            `;

            return;
        }

        notifications.forEach(
            notification => {

                const row =
                    document.createElement(
                        "div"
                    );

                row.className =
                    `notification-item ${
                        notification.isRead
                            ? "read"
                            : "unread"
                    }`;

                row.innerHTML = `

                    <button
                        type="button"
                        class="notification-delete-button"
                        aria-label="Delete notification">

                        ×

                    </button>

                    <div class="notification-title-text">

                        ${escapeHtml(
                            notification.title
                        )}

                    </div>

                    <div class="notification-message-text">

                        ${escapeHtml(
                            notification.message
                        )}

                    </div>

                    <div class="notification-time-text">

                        ${formatDate(
                            notification.createdAt
                        )}

                    </div>
                `;

                row.addEventListener(
                    "click",
                    () =>
                        markAsRead(
                            notification.id
                        )
                );

                row.querySelector(
                    ".notification-delete-button"
                )?.addEventListener(
                    "click",
                    event => {

                        event.stopPropagation();

                        deleteNotification(
                            notification.id
                        );
                    }
                );

                list.appendChild(
                    row
                );
            }
        );
    }

    function updateCount(
        count
    ) {

        const badge =
            document.getElementById(
                "notificationCount"
            );

        if (!badge) {
            return;
        }

        badge.textContent =
            count;

        badge.classList.toggle(
            "hidden",
            count === 0
        );
    }

    /*
        Public creation method.

        Your teammate may use this after Add/Update/Delete.
    */
    async function createNotification(
        type,
        assetName,
        assetCode
    ) {

        const response =
            await post(
                config.urls.create,
                {
                    type,
                    assetName,
                    assetCode
                }
            );

        await loadNotifications();

        return response.json();
    }

    async function markAsRead(
        id
    ) {

        try {

            await post(
                `${config.urls.markAsRead}?id=${encodeURIComponent(id)}`
            );

            await loadNotifications();

        } catch (error) {

            console.error(
                "Mark as read failed:",
                error
            );
        }
    }

    async function markAllAsRead() {

        try {

            await post(
                config.urls.markAllAsRead
            );

            await loadNotifications();

        } catch (error) {

            console.error(
                "Mark all as read failed:",
                error
            );
        }
    }

    async function deleteNotification(
        id
    ) {

        try {

            await post(
                `${config.urls.delete}?id=${encodeURIComponent(id)}`
            );

            await loadNotifications();

        } catch (error) {

            console.error(
                "Delete notification failed:",
                error
            );
        }
    }

    async function clearAllNotifications() {

        if (!notifications.length) {
            return;
        }

        if (
            !window.confirm(
                "Are you sure you want to clear all notifications?"
            )
        ) {
            return;
        }

        try {

            await post(
                config.urls.clearAll
            );

            await loadNotifications();

        } catch (error) {

            console.error(
                "Clear notifications failed:",
                error
            );
        }
    }

    /*
        Create an in-app notification and send an email.

        Used by the teammate after a real successful operation.
    */
    async function createAndEmailNotification(
        type,
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    ) {

        const actionMap = {

            ADD:
                "Added",

            UPDATE:
                "Updated",

            DELETE:
                "Deleted"
        };

        /*
            Create the in-app notification first.
        */
        await createNotification(
            type,
            assetName,
            assetCode
        );

        /*
            Then send email when a recipient exists.
        */
        const email =
            recipientEmail ||
            config
                .emailJs
                ?.defaultRecipientEmail;

        if (
            email &&
            window.InventoryEmail
        ) {

            await window
                .InventoryEmail
                .sendEmail({

                    action:
                        actionMap[type] ||
                        type,

                    assetName,

                    assetCode,

                    performedBy:
                        performedBy ||
                        "Administrator",

                    recipientEmail:
                        email
                });
        }
    }

    /*
        ========================================================
        TEAMMATE FUNCTIONS
        ========================================================

        Add:
            await window.notifyAssetAdded(
                assetName,
                assetCode,
                performedBy,
                recipientEmail
            );

        Update:
            await window.notifyAssetUpdated(...);

        Delete:
            await window.notifyAssetDeleted(...);
    */

    window.notifyAssetAdded = (
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    ) =>
        createAndEmailNotification(
            "ADD",
            assetName,
            assetCode,
            performedBy,
            recipientEmail
        );

    window.notifyAssetUpdated = (
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    ) =>
        createAndEmailNotification(
            "UPDATE",
            assetName,
            assetCode,
            performedBy,
            recipientEmail
        );

    window.notifyAssetDeleted = (
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    ) =>
        createAndEmailNotification(
            "DELETE",
            assetName,
            assetCode,
            performedBy,
            recipientEmail
        );

    window.NotificationModule = {

        loadNotifications,

        createNotification,

        markAsRead,

        markAllAsRead,

        deleteNotification,

        clearAllNotifications,

        createAndEmailNotification
    };

    /*
        Open and close the dropdown.
    */
    function togglePanel() {

        const panel =
            document.getElementById(
                "notificationPanel"
            );

        const button =
            document.getElementById(
                "notificationButton"
            );

        if (
            !panel ||
            !button
        ) {
            return;
        }

        const open =
            panel.classList.toggle(
                "show"
            );

        button.setAttribute(
            "aria-expanded",
            String(open)
        );

        panel.setAttribute(
            "aria-hidden",
            String(!open)
        );
    }

    function formatDate(
        value
    ) {

        const date =
            new Date(value);

        if (
            Number.isNaN(
                date.getTime()
            )
        ) {
            return "";
        }

        return date.toLocaleString(
            "en-IN",
            {
                day:
                    "2-digit",

                month:
                    "short",

                year:
                    "numeric",

                hour:
                    "2-digit",

                minute:
                    "2-digit"
            }
        );
    }

    function escapeHtml(
        value
    ) {

        const element =
            document.createElement(
                "div"
            );

        element.textContent =
            value ??
            "";

        return element.innerHTML;
    }

    /*
        Initialize the three notification event handlers and
        load current notifications.
    */
    document.addEventListener(
        "DOMContentLoaded",
        async () => {

            document
                .getElementById(
                    "notificationButton"
                )
                ?.addEventListener(
                    "click",
                    togglePanel
                );

            document
                .getElementById(
                    "markAllButton"
                )
                ?.addEventListener(
                    "click",
                    markAllAsRead
                );

            document
                .getElementById(
                    "clearAllButton"
                )
                ?.addEventListener(
                    "click",
                    clearAllNotifications
                );

            try {

                await loadNotifications();

            } catch (error) {

                console.error(
                    "Notification initialization failed:",
                    error
                );
            }
        }
    );

})();
