// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Chỉ khóa biểu mẫu POST đã chủ động đăng ký; máy chủ vẫn phải chống trùng.
(() => {
    const pending = new Set();

    function reset(form) {
        pending.delete(form);
        form.removeAttribute("aria-busy");
        form.querySelectorAll("[data-submit-lock]").forEach(button => {
            button.disabled = false;
            button.removeAttribute("data-submit-lock");
        });
        const status = form.querySelector("[data-processing-message]");
        if (status) status.hidden = true;
    }

    document.addEventListener("submit", event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) ||
            !form.matches('form[data-prevent-repeat][method="post" i]')) return;

        if (pending.has(form)) {
            event.preventDefault();
            return;
        }
        if (event.defaultPrevented) return;

        pending.add(form);
        form.setAttribute("aria-busy", "true");
        const status = form.querySelector("[data-processing-message]");
        if (status) status.hidden = false;

        // Chờ hoàn thành sự kiện submit để không làm mất name/value của nút gửi.
        setTimeout(() => {
            if (event.defaultPrevented) {
                reset(form);
                return;
            }
            form.querySelectorAll('button[type="submit"], input[type="submit"]').forEach(button => {
                if (!button.disabled) {
                    button.setAttribute("data-submit-lock", "");
                    button.disabled = true;
                }
            });
        }, 0);
    });

    // Khi người dùng quay lại từ bộ nhớ đệm trình duyệt, cho phép gửi lại.
    window.addEventListener("pageshow", () => {
        for (const form of [...pending]) reset(form);
    });
})();
