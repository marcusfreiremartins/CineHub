let movieIdToDelete = null;

// Função para confirmar exclusão de avaliação
function confirmDeleteRating(movieId, movieTitle) {
    if (!movieId || !movieTitle) {
        console.error('movieId ou movieTitle não fornecidos');
        return;
    }

    movieIdToDelete = movieId;
    const titleElement = document.getElementById('movieTitle');
    if (titleElement) {
        titleElement.textContent = movieTitle;
    }
    showModal('deleteRatingModal');
}

// Função para deletar avaliação usando fetch 
async function deleteRating(movieId) {
    console.log('deleteRating chamado com movieId:', movieId);

    if (!movieId) {
        console.error('movieId não fornecido para deleteRating');
        return;
    }

    try {
        console.log('Fazendo requisição para:', APP_CONFIG.API_ENDPOINTS.DELETE_RATING);

        const result = await makePostRequest(APP_CONFIG.API_ENDPOINTS.DELETE_RATING, { movieId });

        console.log('Resultado da requisição:', result);
        debugger;
        if (result && result.success) {
            showToast(result.message, 'success');
            setTimeout(() => {
                location.reload();
            }, 1000);
        } else {
            const errorMessage = result?.message || 'Erro ao deletar avaliação';
            console.log('Erro na resposta:', errorMessage);
            showToast(errorMessage, 'error');
        }
    } catch (error) {
        console.error('Erro no deleteRating:', error);
        console.error('Stack trace:', error.stack);
        showToast('Erro ao deletar avaliação. Tente novamente.', 'error');
    }
}

// Inicialização quando o DOM estiver carregado
document.addEventListener('DOMContentLoaded', function () {
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', async function () {

            if (!movieIdToDelete) {
                console.error('movieIdToDelete não definido');
                return;
            }

            hideModal('deleteRatingModal');
            await deleteRating(movieIdToDelete);
            movieIdToDelete = null;
        });
    }
});