// Global application initializations on DOM ready
document.addEventListener('DOMContentLoaded', function () {

    // Check if toast container exists, create if not
    if (!document.getElementById('toast-container')) {
        const toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container';
        document.body.appendChild(toastContainer);
    }

    // Show welcome message if flag is set
    const shouldShowWelcome = document.body.getAttribute('data-show-welcome');
    if (shouldShowWelcome === 'true') {
        setTimeout(() => {
            showToast('Faça login para acessar todas as funcionalidades do CineHub! 🎬', 'info', 7000);
        }, 500);
    }
});