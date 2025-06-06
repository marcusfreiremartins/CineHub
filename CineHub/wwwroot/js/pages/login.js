// Display a welcome toast shortly after page load if the 'data-show-welcome' attribute is set to true
document.addEventListener('DOMContentLoaded', function () {
    const shouldShowWelcome = document.body.getAttribute('data-show-welcome');

    if (shouldShowWelcome === 'true') {
        setTimeout(() => {
            showToast('Faça login para acessar todas as funcionalidades do CineHub! 🎬', 'info', 7000);
        }, 500);
    }
});