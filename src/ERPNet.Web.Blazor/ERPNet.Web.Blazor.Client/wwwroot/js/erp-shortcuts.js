let _dotNet = null;
let _handler = null;

// ── Helpers de navegación ──────────────────────────────────

function navegarLista(delta) {
    const items = Array.from(document.querySelectorAll('.erp-lista-cuerpo button'));
    if (items.length === 0) return;
    const activeIdx = items.findIndex(b => b.classList.contains('active'));
    const defaultIdx = delta > 0 ? 0 : items.length - 1;
    const nextIdx = activeIdx < 0 ? defaultIdx : activeIdx + delta;
    if (nextIdx < 0 || nextIdx >= items.length) return;
    items[nextIdx].click();
    items[nextIdx].scrollIntoView({ block: 'nearest' });
}

function navegarListaExtremo(goFirst) {
    const cuerpo = document.querySelector('.erp-lista-cuerpo');
    if (!cuerpo) return;
    const container = cuerpo.firstElementChild; // div interno de VirtualList (el que scrollea)
    if (!container) return;

    function tryScroll(attempts, scrollTo, getTarget) {
        container.scrollTop = scrollTo();
        setTimeout(() => {
            const items = Array.from(container.querySelectorAll('button'));
            const target = getTarget(items, container);
            if (target) {
                target.click();
                target.scrollIntoView({ block: 'nearest' });
            } else if (attempts < 20) {
                tryScroll(attempts + 1, scrollTo, getTarget);
            }
        }, 50);
    }

    if (goFirst) {
        tryScroll(0, () => 0, (items, c) => {
            const top = c.getBoundingClientRect().top;
            return items.length > 0 && (items[0].getBoundingClientRect().top - top) < 150
                ? items[0] : null;
        });
    } else {
        tryScroll(0, () => container.scrollHeight, (items, c) => {
            const atBottom = c.scrollHeight - c.scrollTop - c.clientHeight < 2;
            return atBottom && items.length > 0 ? items[items.length - 1] : null;
        });
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

// ── Helpers del handler ────────────────────────────────────

function invoke(accion) {
    _dotNet.invokeMethodAsync('HandleShortcutAsync', accion);
}

function blurIfEditing(isEditing) {
    if (isEditing) document.activeElement.blur();
}

// ── Grupos de shortcuts ────────────────────────────────────

function handleCtrl(e, isEditing) {
    if (!e.ctrlKey || e.altKey || e.shiftKey) return false;
    switch (e.key) {
        case 'Insert':     invoke('nuevo'); break;
        case 's':          blurIfEditing(isEditing); invoke('guardar'); break;
        case 'Delete':     blurIfEditing(isEditing); invoke('borrar'); break;
        case 'Home':       blurIfEditing(isEditing); navegarListaExtremo(true); break;
        case 'End':        blurIfEditing(isEditing); navegarListaExtremo(false); break;
        case 'ArrowUp':    blurIfEditing(isEditing); navegarLista(-1); break;
        case 'ArrowDown':  blurIfEditing(isEditing); navegarLista(+1); break;
        case 'ArrowLeft':  blurIfEditing(isEditing); navegarTab(-1); break;
        case 'ArrowRight': blurIfEditing(isEditing); navegarTab(+1); break;
        default: return false;
    }
    e.preventDefault();
    return true;
}

function handleAlt(e, isEditing) {
    if (!e.altKey || e.ctrlKey || e.shiftKey) return false;
    // Evitar que Alt active el menú del navegador (Chrome/Edge en Windows)
    if (e.key === 'Alt') { e.preventDefault(); return true; }
    switch (e.key) {
        case 'f': invoke('filtro'); break;
        case 'x': blurIfEditing(isEditing); invoke('limpiarFiltro'); break;
        default: return false;
    }
    e.preventDefault();
    return true;
}

function handlePlain(e, isEditing) {
    if (e.ctrlKey || e.altKey) return false;
    switch (e.key) {
        case 'Escape': invoke('escape'); break;
        case '/':      blurIfEditing(isEditing); invoke('busqueda'); break;
        default: return false;
    }
    e.preventDefault();
    return true;
}

// ── Registro ───────────────────────────────────────────────

export function registerShortcuts(dotNet) {
    _dotNet = dotNet;
    _handler = (e) => {
        const tag = document.activeElement?.tagName?.toLowerCase();
        const isEditing = tag === 'input' || tag === 'textarea' || tag === 'select';
        handleCtrl(e, isEditing) || handleAlt(e, isEditing) || handlePlain(e, isEditing);
    };
    document.addEventListener('keydown', _handler);
}

export function enfocarPrimerCampoDetalle() {
    const panel = document.querySelector('.erp-detail-content, .erp-detail-tabs');
    if (!panel) return;
    const campo = panel.querySelector(
        'input:not([disabled]):not([readonly]):not([type="hidden"]), ' +
        'select:not([disabled]), ' +
        'textarea:not([disabled]):not([readonly])'
    );
    campo?.focus();
}

export function unregisterShortcuts() {
    if (_handler) {
        document.removeEventListener('keydown', _handler);
        _handler = null;
        _dotNet = null;
    }
}
