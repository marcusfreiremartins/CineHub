// Namespace for the rating system
window.RatingSystem = {
    // Cache of elements
    cache: {
        badges: null,
        container: null,
        currentRating: 0
    },

    // Rating-specific configurations (complements APP_CONFIG)
    config: {
        SELECTORS: {
            BADGES: '.rating-badge',
            CONTAINER: '.rating-container',
            BADGE_ELEMENT: '.badge',
            RATING_INPUT: 'input[name="Rating"]'
        },
        CLASSES: {
            PRIMARY: 'bg-primary',
            SECONDARY: 'bg-secondary'
        },
        ATTRIBUTES: {
            CURRENT_RATING: 'data-current-rating'
        }
    },

    // Initializes the rating system
    init() {
        try {
            this.cacheElements();

            if (!this.validateElements()) {
                console.warn('RatingSystem: Required elements not found');
                return false;
            }

            this.getCurrentRating();
            this.bindEvents();
            this.highlightBadges(this.cache.currentRating);

            console.log('RatingSystem: Initialized successfully');
            return true;
        } catch (error) {
            console.error('RatingSystem: Initialization error:', error);
            return false;
        }
    },

    // Cache element references
    cacheElements() {
        this.cache.container = document.querySelector(this.config.SELECTORS.CONTAINER);
        this.cache.badges = document.querySelectorAll(this.config.SELECTORS.BADGES);
    },

    // Validates required elements existence
    validateElements() {
        return this.cache.container && this.cache.badges.length > 0;
    },

    // Retrieves current rating from data attribute
    getCurrentRating() {
        const ratingAttr = this.cache.container.getAttribute(this.config.ATTRIBUTES.CURRENT_RATING);
        this.cache.currentRating = parseInt(ratingAttr) || 0;
    },

    // Highlights badges based on given rating
    // @param {number} rating - Rating number (1-5)
    highlightBadges(rating) {
        if (!this.cache.badges) return;

        this.cache.badges.forEach((badgeLabel, index) => {
            const badge = badgeLabel.querySelector(this.config.SELECTORS.BADGE_ELEMENT);
            if (!badge) return;

            // Remove previous classes
            badge.classList.remove(this.config.CLASSES.PRIMARY, this.config.CLASSES.SECONDARY);

            // Add class based on position
            if (index < rating) {
                badge.classList.add(this.config.CLASSES.PRIMARY);
            } else {
                badge.classList.add(this.config.CLASSES.SECONDARY);
            }
        });
    },

    // Binds events to elements
    bindEvents() {
        this.bindBadgeEvents();
        this.bindContainerEvents();
    },

    // Binds individual badge events
    bindBadgeEvents() {
        this.cache.badges.forEach((badgeLabel, index) => {
            // Hover event - highlights badges up to hovered one
            badgeLabel.addEventListener('mouseenter', () => {
                this.highlightBadges(index + 1);
            });

            // Click event - selects the rating
            badgeLabel.addEventListener('click', (e) => {
                this.handleBadgeClick(badgeLabel, index);
            });

            // Add keyboard support for accessibility
            if (window.AccessibilityUtils) {
                window.AccessibilityUtils.addKeyboardNavigation(badgeLabel, () => {
                    this.handleBadgeClick(badgeLabel, index);
                });
            }

            // Add accessibility attributes
            this.enhanceAccessibility(badgeLabel, index);
        });
    },

    // Binds container events
    bindContainerEvents() {
        // Restore state when mouse leaves the container
        this.cache.container.addEventListener('mouseleave', () => {
            this.restoreOriginalState();
        });
    },

    // Handles click on a badge
    // @param {Element} badgeLabel - Clicked badge element
    // @param {number} index - Badge index
    handleBadgeClick(badgeLabel, index) {
        const input = badgeLabel.querySelector('input');
        if (input) {
            input.checked = true;
            this.highlightBadges(index + 1);

            // Dispatch custom event for other app parts
            this.dispatchRatingChangeEvent(index + 1);
        }
    },

    // Restores original state based on current selection
    restoreOriginalState() {
        const checkedInput = document.querySelector(`${this.config.SELECTORS.RATING_INPUT}:checked`);
        const checkedValue = checkedInput ? parseInt(checkedInput.value) : this.cache.currentRating;
        this.highlightBadges(checkedValue);
    },

    // Enhances accessibility of badges
    // @param {Element} badgeLabel - Badge element
    // @param {number} index - Badge index
    enhanceAccessibility(badgeLabel, index) {
        const rating = index + 1;
        badgeLabel.setAttribute('role', 'button');
        badgeLabel.setAttribute('tabindex', '0');
        badgeLabel.setAttribute('aria-label', `Avaliar com ${rating} estrela${rating > 1 ? 's' : ''}`);
        badgeLabel.setAttribute('title', `${rating} estrela${rating > 1 ? 's' : ''}`);
    },

    // Dispatches custom event when rating changes
    // @param {number} rating - New rating
    dispatchRatingChangeEvent(rating) {
        const event = new CustomEvent('ratingChanged', {
            detail: {
                rating,
                previousRating: this.cache.currentRating
            },
            bubbles: true
        });

        this.cache.container.dispatchEvent(event);
    },

    // Programmatically sets a rating
    // @param {number} rating - Rating to set
    setRating(rating) {
        if (rating < 1 || rating > 5) {
            console.warn('RatingSystem: Rating must be between 1 and 5');
            return false;
        }

        const targetBadge = this.cache.badges[rating - 1];
        if (targetBadge) {
            this.handleBadgeClick(targetBadge, rating - 1);
            return true;
        }

        return false;
    },

    // Gets the current selected rating
    // @returns {number} Current rating
    getCurrentSelectedRating() {
        const checkedInput = document.querySelector(`${this.config.SELECTORS.RATING_INPUT}:checked`);
        return checkedInput ? parseInt(checkedInput.value) : 0;
    },

    // Resets the rating system
    reset() {
        // Clear selections
        const inputs = document.querySelectorAll(this.config.SELECTORS.RATING_INPUT);
        inputs.forEach(input => input.checked = false);

        // Restore original state
        this.highlightBadges(this.cache.currentRating);
    },

    // Destroys the rating system (removes events)
    destroy() {
        if (this.cache.badges) {
            this.cache.badges.forEach(badge => {
                badge.replaceWith(badge.cloneNode(true));
            });
        }

        if (this.cache.container) {
            this.cache.container.replaceWith(this.cache.container.cloneNode(true));
        }

        this.cache = { badges: null, container: null, currentRating: 0 };
    }
};

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    // Initialize only if elements exist
    const ratingContainer = document.querySelector('.rating-container');
    if (ratingContainer) {
        window.RatingSystem.init();
    }
});