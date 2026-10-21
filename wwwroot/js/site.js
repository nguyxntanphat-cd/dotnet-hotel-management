// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ================= KPI CARD =================
(function () {
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    // 1. Số đếm dần từ 0 lên giá trị thật
    //    <span data-count>14</span>  hoặc  <span data-count="175.5" data-decimals="1">175,5</span>
    function animateCount(el, delay) {
        const raw = el.getAttribute("data-count") || el.textContent.trim().replace(/\./g, "").replace(",", ".");
        const target = parseFloat(raw);
        if (!isFinite(target) || reduceMotion) return;

        const decimals = parseInt(el.getAttribute("data-decimals") || "0", 10);
        const fmt = new Intl.NumberFormat("vi-VN", { minimumFractionDigits: decimals, maximumFractionDigits: decimals });
        const duration = 1100;

        el.textContent = fmt.format(0);
        setTimeout(() => {
            const start = performance.now();
            function frame(now) {
                const t = Math.min((now - start) / duration, 1);
                const eased = 1 - Math.pow(1 - t, 4); // easeOutQuart
                el.textContent = fmt.format(target * eased);
                if (t < 1) requestAnimationFrame(frame);
            }
            requestAnimationFrame(frame);
        }, delay);
    }

    document.querySelectorAll(".kpi").forEach(card => {
        const index = parseInt(getComputedStyle(card).getPropertyValue("--i"), 10) || 0;
        card.querySelectorAll("[data-count]").forEach(el => animateCount(el, index * 70 + 150));

        // 2. Vệt sáng đi theo con trỏ
        card.addEventListener("pointermove", e => {
            const r = card.getBoundingClientRect();
            card.style.setProperty("--mx", (e.clientX - r.left) + "px");
            card.style.setProperty("--my", (e.clientY - r.top) + "px");
        });
    });
})();
