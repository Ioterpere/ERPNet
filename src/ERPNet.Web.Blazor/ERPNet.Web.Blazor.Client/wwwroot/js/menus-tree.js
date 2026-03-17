let _dotNetRef = null;
const _sortables = [];

async function ensureSortable() {
    if (window.Sortable) return;
    await new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = 'lib/sortablejs/Sortable.min.js';
        s.onload = resolve;
        s.onerror = () => reject(new Error('No se pudo cargar SortableJS'));
        document.head.appendChild(s);
    });
}

export async function initTree(dotNetRef, rootEl) {
    await ensureSortable();
    _dotNetRef = dotNetRef;
    _initSortables(rootEl);
}

function _initSortables(rootEl) {
    _sortables.forEach(s => s.destroy());
    _sortables.length = 0;
    if (!rootEl) return;
    _createSortable(rootEl);
    rootEl.querySelectorAll('.menu-subtree').forEach(el => _createSortable(el));
}

function _createSortable(el) {
    const s = Sortable.create(el, {
        group: { name: 'menus', pull: true, put: true },
        animation: 150,
        handle: '.drag-handle',
        ghostClass: 'drag-ghost',
        onEnd: (evt) => {
            if (evt.from === evt.to && evt.oldIndex === evt.newIndex) return;
            const menuId = parseInt(evt.item.dataset.id);
            const newParentId = evt.to.dataset.parentId
                ? parseInt(evt.to.dataset.parentId)
                : null;
            const newIndex = evt.newIndex;
            if (_dotNetRef) {
                _dotNetRef.invokeMethodAsync('OnMoverMenuAsync', menuId, newParentId, newIndex);
            }
        }
    });
    _sortables.push(s);
}

export function dispose() {
    _sortables.forEach(s => s.destroy());
    _sortables.length = 0;
    _dotNetRef = null;
}
