(function () {
    "use strict";

    function markActiveNavigation() {
        var currentPage = window.location.pathname.split("/").pop() || "Default.aspx";
        var links = document.querySelectorAll(".main-nav a");

        for (var i = 0; i < links.length; i += 1) {
            var href = links[i].getAttribute("href");
            if (href && href.toLowerCase() === currentPage.toLowerCase()) {
                links[i].className = (links[i].className + " active").trim();
            }
        }
    }

    function enhanceTables() {
        var tables = document.querySelectorAll(".data-grid");
        for (var i = 0; i < tables.length; i += 1) {
            tables[i].setAttribute("data-enhanced", "true");
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        markActiveNavigation();
        enhanceTables();
    });
}());
