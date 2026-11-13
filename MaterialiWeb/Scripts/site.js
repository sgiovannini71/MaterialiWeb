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

    function markActiveCrudSection() {
        var links = document.querySelectorAll('[data-crud-navigation] a');
        if (!links.length) {
            return;
        }

        function selectSection(hash) {
            for (var i = 0; i < links.length; i += 1) {
                var selected = links[i].getAttribute("href") === hash;
                links[i].classList.toggle("active", selected);
                if (selected) {
                    links[i].setAttribute("aria-current", "location");
                } else {
                    links[i].removeAttribute("aria-current");
                }
            }
        }

        function updateFromHash() {
            var hash = window.location.hash;
            if (!hash || hash === "#crud-top") {
                hash = links[0].getAttribute("href");
            }
            selectSection(hash);
        }

        document.addEventListener("click", function (event) {
            var link = event.target.closest('a[href^="#crud-"]');
            if (link) {
                selectSection(link.getAttribute("href"));
            }
        });
        window.addEventListener("hashchange", updateFromHash);
        updateFromHash();
    }

    function enhanceTables() {
        var tables = document.querySelectorAll(".data-grid");
        for (var i = 0; i < tables.length; i += 1) {
            tables[i].setAttribute("data-enhanced", "true");
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        markActiveNavigation();
        markActiveCrudSection();
        enhanceTables();
    });
}());
