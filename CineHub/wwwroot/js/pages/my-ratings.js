let movieIdToDelete = null;

// Function to confirm deletion of a rating
function confirmDeleteRating(movieId, movieTitle) {
    if (!movieId || !movieTitle) {
        console.error('movieId or movieTitle not provided');
        return;
    }

    movieIdToDelete = movieId;
    const titleElement = document.getElementById('movieTitle');
    if (titleElement) {
        titleElement.textContent = movieTitle;
    }
    showModal('deleteRatingModal');
}

// Function to remove the rating card from the UI
function removeRatingCard(movieId) {

    const deleteButton = document.querySelector(`button[onclick*="confirmDeleteRating(${movieId}"]`);

    if (deleteButton) {

        const cardContainer = deleteButton.closest('.col-12.mb-4');
        if (cardContainer) {
            cardContainer.style.transition = 'all 0.3s ease-out';
            cardContainer.style.opacity = '0';
            cardContainer.style.transform = 'translateX(-100%)';

            setTimeout(() => {
                cardContainer.remove();
                updateRatingsCount();
            }, 300);
        }
    }
}

// Function to update the count of ratings displayed
function updateRatingsCount() {
    const remainingCards = document.querySelectorAll('.col-12.mb-4 .card').length;
    const countElement = document.querySelector('.text-muted small');

    if (countElement) {
        countElement.textContent = `${remainingCards} avaliação(ões) realizada(s)`;
    }

    const evaluateMoreSection = document.getElementById('evaluateMoreSection');
    if (evaluateMoreSection) {
        if (remainingCards === 0) {
            evaluateMoreSection.style.display = 'none';
        }
    }

    if (remainingCards === 0) {
        showEmptyState();
    }
}

// Function to show empty state when there are no more ratings
function showEmptyState() {
    const container = document.querySelector('.row');
    if (container) {
        container.innerHTML = `
            <div class="col-12">
                <div class="text-center py-5">
                    <div class="display-1 text-muted">📝</div>
                    <h3 class="text-muted">Você não tem mais avaliações</h3>
                    <p class="text-muted">Todas as suas avaliações foram removidas. Que tal avaliar mais filmes?</p>
                    <a href="/Movies" class="btn btn-primary">
                        🎬 Explorar Filmes
                    </a>
                </div>
            </div>
        `;
    }
}

// Function to delete a rating using fetch
async function deleteRating(movieId) {
    console.log('deleteRating called with movieId:', movieId);

    if (!movieId) {
        console.error('movieId not provided for deleteRating');
        return;
    }

    try {
        console.log('Making request to:', APP_CONFIG.API.ENDPOINTS.DELETE_RATING);

        const result = await makePostRequest(APP_CONFIG.API.ENDPOINTS.DELETE_RATING, { movieId });

        console.log('Request result:', result);

        if (result && result.success) {
            showToast(result.message || 'Avaliação deletada com sucesso!', 'success');
            removeRatingCard(movieId); // Remove the card from the DOM
        } else {
            const errorMessage = result?.message || 'Erro ao deletar avaliação';
            console.log('Error in response:', errorMessage);
            showToast(errorMessage, 'error');
        }
    } catch (error) {
        console.error('Error in deleteRating:', error);
        showToast('Erro ao deletar avaliação. Tente novamente.', 'error');
    }
}

// Initialization when DOM is fully loaded
document.addEventListener('DOMContentLoaded', function () {
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', async function () {
            if (!movieIdToDelete) {
                console.error('movieIdToDelete not defined');
                showToast('Erro interno: ID do filme não encontrado', 'error'); 
                return;
            }

            // Close modal before starting the process
            hideModal('deleteRatingModal');

            try {
                await deleteRating(movieIdToDelete);
            } catch (error) {
                console.error('Error deleting rating:', error);
                showToast('Erro ao deletar avaliação. Tente novamente.', 'error');
            } finally {
                movieIdToDelete = null;
            }
        });
    }
});