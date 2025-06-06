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

                    if (remainingCards === 0) {
                        location.reload();
                    } else {
                        updateFavoriteCounter(remainingCards);
                    }
                });
            }

            showToast(result.message, 'success');
        } else {
            showToast(result.message || 'Erro ao processar favorito', 'error');
        }
    } catch (error) {
        console.error('Erro no toggleFavorite:', error);
        showToast('Erro ao processar favorito. Tente novamente.', 'error');
    }
}

function updateFavoriteCounter(count) {
    const counter = document.querySelector(APP_CONFIG.SELECTORS.COUNTER);
    if (counter) {
        counter.textContent = `${count} filme(s) favorito(s)`;
    }
}