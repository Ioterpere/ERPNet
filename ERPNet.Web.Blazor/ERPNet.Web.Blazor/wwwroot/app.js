window.sidebar = {
    removeSidebarInit: function () {
        document.documentElement.removeAttribute('data-sidebar-collapsed');
    }
};

window.itemSelector = {
    /** Registra preventDefault para ArrowDown/ArrowUp/Escape en el input. */
    registerKeys: function (el) {
        el.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowDown' || e.key === 'ArrowUp' || e.key === 'Escape') {
                e.preventDefault();
            }
        });
    },
    /** Desplaza el item activo para que sea visible en el dropdown. */
    scrollIntoView: function (id) {
        document.getElementById(id)?.scrollIntoView({ block: 'nearest' });
    }
};
