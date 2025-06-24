/**
 * Controls pagination and content loading via AJAX in a modular way.
 */
class AjaxPaginator {
    /**
     * @param {string} containerSelector CSS selector for the main container (e.g., '#movie-list-section').
     * @param {string} cardAnimationClass CSS class to animate new items (e.g., '.content-card-animation').
     */
    constructor(containerSelector, cardAnimationClass) {
        this.contentSection = document.querySelector(containerSelector);
        this.cardAnimationClass = cardAnimationClass;

        if (!this.contentSection) {
            console.warn(`AjaxPaginator: Container not found for selector: ${containerSelector}`);
            return;
        }

        this.containerId = this.contentSection.id;
        if (!this.containerId) {
            console.error("AjaxPaginator: The pagination container must have a unique ID.");
            return;
        }

        // Ensures 'this' inside event handlers refers to this class instance.
        this.loadPage = this.loadPage.bind(this);
        this._handlePaginationClick = this._handlePaginationClick.bind(this);
        this._handlePopState = this._handlePopState.bind(this);
    }

    /**
     * Loads content from a URL into the container via AJAX.
     * This method is public and can be called from other scripts.
     * @param {string} url The URL to load.
     */
    async loadPage(url) {
        const loadingOverlay = this.contentSection.querySelector('.loading-overlay');

        if (loadingOverlay) loadingOverlay.classList.add('visible');
        this.contentSection.style.minHeight = this.contentSection.offsetHeight + 'px';

        try {
            const response = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!response.ok) throw new Error(`Network error: ${response.statusText}`);

            const html = await response.text();
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const newContent = doc.getElementById(this.containerId);

            if (newContent) {
                this.contentSection.innerHTML = newContent.innerHTML;
                window.history.pushState({ path: url }, '', url);

                const newCards = this.contentSection.querySelectorAll(this.cardAnimationClass);
                newCards.forEach((card, index) => {
                    card.style.animationDelay = `${index * 50}ms`;
                });
            } else {
                throw new Error(`Content section with ID "${this.containerId}" not found in response.`);
            }

        } catch (error) {
            console.error('Failed to load page via AJAX:', error);
            window.location.href = url; // Fallback to full page load
        } finally {
            if (loadingOverlay) loadingOverlay.classList.remove('visible');
            this.contentSection.style.minHeight = '';
            this.contentSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }

    /**
     * Handles click events on pagination links.
     * Prevents default navigation and loads the page via AJAX instead.
     * @param {MouseEvent} event The click event.
     * @private
     */
    _handlePaginationClick(event) {
        const paginationLink = event.target.closest('.pagination .page-link');
        if (paginationLink && !paginationLink.closest('.page-item.disabled') && !paginationLink.closest('.page-item.active')) {
            event.preventDefault();
            const url = paginationLink.getAttribute('href');
            if (url) {
                this.loadPage(url);
            }
        }
    }

    /**
     * Handles browser history popstate events.
     * Loads the corresponding page content via AJAX.
     * @param {PopStateEvent} event The popstate event.
     * @private
     */
    _handlePopState(event) {
        if (event.state && event.state.path) {
            this.loadPage(event.state.path);
        }
    }

    /**
     * Initializes the AjaxPaginator:
     * - Attaches click listener to pagination links.
     * - Attaches popstate listener for back/forward navigation.
     * - Sets initial history state if missing.
     */
    initialize() {
        if (!this.contentSection) return;

        this.contentSection.addEventListener('click', this._handlePaginationClick);
        window.addEventListener('popstate', this._handlePopState);

        if (!window.history.state || window.history.state.path !== window.location.href) {
            window.history.replaceState({ path: window.location.href }, '', window.location.href);
        }
    }
}
