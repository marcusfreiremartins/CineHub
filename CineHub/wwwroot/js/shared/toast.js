const toastIcons = {
    success: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24">
                <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>
            </svg>`,
    warning: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24">
                <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z"/>
            </svg>`,
    error: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24">
                <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
            </svg>`,
    info: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/>
            </svg>`
};

function showToast(message, type = 'info', duration = APP_CONFIG.TOAST_DURATION) {

    // Garantir que o container existe
    let container = document.getElementById('toast-container');
    if (!container) {
        console.warn('Container do toast não encontrado, criando...');
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.setAttribute('data-type', type);

    toast.innerHTML = `
        <div class="toast-icon">${toastIcons[type] || toastIcons.info}</div>
        <div class="toast-content">${message}</div>
        <button class="toast-close" type="button">&times;</button>
        <div class="toast-progress"></div>
    `;

    // Event listener para o botão de fechar
    const closeButton = toast.querySelector('.toast-close');
    closeButton.addEventListener('click', function () {
        hideToast(this);
    });

    container.appendChild(toast);

    // Mostrar o toast
    requestAnimationFrame(() => {
        toast.classList.add('show');
    });

    // Auto-hide após o tempo especificado
    setTimeout(() => {
        if (toast.parentNode) {
            hideToast(closeButton);
        }
    }, duration);

    return toast;
}

function hideToast(closeButton) {
    const toast = closeButton.closest('.toast');
    if (!toast) return;

    toast.classList.remove('show');
    toast.classList.add('hide');

    setTimeout(() => {
        if (toast.parentNode) {
            toast.parentNode.removeChild(toast);
        }
    }, 400);
}