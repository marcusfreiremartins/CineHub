// Requisições HTTP unificadas
async function makePostRequest(url, data) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: typeof data === 'string' ? data : new URLSearchParams(data)
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();
    } catch (error) {
        console.error('Erro na requisição:', error);
        throw error;
    }
}

// Função para animar e remover elementos
function animateAndRemove(element, callback = null) {
    if (!element) return;

    element.style.transition = `opacity ${APP_CONFIG.TIMING.ANIMATION_DURATION}ms ease`;
    element.style.opacity = '0';

    setTimeout(() => {
        if (element.parentNode) {
            element.remove();
        }
        if (callback) callback();
    }, APP_CONFIG.TIMING.ANIMATION_DURATION);
}

// Funções para mostrar/esconder modais Bootstrap
function showModal(modalId) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) {
        console.error(`Modal ${modalId} não encontrado`);
        return null;
    }

    const modal = new bootstrap.Modal(modalElement);
    modal.show();
    return modal;
}

function hideModal(modalId) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) return;

    const modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) modal.hide();
}
