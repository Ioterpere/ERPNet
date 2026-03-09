window.sidebar = {
    removeSidebarInit: function () {
        document.documentElement.removeAttribute('data-sidebar-collapsed');
    },
    _shortcutHandler: null,
    registerShortcut: function (dotNetRef) {
        this._shortcutHandler = function (e) {
            if (!e.ctrlKey || e.altKey || e.shiftKey || e.key !== 'b') return;
            const tag = document.activeElement?.tagName;
            if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;
            e.preventDefault();
            dotNetRef.invokeMethodAsync('ToggleSidebarFromJs');
        };
        document.addEventListener('keydown', this._shortcutHandler);
    },
    unregisterShortcut: function () {
        if (this._shortcutHandler) {
            document.removeEventListener('keydown', this._shortcutHandler);
            this._shortcutHandler = null;
        }
    }
};

window.globalShortcut = {
    _handlers: new Map(),
    register: function (id, key, el) {
        const handler = function (e) {
            if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === key) {
                e.preventDefault();
                el.focus();
                el.select();
            }
        };
        document.addEventListener('keydown', handler);
        this._handlers.set(id, handler);
    },
    unregister: function (id) {
        const handler = this._handlers.get(id);
        if (handler) {
            document.removeEventListener('keydown', handler);
            this._handlers.delete(id);
        }
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
    },
    /** Quita el foco del input. */
    blur: function (el) {
        el?.blur();
    }
};
