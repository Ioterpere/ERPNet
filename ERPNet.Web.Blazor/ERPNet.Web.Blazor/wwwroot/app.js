window.sidebar = {
    removeSidebarInit: function () {
        document.documentElement.removeAttribute('data-sidebar-collapsed');
    }
};

window.chat = {
    scrollToBottom: function (id) {
        var el = document.getElementById(id);
        if (el) el.scrollTop = el.scrollHeight;
    },
    hide: function () {
        var el = document.getElementById('chat-offcanvas');
        if (el) bootstrap.Offcanvas.getInstance(el)?.hide();
    },
    registerShortcut: function () {
        var el = document.getElementById('chat-offcanvas');
        if (!el) return;

        // Foco en el textarea cuando el offcanvas termina de abrirse
        el.addEventListener('shown.bs.offcanvas', function () {
            var ta = el.querySelector('textarea.chat-textarea');
            if (ta) ta.focus();
        });

        // Ctrl+Shift+A → toggle del panel
        document.addEventListener('keydown', function (e) {
            if (e.ctrlKey && e.shiftKey && (e.key === 'A' || e.key === 'a')) {
                e.preventDefault();
                bootstrap.Offcanvas.getOrCreateInstance(el).toggle();
            }
        });
    }
};

window.stt = {
    _recognition: null,

    start: function (dotnetRef) {
        var SR = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SR) return false;

        var r = new SR();
        r.lang = 'es-ES';
        r.interimResults = false;

        r.onresult = e => dotnetRef.invokeMethodAsync('OnSpeechResult', e.results[0][0].transcript);
        r.onerror  = () => dotnetRef.invokeMethodAsync('OnSpeechEnd');
        r.onend    = () => dotnetRef.invokeMethodAsync('OnSpeechEnd');

        r.start();
        this._recognition = r;
        return true;
    },

    stop: function () {
        this._recognition?.stop();
        this._recognition = null;
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
