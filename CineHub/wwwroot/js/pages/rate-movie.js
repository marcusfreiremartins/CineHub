document.addEventListener('DOMContentLoaded', function () {
    const badges = document.querySelectorAll('.rating-badge');
    const container = document.querySelector('.rating-container');

    if (!container || badges.length === 0) return;

    // Pega o valor do rating atual do atributo data
    const currentRating = parseInt(container.getAttribute('data-current-rating')) || 0;

    // Função para destacar os badges baseado na nota
    function highlightBadges(rating) {
        badges.forEach((badgeLabel, index) => {
            const badge = badgeLabel.querySelector('.badge');
            if (!badge) return;

            if (index < rating) {
                badge.classList.remove('bg-secondary');
                badge.classList.add('bg-primary');
            } else {
                badge.classList.remove('bg-primary');
                badge.classList.add('bg-secondary');
            }
        });
    }

    // Destacar a nota atual ao carregar a página
    highlightBadges(currentRating);

    // Adicionar event listeners para cada badge
    badges.forEach((badgeLabel, index) => {
        // Efeito hover - destaca badges até a posição do mouse
        badgeLabel.addEventListener('mouseenter', () => {
            highlightBadges(index + 1);
        });

        // Click - seleciona a nota
        badgeLabel.addEventListener('click', () => {
            const input = badgeLabel.querySelector('input');
            if (input) {
                input.checked = true;
                highlightBadges(index + 1);
            }
        });
    });

    // Restaurar estado ao sair do container
    container.addEventListener('mouseleave', () => {
        const checkedInput = document.querySelector('input[name="Rating"]:checked');
        const checkedValue = checkedInput ? parseInt(checkedInput.value) : currentRating;
        highlightBadges(checkedValue);
    });
});