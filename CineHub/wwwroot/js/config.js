// Configuration object for app settings, API endpoints, and selectors
const APP_CONFIG = {

    // Duration for toast notifications (in milliseconds)
    TOAST_DURATION: 5000,

    // Default duration for animations (in milliseconds)
    ANIMATION_DURATION: 300,

    // API endpoints used throughout the application
    API_ENDPOINTS: {
        TOGGLE_FAVORITE: '/Rating/ToggleFavorite',
        DELETE_RATING: '/Rating/DeleteRating',
        MOVIE_DETAILS: '/Movies/Details' // Endpoint for fetching movie details
    },

    // CSS selectors for DOM manipulation
    SELECTORS: {
        MOVIE_CARD: '.col-lg-3, .col-md-4, .col-sm-6',
        COUNTER: '.favorite-counter',
        TOAST_CONTAINER: '#toast-container',

        // Selectors related to the movie details page
        COMMENTS_CONTAINER: '#comments-container',
        PAGINATION_CONTAINER: '.pagination',
        PAGINATION_LINKS: '.pagination .page-link',
        COMMENTS_HEADER: '.card-header h4',
        COMMENT_ITEM: '.comment-item',
        COMMENT_TEXT: '.comment-text',
        RATING_STARS: '.rating-stars',
        BADGES: '.badge'
    },

    // Timing and delay settings for animations
    ANIMATION: {
        FADE_DURATION: 300,
        FADE_IN_DURATION: 400,
        ITEM_STAGGER_DELAY: 80,
        SCROLL_DELAY: 200
    },

    // Limits and thresholds used in the UI
    LIMITS: {
        COMMENT_PREVIEW_LENGTH: 300
    },

    // Default headers for API requests
    API: {
        HEADERS: {
            'X-Requested-With': 'XMLHttpRequest',
            'Content-Type': 'application/json',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
        }
    },

    // Default error messages used throughout the app
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

    // List of dangerous HTML elements to sanitize
    DANGEROUS_ELEMENTS: ['script', 'iframe', 'object', 'embed', 'form'],

    // List of dangerous HTML attributes to sanitize
    DANGEROUS_ATTRIBUTES: ['onclick', 'onload', 'onerror', 'onmouseover', 'onfocus', 'onblur'],

    // CSS classes to remove during content sanitization
    CLASSES_TO_REMOVE: ['initial-load']
};