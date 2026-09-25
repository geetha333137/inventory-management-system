# Notification Button Only MVC

This project contains only the notification module.

Visible standalone UI:
    one notification bell button

Clicking the bell:
    opens the notification dropdown
    shows all notifications
    shows unread count

The dropdown supports:
    mark one as read
    mark all as read
    delete one
    clear all

There is no:
    dashboard
    asset management
    page header
    demo button
    test email button

## Run

    dotnet restore
    dotnet build
    dotnet run

Open:

    https://localhost:7169/Notifications

## Standalone storage

Notifications are stored in:

    notifications.json

No Entity Framework Core or SQLite packages are used.

## Team integration

Your teammate adds the shared partial to their existing layout:

    <partial name="_NotificationBell" />

Then loads:
    wwwroot/css/notification.css
    wwwroot/js/emailjs.js
    wwwroot/js/notifications.js

After successful Add Asset:

    await window.notifyAssetAdded(
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    );

After successful Update Asset:

    await window.notifyAssetUpdated(
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    );

After successful Delete Asset:

    await window.notifyAssetDeleted(
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    );

Call the function only after the real asset operation succeeds.

## EmailJS

Values are in appsettings.json:

    PublicKey
    ServiceId
    TemplateId
    DefaultRecipientEmail

EmailJS template variables:

    {{to_name}}
    {{to_email}}
    {{action}}
    {{asset_name}}
    {{asset_code}}
    {{performed_by}}
    {{date_time}}
    {{message}}

Use the dynamic to_email value as the template recipient.

The EmailJS browser package is loaded from JavaScript instead of
Razor, avoiding Razor parsing issues with the package URL.
