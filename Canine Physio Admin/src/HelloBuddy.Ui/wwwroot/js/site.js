// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Back link handler removed (ERR-AT-009): history.back() replaced by explicit
// stable hrefs on each page. The [data-back-link] attribute is no longer used.

// Toast notifications: top-right fading success/error popups (Bootstrap toasts).
// Server-rendered toasts (from TempData) are shown on load; window.showToast lets
// client-side flows (e.g. the async programme builder) raise toasts dynamically.
(function () {
    function getContainer() {
        var container = document.querySelector('.app-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container app-toast-container position-fixed top-0 end-0 p-3';
            container.setAttribute('aria-live', 'polite');
            container.setAttribute('aria-atomic', 'true');
            document.body.appendChild(container);
        }
        return container;
    }

    function showToastElement(el) {
        if (!window.bootstrap || !window.bootstrap.Toast) {
            return;
        }
        var toast = window.bootstrap.Toast.getOrCreateInstance(el);
        el.addEventListener('hidden.bs.toast', function () {
            el.remove();
        });
        toast.show();
    }

    window.showToast = function (message, kind) {
        if (!message) {
            return;
        }
        var container = getContainer();
        var el = document.createElement('div');
        var variant = kind === 'error' ? 'text-bg-danger' : 'text-bg-success';
        el.className = 'toast border-0 app-toast ' + variant;
        el.setAttribute('role', kind === 'error' ? 'alert' : 'status');
        el.setAttribute('aria-live', kind === 'error' ? 'assertive' : 'polite');
        el.setAttribute('aria-atomic', 'true');
        el.setAttribute('data-bs-delay', '6000');

        var inner = document.createElement('div');
        inner.className = 'd-flex';
        var body = document.createElement('div');
        body.className = 'toast-body';
        body.textContent = message;
        var close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-close btn-close-white me-2 m-auto';
        close.setAttribute('data-bs-dismiss', 'toast');
        close.setAttribute('aria-label', 'Close');

        inner.appendChild(body);
        inner.appendChild(close);
        el.appendChild(inner);
        container.appendChild(el);
        showToastElement(el);
    };

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.app-toast-container .toast').forEach(showToastElement);
    });
})();
