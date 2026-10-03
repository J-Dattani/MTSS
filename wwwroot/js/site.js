// =========================================================
// MTSS - Static UI JavaScript
// =========================================================

document.addEventListener("DOMContentLoaded", function () {

    const sidebar = document.getElementById("mtssSidebar");
    const sidebarToggle = document.getElementById("sidebarToggle");

    /*
     * Mobile sidebar
     */
    if (sidebar && sidebarToggle) {

        sidebarToggle.addEventListener("click", function () {

            sidebar.classList.toggle("sidebar-open");

            let overlay =
                document.querySelector(".mtss-sidebar-overlay");

            if (!overlay) {

                overlay =
                    document.createElement("div");

                overlay.className =
                    "mtss-sidebar-overlay";

                document.body.appendChild(overlay);

                overlay.addEventListener("click", function () {

                    sidebar.classList.remove(
                        "sidebar-open"
                    );

                    overlay.classList.remove(
                        "active"
                    );
                });
            }

            overlay.classList.toggle("active");
        });
    }


    /*
     * Static navigation placeholders
     *
     * These links will later be replaced with
     * actual Razor routes/API-integrated pages.
     */
    const staticLinks =
        document.querySelectorAll(".static-nav-link");

    staticLinks.forEach(function (link) {

        link.addEventListener("click", function (event) {

            event.preventDefault();

            showMTSSMessage(
                "This section is currently being built."
            );
        });

    });


    /*
     * Notification button
     */
    const notificationButton =
        document.querySelector(".topbar-icon-button");

    if (notificationButton) {

        notificationButton.addEventListener(
            "click",
            function () {

                showMTSSMessage(
                    "Notifications will be connected later."
                );

            }
        );
    }

});


/*
 * MTSS toast message
 */
function showMTSSMessage(message) {

    const toastElement =
        document.getElementById("mtssToast");

    const messageElement =
        document.getElementById("mtssToastMessage");

    if (!toastElement || !messageElement) {
        return;
    }

    messageElement.textContent = message;

    const toast =
        bootstrap.Toast.getOrCreateInstance(
            toastElement
        );

    toast.show();
}