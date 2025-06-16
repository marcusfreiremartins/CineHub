// Main class to manage movie details
class MovieDetailsManager {
    constructor() {
        this.isLoading = false;
        this.currentMovieId = null;
        this.init();
    }

    init() {
        try {
            this.currentMovieId = UrlUtils.extractIdFromUrl();
            if (!this.currentMovieId) {
                throw new Error(APP_CONFIG.ERROR_MESSAGES.MOVIE_ID_NOT_FOUND);
            }

            this.setupHistoryState();
            this.attachEventListeners();
            this.initializeFeatures();

            console.log('MovieDetailsManager initialized successfully');
        } catch (error) {
            console.error('Initialization error:', error);
            this.showErrorState(APP_CONFIG.ERROR_MESSAGES.INITIALIZATION_ERROR);
        }
    }

    // Setup initial history state
    setupHistoryState() {
        const currentPage = UrlUtils.getCurrentPage();
        if (window.history?.replaceState) {
            window.history.replaceState({ page: currentPage }, '', window.location.href);
        }
    }

    // Attach all necessary event listeners
    attachEventListeners() {
        this.attachPaginationListeners();
        window.addEventListener('popstate', this.handlePopState.bind(this));
    }

    // Load comments for a specific page
    async loadComments(page) {
        if (this.isLoading || !this.validatePage(page)) return;

        this.isLoading = true;
        const container = document.querySelector(APP_CONFIG.SELECTORS.COMMENTS_CONTAINER);

        if (!container) {
            console.error(APP_CONFIG.ERROR_MESSAGES.CONTAINER_NOT_FOUND);
            this.showErrorState(APP_CONFIG.ERROR_MESSAGES.CONTAINER_NOT_FOUND);
            return;
        }

        const hasContent = container.children.length > 0 && !container.querySelector('.spinner-border');

        try {
            if (hasContent) {
                await AnimationUtils.fadeElement(container, 'out');
            }

            this.showLoadingState();
            const html = await this.fetchCommentsPage(page);
            await this.updateCommentsContent(html, container);

            this.updatePagination(html);
            UrlUtils.updateUrl({ page });
            ScrollUtils.scrollToElement(APP_CONFIG.SELECTORS.COMMENTS_HEADER);

            setTimeout(() => this.initializeCommentFeatures(), 500);
        } catch (error) {
            console.error('Error loading comments:', error);
            this.showErrorState(HttpUtils.getErrorMessage(error));
            AnimationUtils.fadeElement(container, 'in');
        } finally {
            this.isLoading = false;
        }
    }

    // Validate page number
    validatePage(page) {
        if (!page || page < 1) {
            console.error('Invalid page number:', page);
            return false;
        }
        return true;
    }

    // Request to fetch comments
    async fetchCommentsPage(page) {
        const url = `${APP_CONFIG.API_ENDPOINTS.MOVIE_DETAILS}/${this.currentMovieId}?page=${page}`;
        const response = await HttpUtils.fetchWithDefaults(url);
        return await response.text();
    }

    // Update the comments content
    async updateCommentsContent(html, container) {
        const doc = HttpUtils.parseHtmlResponse(html);
        const newContainer = doc.querySelector(APP_CONFIG.SELECTORS.COMMENTS_CONTAINER);

        if (!newContainer) {
            throw new Error(APP_CONFIG.ERROR_MESSAGES.COMMENTS_SECTION_NOT_FOUND);
        }

        const sanitizedContent = SecurityUtils.sanitizeHtmlContent(newContainer.innerHTML);
        container.innerHTML = sanitizedContent;

        await this.animateNewComments(container);
    }

    // Animate new comments
    async animateNewComments(container) {
        const commentItems = container.querySelectorAll(APP_CONFIG.SELECTORS.COMMENT_ITEM);

        commentItems.forEach(item => {
            item.style.cssText = 'opacity: 0; transform: translateY(15px);';
        });

        await AnimationUtils.fadeElement(container, 'in');
        AnimationUtils.staggerElements(commentItems);
    }

    // Update pagination
    updatePagination(html) {
        const paginationContainer = document.querySelector(APP_CONFIG.SELECTORS.PAGINATION_CONTAINER);
        if (!paginationContainer) return;

        const doc = HttpUtils.parseHtmlResponse(html);
        const newPagination = doc.querySelector(APP_CONFIG.SELECTORS.PAGINATION_CONTAINER);

        if (newPagination) {
            const paginationParent = paginationContainer.parentNode;
            const newPaginationParent = newPagination.parentNode;

            if (paginationParent && newPaginationParent) {
                paginationParent.innerHTML = newPaginationParent.innerHTML;
                this.attachPaginationListeners();
            }
        }
    }

    // Attach pagination link listeners
    attachPaginationListeners() {
        const links = document.querySelectorAll(APP_CONFIG.SELECTORS.PAGINATION_LINKS);

        links.forEach(link => {
            const newLink = this.cloneElementWithoutListeners(link);
            newLink.addEventListener('click', this.handlePaginationClick.bind(this));
            this.addAccessibilityFeatures(newLink);
        });
    }

    // Clone element without previous listeners
    cloneElementWithoutListeners(element) {
        const newElement = element.cloneNode(true);
        element.parentNode.replaceChild(newElement, element);
        return newElement;
    }

    // Handle pagination click
    handlePaginationClick(event) {
        event.preventDefault();

        if (this.isLoading) {
            console.log('Loading in progress, ignoring click');
            return;
        }

        const page = this.extractPageFromElement(event.target);
        if (this.validatePage(page)) {
            this.loadComments(page);
        }
    }

    // Extract page number from element
    extractPageFromElement(element) {
        const onclickAttr = element.getAttribute('onclick');

        if (onclickAttr) {
            const match = onclickAttr.match(/loadComments\((\d+)\)/);
            if (match) return parseInt(match[1]);
        }

        const href = element.getAttribute('href');
        if (href?.includes('page=')) {
            const urlParams = new URLSearchParams(href.split('?')[1]);
            return parseInt(urlParams.get('page')) || 1;
        }

        return 1;
    }

    // Add accessibility enhancements
    addAccessibilityFeatures(element) {
        if (element.classList.contains('disabled')) return;

        AccessibilityUtils.addHoverEffects(element);
        AccessibilityUtils.addKeyboardNavigation(element, () => element.click());
    }

    // Handle browser history navigation
    handlePopState(event) {
        const page = event.state?.page || UrlUtils.getCurrentPage();
        if (page > 1) {
            this.loadComments(page);
        }
    }

    // Initialize comment features
    initializeCommentFeatures() {
        this.initializeExpandableComments();
    }

    // Initialize expandable comment logic
    initializeExpandableComments() {
        const commentTexts = document.querySelectorAll(APP_CONFIG.SELECTORS.COMMENT_TEXT);

        commentTexts.forEach(textElement => {
            const textContent = textElement.textContent || textElement.innerText;

            if (textContent.length > APP_CONFIG.LIMITS.COMMENT_PREVIEW_LENGTH) {
                new ExpandableComment(textElement, textContent);
            }
        });
    }

    // Initialize other features
    initializeFeatures() {
        AccessibilityUtils.enhanceRatingElements();
        AccessibilityUtils.addTooltips();
        this.initializeCommentFeatures();
    }

    // Show loading state
    showLoadingState() {
        const container = document.querySelector(APP_CONFIG.SELECTORS.COMMENTS_CONTAINER);
        if (container) {
            container.innerHTML = UITemplates.getLoadingTemplate('Carregando comentários...');
        }
    }

    // Show error state
    showErrorState(message = APP_CONFIG.ERROR_MESSAGES.GENERIC_ERROR) {
        const container = document.querySelector(APP_CONFIG.SELECTORS.COMMENTS_CONTAINER);
        if (container) {
            container.innerHTML = UITemplates.getErrorTemplate(message, {
                customRetryCallback: 'movieDetailsManager.retryLoadComments()'
            });
        }
    }

    // Retry loading comments
    retryLoadComments() {
        const currentPage = UrlUtils.getCurrentPage();
        this.loadComments(currentPage);
    }
}

// Class to handle expandable comment logic
class ExpandableComment {
    constructor(element, fullText) {
        this.element = element;
        this.fullText = element.innerHTML;
        this.shortText = fullText.substring(0, APP_CONFIG.LIMITS.COMMENT_PREVIEW_LENGTH) + '...';
        this.isExpanded = false;

        this.init();
    }

    init() {
        this.element.innerHTML = SecurityUtils.sanitizeText(this.shortText);
        this.createToggleButton();
    }

    createToggleButton() {
        this.button = document.createElement('button');
        this.button.className = 'btn btn-link btn-sm p-0 ms-1';
        this.button.style.fontSize = '0.85em';
        this.button.innerHTML = 'Ver mais';
        this.button.setAttribute('aria-label', 'Expandir comentário completo');

        this.button.addEventListener('click', this.toggle.bind(this));
        this.element.appendChild(this.button);
    }

    toggle(event) {
        event.preventDefault();
        if (!this.isExpanded) {
            this.expand();
        } else {
            this.collapse();
        }
    }

    expand() {
        this.element.innerHTML = this.fullText;
        this.button.innerHTML = 'Ver menos';
        this.button.setAttribute('aria-label', 'Recolher comentário');
        this.element.appendChild(this.button);
        this.isExpanded = true;
    }

    collapse() {
        this.element.innerHTML = SecurityUtils.sanitizeText(this.shortText);
        this.button.innerHTML = 'Ver mais';
        this.button.setAttribute('aria-label', 'Expandir comentário completo');
        this.element.appendChild(this.button);
        this.isExpanded = false;
    }
}

// Initialization
let movieDetailsManager;

const initializeMovieDetails = () => {
    movieDetailsManager = new MovieDetailsManager();
};

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeMovieDetails);
} else {
    initializeMovieDetails();
}

// Export to HTML
window.loadComments = (page) => movieDetailsManager?.loadComments(page);
window.retryLoadComments = () => movieDetailsManager?.retryLoadComments();