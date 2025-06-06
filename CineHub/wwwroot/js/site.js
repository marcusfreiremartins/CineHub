// Inicializações globais da aplicação
document.addEventListener('DOMContentLoaded', function () {

    // Verificar se o container de toast existe, se não, criar
    if (!document.getElementById('toast-container')) {
        const toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container';
        document.body.appendChild(toastContainer);
    }

    // Verificar se deve mostrar mensagem de boas-vindas
    const shouldShowWelcome = document.body.getAttribute('data-show-welcome');
    if (shouldShowWelcome === 'true') {
        setTimeout(() => {
            showToast('Faça login para acessar todas as funcionalidades do CineHub! 🎬', 'info', 7000);
        }, 500);
    }
});