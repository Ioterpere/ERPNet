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

window.chat = {
    scrollToBottom: function (id) {
        const el = document.getElementById(id);
        if (el) el.scrollTop = el.scrollHeight;
    },
    _shortcutHandler: null,
    _shownHandler: null,
    registerShortcut: function () {
        this._shortcutHandler = function (e) {
            if (!e.ctrlKey || !e.shiftKey || e.altKey || e.key !== 'A') return;
            const tag = document.activeElement?.tagName;
            if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;
            e.preventDefault();
            const el = document.getElementById('chat-offcanvas');
            if (el) bootstrap.Offcanvas.getOrCreateInstance(el).toggle();
        };
        document.addEventListener('keydown', this._shortcutHandler);
        this._shownHandler = function (e) {
            if (e.target.id !== 'chat-offcanvas') return;
            e.target.querySelector('textarea')?.focus();
        };
        document.addEventListener('shown.bs.offcanvas', this._shownHandler);
    },
    hide: function () {
        const el = document.getElementById('chat-offcanvas');
        if (el) bootstrap.Offcanvas.getOrCreateInstance(el).hide();
    },
    clickFileInput: function (id) {
        document.getElementById(id)?.click();
    },
    unregisterShortcut: function () {
        if (this._shortcutHandler) {
            document.removeEventListener('keydown', this._shortcutHandler);
            this._shortcutHandler = null;
        }
        if (this._shownHandler) {
            document.removeEventListener('shown.bs.offcanvas', this._shownHandler);
            this._shownHandler = null;
        }
    }
};

window.stt = {
    _recognition: null,
    start: function (dotNetRef) {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) return false;

        const recognition = new SpeechRecognition();
        recognition.lang = 'es-ES';
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        recognition.onresult = function (e) {
            const texto = e.results[0][0].transcript;
            dotNetRef.invokeMethodAsync('OnSpeechResult', texto);
        };
        recognition.onend = function () {
            dotNetRef.invokeMethodAsync('OnSpeechEnd');
        };
        recognition.onerror = function () {
            dotNetRef.invokeMethodAsync('OnSpeechEnd');
        };

        recognition.start();
        this._recognition = recognition;
        return true;
    },
    stop: function () {
        if (this._recognition) {
            this._recognition.stop();
            this._recognition = null;
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
