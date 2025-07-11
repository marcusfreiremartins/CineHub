// Configuration object for app settings, API endpoints, and selectors
const APP_CONFIG = {
    // Durations and timing
    TIMING: {
        TOAST_DURATION: 5000,
        ANIMATION_DURATION: 300,
        FADE_DURATION: 300,
        FADE_IN_DURATION: 400,
        ITEM_STAGGER_DELAY: 80,
        SCROLL_DELAY: 200,
        WELCOME_DELAY: 500,
        SCROLL_THROTTLE: 10
    },
    // API endpoints
    API: {
        ENDPOINTS: {
            TOGGLE_FAVORITE: '/Rating/ToggleFavorite',
            DELETE_RATING: '/Rating/DeleteRating',
            MOVIE_DETAILS: '/Movies/Details'
        },
        HEADERS: {
            'X-Requested-With': 'XMLHttpRequest',
            'Content-Type': 'application/json',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,/;q=0.8'
        }
    },
    // API endpoints (legacy compatibility)
    API_ENDPOINTS: {
        MOVIE_DETAILS: '/Movies/Details'
    },
    // CSS selectors
    SELECTORS: {
        // Containers and structure
        NAVBAR: '.navbar',
        TOAST_CONTAINER: '#toast-container',
        COMMENTS_CONTAINER: '#comments-container',
        PAGINATION_CONTAINER: '.pagination',
        // Movie elements
        MOVIE_CARD: '.col-lg-3, .col-md-4, .col-sm-6',
        MOVIE_CARD_GLOBAL: '.movie-card-global',
        MOVIE_POSTER_GLOBAL: '.movie-poster-global',
        MOVIE_POSTER_PLACEHOLDER: '.movie-poster-placeholder-global',
        MOVIE_BTN_GLOBAL: '.movie-btn-global',
        // Interaction elements
        PAGINATION_LINKS: '.pagination .page-link',
        RATING_STARS: '.rating-stars',
        BADGES: '.badge',
        COUNTER: '.favorite-counter',
        // Comment elements
        COMMENTS_HEADER: '.card-header h4',
        COMMENT_ITEM: '.comment-item',
        COMMENT_TEXT: '.comment-text'
    },
    // Limits and settings
    LIMITS: {
        COMMENT_PREVIEW_LENGTH: 300,
        NAVBAR_HIDE_THRESHOLD: 100
    },
    // Error messages
    ERROR_MESSAGES: {
        NETWORK_ERROR: 'Erro de conexão. Verifique sua internet.',
        NOT_FOUND: 'Página não encontrada',
        SERVER_ERROR: 'Erro interno do servidor',
        GENERIC_ERROR: 'Erro ao carregar comentários',
        CONTAINER_NOT_FOUND: 'Container de comentários não encontrado',
        MOVIE_ID_NOT_FOUND: 'ID do filme não encontrado na URL',
        INITIALIZATION_ERROR: 'Erro ao inicializar a página',
        EMPTY_RESPONSE: 'Resposta vazia do servidor',
        PARSE_ERROR: 'Erro ao processar resposta do servidor',
        COMMENTS_SECTION_NOT_FOUND: 'Seção de comentários não encontrada na resposta'
    },
    // Security settings
    SECURITY: {
        DANGEROUS_ELEMENTS: ['script', 'iframe', 'object', 'embed', 'form'],
        DANGEROUS_ATTRIBUTES: ['onclick', 'onload', 'onerror', 'onmouseover', 'onfocus', 'onblur'],
        CLASSES_TO_REMOVE: ['initial-load']
    },
    // Animation settings
    ANIMATION: {
        INTERSECTION_THRESHOLD: 0.1,
        INTERSECTION_ROOT_MARGIN: '50px'
    }
};
// Make this config object available globally
window.APP_CONFIG = APP_CONFIG;