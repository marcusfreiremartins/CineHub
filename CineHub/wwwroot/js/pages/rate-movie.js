document.addEventListener('DOMContentLoaded', function () {
    const badges = document.querySelectorAll('.rating-badge');
    const container = document.querySelector('.rating-container');

    if (!container || badges.length === 0) return;

    // Get the current rating value from the data attribute
    const currentRating = parseInt(container.getAttribute('data-current-rating')) || 0;

    // Function to highlight badges based on the given rating
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

    // Highlight the current rating on page load
    highlightBadges(currentRating);

    // Add event listeners to each badge
    badges.forEach((badgeLabel, index) => {
        // Hover effect - highlight badges up to the hovered badge
        badgeLabel.addEventListener('mouseenter', () => {
            highlightBadges(index + 1);
        });

        // Click - select the rating
        badgeLabel.addEventListener('click', () => {
            const input = badgeLabel.querySelector('input');
            if (input) {
                input.checked = true;
                highlightBadges(index + 1);
            }
        });
    });

    // Restore the highlighted state when the mouse leaves the container
    container.addEventListener('mouseleave', () => {
        const checkedInput = document.querySelector('input[name="Rating"]:checked');
        const checkedValue = checkedInput ? parseInt(checkedInput.value) : currentRating;
        highlightBadges(checkedValue);
    });
});