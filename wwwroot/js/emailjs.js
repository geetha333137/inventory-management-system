/*
    ============================================================
    EMAILJS HELPER
    ============================================================

    EmailJS is loaded dynamically from this JavaScript file.
    This keeps the EmailJS package URL outside Razor.

    Required template variables:

        {{to_name}}
        {{to_email}}
        {{action}}
        {{asset_name}}
        {{asset_code}}
        {{performed_by}}
        {{date_time}}
        {{message}}

    In the EmailJS template, the recipient field should use the
    dynamic to_email value.
*/

(function () {

    "use strict";

    let libraryPromise = null;
    let initialized = false;

    function getConfig() {

        return (
            window
                .notificationConfig
                ?.emailJs
            || {}
        );
    }

    function loadLibrary() {

        if (window.emailjs) {
            return Promise.resolve();
        }

        if (libraryPromise) {
            return libraryPromise;
        }

        libraryPromise =
            new Promise(
                (resolve, reject) => {

                    const script =
                        document.createElement(
                            "script"
                        );

                    /*
                        This file is JavaScript, not Razor, so the
                        normal EmailJS package URL is safe here.
                    */
                    script.src =
                        "https://cdn.jsdelivr.net/npm/@emailjs/browser@4/dist/email.min.js";

                    script.async = true;

                    script.onload = () => {

                        if (window.emailjs) {
                            resolve();
                        } else {
                            reject(
                                new Error(
                                    "EmailJS library loaded but emailjs was not found."
                                )
                            );
                        }
                    };

                    script.onerror = () => {

                        reject(
                            new Error(
                                "EmailJS library could not be loaded."
                            )
                        );
                    };

                    document.head.appendChild(
                        script
                    );
                }
            );

        return libraryPromise;
    }

    async function initialize() {

        if (
            initialized &&
            window.emailjs
        ) {
            return true;
        }

        const config =
            getConfig();

        if (!config.publicKey) {

            console.error(
                "EmailJS PublicKey is missing."
            );

            return false;
        }

        try {

            await loadLibrary();

            window.emailjs.init({
                publicKey:
                    config.publicKey
            });

            initialized = true;

            console.log(
                "EmailJS initialized successfully."
            );

            return true;

        } catch (error) {

            console.error(
                "EmailJS initialization failed:",
                error
            );

            return false;
        }
    }

    async function sendEmail({
        action,
        assetName,
        assetCode,
        performedBy,
        recipientEmail
    }) {

        const config =
            getConfig();

        if (!recipientEmail) {

            console.warn(
                "Email skipped because recipient email is empty."
            );

            return false;
        }

        const ready =
            await initialize();

        if (!ready) {
            return false;
        }

        if (
            !config.serviceId ||
            !config.templateId
        ) {

            console.error(
                "EmailJS ServiceId or TemplateId is missing."
            );

            return false;
        }

        const message =
            action === "Added"
                ? `${assetName} (${assetCode}) has been added to the inventory.`
                : action === "Updated"
                    ? `${assetName} (${assetCode}) has been updated.`
                    : `${assetName} (${assetCode}) has been deleted from the inventory.`;

        /*
            These variable names must match your EmailJS template.
        */
        const templateParams = {

            to_name:
                "Inventory User",

            to_email:
                recipientEmail,

            action,

            asset_name:
                assetName,

            asset_code:
                assetCode,

            performed_by:
                performedBy ||
                "Administrator",

            date_time:
                new Date()
                    .toLocaleString(
                        "en-IN"
                    ),

            message
        };

        try {

            const response =
                await window.emailjs.send(
                    config.serviceId,
                    config.templateId,
                    templateParams
                );

            console.log(
                "EmailJS send successful:",
                recipientEmail,
                response.status,
                response.text
            );

            return true;

        } catch (error) {

            console.error(
                "EmailJS send failed:",
                recipientEmail,
                error
            );

            return false;
        }
    }

    window.InventoryEmail = {

        initialize,

        sendEmail
    };

})();
