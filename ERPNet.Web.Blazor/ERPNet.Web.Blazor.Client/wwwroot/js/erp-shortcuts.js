let _dotNet = null;
let _handler = null;

export function registerShortcuts(dotNet) {
    _dotNet = dotNet;
    _handler = (e) => {
        const tag = document.activeElement?.tagName?.toLowerCase();
        const isEditing = tag === 'input' || tag === 'textarea' || tag === 'select';

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
