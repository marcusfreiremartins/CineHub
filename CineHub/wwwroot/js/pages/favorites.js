// Function to toggle favorite and remove from UI without reload
async function toggleFavorite(movieId, button) {
    if (!movieId || !button) {
        console.error('movieId ou button não fornecidos');
        return;
    }

    try {
        const result = await makePostRequest(APP_CONFIG.API_ENDPOINTS.TOGGLE_FAVORITE, `movieId=${movieId}`);

        if (result.success) {
            const card = button.closest(APP_CONFIG.SELECTORS.MOVIE_CARD);

            if (card) {
                animateAndRemove(card, () => {
                    const remainingCards = document.querySelectorAll(APP_CONFIG.SELECTORS.MOVIE_CARD).length;

                    updateFavoriteCounter(remainingCards);

                    if (remainingCards === 0) {
                        showEmptyFavoriteState();
                    }

                    showToast(result.message || 'Filme removido dos favoritos!', 'success');
                });
            } else {
                showToast(result.message || 'Filme removido dos favoritos!', 'success');
            }
        } else {
            showToast(result.message || 'Erro ao processar favorito', 'error');
        }
    } catch (error) {
        console.error('Erro no toggleFavorite:', error);
        showToast('Erro ao processar favorito. Tente novamente.', 'error');
    }
}

// Update the favorite movies counter
function updateFavoriteCounter(count) {
    const counter = document.querySelector(APP_CONFIG.SELECTORS.COUNTER);
    if (counter) {
        counter.textContent = `${count} filme(s) favorito(s)`;
    }

    const discoverMoreSection = document.getElementById('discoverMoreSection');
    if (discoverMoreSection) {
        if (count === 0) {
            discoverMoreSection.style.display = 'none';
        }
    }
}

// Show empty state when no favorites left
function showEmptyFavoriteState() {
    const container = document.querySelector('.row');
    if (container) {
        container.innerHTML = `
            <div class="col-12">
                <div class="text-center py-5">
                    <div class="display-1 text-muted">💔</div>
                    <h3 class="text-muted">Você não tem mais favoritos</h3>
                    <p class="text-muted">Todos os filmes foram removidos dos seus favoritos. Que tal explorar mais?</p>
                    <a href="/Movies" class="btn btn-primary">
                        🎬 Explorar Filmes
                    </a>
                </div>
            </div>
        `;
    }
}

// Checks for a pending toast in sessionStorage after reload
function checkAndShowPendingToast() {
    const message = sessionStorage.getItem('toastMessage');
    const type = sessionStorage.getItem('toastType');

    if (message && type) {
        sessionStorage.removeItem('toastMessage');
        sessionStorage.removeItem('toastType');

        setTimeout(() => {
            showToast(message, type);
        }, 200);
    }
}

// Animation function to smoothly remove the card
function animateAndRemove(element, callback) {
    if (!element) return;

    element.style.transition = 'all 0.3s ease-out';
    element.style.opacity = '0';
    element.style.transform = 'translateX(-100%)';

    setTimeout(() => {
        element.remove();
        if (typeof callback === 'function') {
            callback();
        }
    }, 300);
}

// Initialization on page load
document.addEventListener('DOMContentLoaded', function () {
    checkAndShowPendingToast();
});