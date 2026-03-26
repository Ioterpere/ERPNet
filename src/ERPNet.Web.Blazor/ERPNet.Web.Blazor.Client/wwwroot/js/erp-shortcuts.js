let _dotNet = null;
let _handler = null;

function navegarLista(delta) {
    const items = Array.from(document.querySelectorAll('.erp-lista-cuerpo button'));
    if (items.length === 0) return;
    const activeIdx = items.findIndex(b => b.classList.contains('active'));
    const nextIdx = activeIdx < 0
        ? (delta > 0 ? 0 : items.length - 1)
        : activeIdx + delta;
    if (nextIdx < 0 || nextIdx >= items.length) return;
    items[nextIdx].click();
    items[nextIdx].scrollIntoView({ block: 'nearest' });
}

function navegarListaExtremo(goFirst) {
    const cuerpo = document.querySelector('.erp-lista-cuerpo');
    if (!cuerpo) return;
    const container = cuerpo.firstElementChild; // div interno de VirtualList (el que scrollea)
    if (!container) return;
    if (goFirst) {
        function tryStart(attempts) {
            container.scrollTop = 0;
            setTimeout(() => {
                const items = Array.from(container.querySelectorAll('button'));
                const containerTop = container.getBoundingClientRect().top;
                const firstTop = items[0]?.getBoundingClientRect().top ?? Infinity;
                if (items.length > 0 && firstTop - containerTop < 150) {
                    items[0].click();
                    items[0].scrollIntoView({ block: 'nearest' });
                } else if (attempts < 20) {
                    tryStart(attempts + 1);
                }
            }, 50);
        }
        tryStart(0);
    } else {
        function tryEnd(attempts) {
            container.scrollTop = container.scrollHeight;
            setTimeout(() => {
                const atBottom =
                    container.scrollHeight - container.scrollTop - container.clientHeight < 2;
                const items = Array.from(container.querySelectorAll('button'));
                if (atBottom && items.length > 0) {
                    const last = items[items.length - 1];
                    last.click();
                    last.scrollIntoView({ block: 'nearest' });
                } else if (attempts < 20) {
                    tryEnd(attempts + 1);
                }
            }, 50);
        }
        tryEnd(0);
    }
}

function navegarTab(delta) {
    const tabs = Array.from(document.querySelectorAll('.tabs-nav button.nav-link'));
    if (tabs.length === 0) return;
    const activeIdx = tabs.findIndex(t => t.classList.contains('active'));
    if (activeIdx < 0) return;
    const nextIdx = activeIdx + delta;
    if (nextIdx < 0 || nextIdx >= tabs.length) return;
    tabs[nextIdx].click();
}

export function registerShortcuts(dotNet) {
    _dotNet = dotNet;
    _handler = (e) => {
        const tag = document.activeElement?.tagName?.toLowerCase();
        const isEditing = tag === 'input' || tag === 'textarea' || tag === 'select';

        // Evitar que Alt active el menú del navegador (Chrome/Edge en Windows)
        if (e.key === 'Alt' && !e.ctrlKey && !e.shiftKey) {
            e.preventDefault();
            return;
        }

        // Alt+N: nuevo
        if (e.altKey && !e.ctrlKey && !e.shiftKey && e.key === 'n') {
            e.preventDefault();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'nuevo');
            return;
        }

        // Ctrl+S: guardar (sobreescribe el diálogo de guardar del navegador)
        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 's') {
            e.preventDefault();
            // Forzar blur para que @bind de Blazor vacíe valores pendientes antes de guardar
            if (isEditing) document.activeElement.blur();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'guardar');
            return;
        }

        // Alt+Suprimir: borrar (ignorado si el foco está en un campo de texto)
        if (e.altKey && !e.ctrlKey && !e.shiftKey && e.key === 'Delete' && !isEditing) {
            e.preventDefault();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'borrar');
            return;
        }

        // Alt+X: limpiar filtro (ignorado si el foco está en un campo de texto)
        if (e.altKey && !e.ctrlKey && !e.shiftKey && e.key === 'x' && !isEditing) {
            e.preventDefault();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'limpiarFiltro');
            return;
        }

        // Alt+F: filtro
        if (e.altKey && !e.ctrlKey && !e.shiftKey && e.key === 'f') {
            e.preventDefault();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'filtro');
            return;
        }

        // Esc: cerrar modal
        if (!e.altKey && !e.ctrlKey && !e.shiftKey && e.key === 'Escape') {
            e.preventDefault();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'escape');
            return;
        }

        // /: ir al buscador (ignorado si el foco ya está en un campo de texto)
        if (!e.altKey && !e.ctrlKey && e.key === '/' && !isEditing) {
            e.preventDefault();
            _dotNet.invokeMethodAsync('HandleShortcutAsync', 'busqueda');
            return;
        }

        // Ctrl+Inicio/Fin: seleccionar primer/último ítem de la lista
        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 'Home' && !isEditing) {
            e.preventDefault();
            navegarListaExtremo(true);
            return;
        }

        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 'End' && !isEditing) {
            e.preventDefault();
            navegarListaExtremo(false);
            return;
        }

        // Ctrl+Arriba/Abajo: navegar lista lateral (ignorado si el foco está en un campo de texto)
        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 'ArrowUp' && !isEditing) {
            e.preventDefault();
            navegarLista(-1);
            return;
        }

        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 'ArrowDown' && !isEditing) {
            e.preventDefault();
            navegarLista(+1);
            return;
        }

        // Ctrl+Izquierda/Derecha: navegar tabs (ignorado si el foco está en un campo de texto)
        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 'ArrowLeft' && !isEditing) {
            e.preventDefault();
            navegarTab(-1);
            return;
        }

        if (e.ctrlKey && !e.altKey && !e.shiftKey && e.key === 'ArrowRight' && !isEditing) {
            e.preventDefault();
            navegarTab(+1);
            return;
        }
    };
    document.addEventListener('keydown', _handler);
}

export function unregisterShortcuts() {
    if (_handler) {
        document.removeEventListener('keydown', _handler);
        _handler = null;
        _dotNet = null;
    }
}
