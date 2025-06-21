class MovieCardManager {
    constructor(options = {}) {
        // Initialize configuration options
        this.options = {
            enableAnimations: true,
            enableLazyLoading: true,
            animationDelay: APP_CONFIG.TIMING.ITEM_STAGGER_DELAY,
            ...options
        };
        this.init();
    }

    // Initialize all features
    init() {
        this.setupLazyLoading();
        this.setupCardAnimations();
        this.setupKeyboardNavigation();
        this.setupImageErrorHandling();
    }

    // Set up lazy-loading for images
    setupLazyLoading() {
        if (!this.options.enableLazyLoading) return;

        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const src = img.dataset.src;

                    if (src) {
                        img.src = src;
                        img.removeAttribute('data-src');
                        img.classList.remove('lazy-loading');
                        observer.unobserve(img);
                    }
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach((img) => {
            img.classList.add('lazy-loading');
            imageObserver.observe(img);
        });
    }

    // Set up animated appearance for cards
    setupCardAnimations() {
        if (!this.options.enableAnimations) return;

        const animationObserver = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry, index) => {
                    if (entry.isIntersecting) {
                        setTimeout(() => {
                            entry.target.classList.add('movie-card-animate-in');
                        }, index * this.options.animationDelay);

                        animationObserver.unobserve(entry.target);
                    }
                });
            },
            {
                threshold: APP_CONFIG.ANIMATION.INTERSECTION_THRESHOLD,
                rootMargin: APP_CONFIG.ANIMATION.INTERSECTION_ROOT_MARGIN
            }
        );

        document.querySelectorAll(APP_CONFIG.SELECTORS.MOVIE_CARD_GLOBAL).forEach((card) => {
            animationObserver.observe(card);
        });
    }

    // Enable keyboard accessibility and navigation
    setupKeyboardNavigation() {
        document.addEventListener('keydown', (e) => {
            const card = e.target.closest(APP_CONFIG.SELECTORS.MOVIE_CARD_GLOBAL);
            if (!card) return;

            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                const link = card.querySelector(APP_CONFIG.SELECTORS.MOVIE_BTN_GLOBAL);
                if (link) {
                    link.click();
                }
            }
        });

        document.querySelectorAll(APP_CONFIG.SELECTORS.MOVIE_CARD_GLOBAL).forEach((card) => {
            if (!card.hasAttribute('tabindex')) {
                card.setAttribute('tabindex', '0');
            }
        });
    }

    // Handle image errors by showing a placeholder
    setupImageErrorHandling() {
        document.addEventListener(
            'error',
            (e) => {
                if (e.target.tagName === 'IMG' && e.target.classList.contains('movie-poster-global')) {
                    const placeholder = e.target.parentNode.querySelector('.movie-poster-placeholder-global');
                    if (placeholder) {
                        e.target.style.display = 'none';
                        placeholder.style.display = 'flex';
                    }
                }
            },
            true
        );
    }

    // Add new movie cards to the container
    addCards(cardsHtml, container) {
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = cardsHtml;

        const newCards = tempDiv.querySelectorAll(APP_CONFIG.SELECTORS.MOVIE_CARD_GLOBAL);
        newCards.forEach((card, index) => {
            container.appendChild(card);

            if (this.options.enableAnimations) {
                setTimeout(() => {
                    card.classList.add('movie-card-animate-in');
                }, index * this.options.animationDelay);
            }
        });

        this.setupLazyLoading();
        this.setupKeyboardNavigation();
    }

    // Filter displayed cards based on a predicate
    filterCards(filterFn) {
        document.querySelectorAll(APP_CONFIG.SELECTORS.MOVIE_CARD_GLOBAL).forEach((card) => {
            const movieData = this.getMovieDataFromCard(card);
            const shouldShow = filterFn(movieData);
            card.style.display = shouldShow ? 'block' : 'none';
        });
    }

    // Get data attributes from a movie card
    getMovieDataFromCard(card) {
        return {
            title: card.querySelector('.movie-title-global')?.textContent || '',
            year: card.querySelector('.movie-year-global')?.textContent || '',
            rating: card.querySelector('.movie-rating-global')?.textContent || ''
        };
    }
}

// Utilities for movie cards
window.MovieCardUtils = {
    // Shorten long text
    truncateText(text, maxLength) {
        if (!text || text.length <= maxLength) return text;
        return text.substring(0, maxLength).trim() + '...';
    },

    // Format numeric rating to one decimal place
    formatRating(rating) {
        return Number.parseFloat(rating).toFixed(1);
    },

    // Get year from full date string
    formatYear(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.getFullYear();
    },

    // Generate the full HTML for a movie card
    generateMovieCardHtml(movie, options = {}) {
        const {
            cardSize = 'standard',
            showRanking = false,
            imageBaseUrl = '',
            ranking = null
        } = options;

        const posterUrl = movie.PosterPath ? `${imageBaseUrl}${movie.PosterPath}` : null;
        const year = movie.ReleaseDate ? this.formatYear(movie.ReleaseDate) : '';
        const rating = movie.VoteAverage > 0 ? this.formatRating(movie.VoteAverage) : '';

        return `
            <div class="movie-card-global movie-card-${cardSize}" data-movie-id="${movie.Id}">
                <div class="movie-poster-container">
                    ${posterUrl
                ? `<img src="${posterUrl}" alt="${movie.Title}" class="movie-poster-global" loading="lazy">`
                : `<div class="movie-poster-placeholder-global">🎬</div>`
            }
                    ${showRanking && ranking ? `<div class="movie-ranking-badge">#${ranking}</div>` : ''}
                </div>
                
                <div class="movie-content-global">
                    <h3 class="movie-title-global">${movie.Title}</h3>
                    
                    <div class="movie-meta-global">
                        ${year ? `<span class="movie-year-global">📅 ${year}</span>` : ''}
                        ${rating ? `<span class="movie-rating-global">⭐ ${rating}</span>` : ''}
                    </div>
                    
                    <a href="${APP_CONFIG.API.ENDPOINTS.MOVIE_DETAILS}/${movie.Id}" class="movie-btn-global">
                        View Details
                    </a>
                </div>
            </div>
        `;
    }
};

// Automatic initialization of movie cards
document.addEventListener('DOMContentLoaded', () => {
    if (document.querySelector(APP_CONFIG.SELECTORS.MOVIE_CARD_GLOBAL)) {
        window.movieCardManager = new MovieCardManager({
            enableAnimations: true,
            enableLazyLoading: true,
            animationDelay: APP_CONFIG.TIMING.ITEM_STAGGER_DELAY
        });
    }
});

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { MovieCardManager, MovieCardUtils: window.MovieCardUtils };
}