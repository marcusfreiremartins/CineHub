// Namespace para o sistema de avalia��o
window.RatingSystem = {
    // Cache dos elementos
    cache: {
        badges: null,
        container: null,
        currentRating: 0
    },

    // Configura��es espec�ficas do rating (complementa APP_CONFIG)
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

    /**
     * Inicializa o sistema de avalia��o
     */
    init() {
        try {
            this.cacheElements();

            if (!this.validateElements()) {
                console.warn('RatingSystem: Elementos necess�rios n�o encontrados');
                return false;
            }

            this.getCurrentRating();
            this.bindEvents();
            this.highlightBadges(this.cache.currentRating);

            console.log('RatingSystem: Inicializado com sucesso');
            return true;
        } catch (error) {
            console.error('RatingSystem: Erro na inicializa��o:', error);
            return false;
        }
    },

    /**
     * Armazena refer�ncias dos elementos em cache
     */
    cacheElements() {
        this.cache.container = document.querySelector(this.config.SELECTORS.CONTAINER);
        this.cache.badges = document.querySelectorAll(this.config.SELECTORS.BADGES);
    },

    /**
     * Valida se os elementos necess�rios existem
     */
    validateElements() {
        return this.cache.container && this.cache.badges.length > 0;
    },

    /**
     * Obt�m a avalia��o atual do data attribute
     */
    getCurrentRating() {
        const ratingAttr = this.cache.container.getAttribute(this.config.ATTRIBUTES.CURRENT_RATING);
        this.cache.currentRating = parseInt(ratingAttr) || 0;
    },

    /**
     * Destaca badges baseado na avalia��o fornecida
     * @param {number} rating - N�mero da avalia��o (1-5)
     */
    highlightBadges(rating) {
        if (!this.cache.badges) return;

        this.cache.badges.forEach((badgeLabel, index) => {
            const badge = badgeLabel.querySelector(this.config.SELECTORS.BADGE_ELEMENT);
            if (!badge) return;

            // Remove classes anteriores
            badge.classList.remove(this.config.CLASSES.PRIMARY, this.config.CLASSES.SECONDARY);

            // Adiciona classe baseada na posi��o
            if (index < rating) {
                badge.classList.add(this.config.CLASSES.PRIMARY);
            } else {
                badge.classList.add(this.config.CLASSES.SECONDARY);
            }
        });
    },

    /**
     * Vincula eventos aos elementos
     */
    bindEvents() {
        this.bindBadgeEvents();
        this.bindContainerEvents();
    },

    /**
     * Vincula eventos individuais dos badges
     */
    bindBadgeEvents() {
        this.cache.badges.forEach((badgeLabel, index) => {
            // Evento de hover - destaca badges at� o hovereado
            badgeLabel.addEventListener('mouseenter', () => {
                this.highlightBadges(index + 1);
            });

            // Evento de click - seleciona a avalia��o
            badgeLabel.addEventListener('click', (e) => {
                this.handleBadgeClick(badgeLabel, index);
            });

            // Adiciona suporte a teclado para acessibilidade
            if (window.AccessibilityUtils) {
                window.AccessibilityUtils.addKeyboardNavigation(badgeLabel, () => {
                    this.handleBadgeClick(badgeLabel, index);
                });
            }

            // Adiciona atributos de acessibilidade
            this.enhanceAccessibility(badgeLabel, index);
        });
    },

    /**
     * Vincula eventos do container
     */
    bindContainerEvents() {
        // Restaura estado quando o mouse sai do container
        this.cache.container.addEventListener('mouseleave', () => {
            this.restoreOriginalState();
        });
    },

    /**
     * Manipula o click em um badge
     * @param {Element} badgeLabel - Elemento do badge clicado
     * @param {number} index - �ndice do badge
     */
    handleBadgeClick(badgeLabel, index) {
        const input = badgeLabel.querySelector('input');
        if (input) {
            input.checked = true;
            this.highlightBadges(index + 1);

            // Dispara evento customizado para outras partes da aplica��o
            this.dispatchRatingChangeEvent(index + 1);
        }
    },

    /**
     * Restaura o estado original baseado na sele��o atual
     */
    restoreOriginalState() {
        const checkedInput = document.querySelector(`${this.config.SELECTORS.RATING_INPUT}:checked`);
        const checkedValue = checkedInput ? parseInt(checkedInput.value) : this.cache.currentRating;
        this.highlightBadges(checkedValue);
    },

    /**
     * Melhora a acessibilidade dos badges
     * @param {Element} badgeLabel - Elemento do badge
     * @param {number} index - �ndice do badge
     */
    enhanceAccessibility(badgeLabel, index) {
        const rating = index + 1;
        badgeLabel.setAttribute('role', 'button');
        badgeLabel.setAttribute('tabindex', '0');
        badgeLabel.setAttribute('aria-label', `Avaliar com ${rating} estrela${rating > 1 ? 's' : ''}`);
        badgeLabel.setAttribute('title', `${rating} estrela${rating > 1 ? 's' : ''}`);
    },

    /**
     * Dispara evento customizado quando a avalia��o muda
     * @param {number} rating - Nova avalia��o
     */
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

    /**
     * Define programaticamente uma avalia��o
     * @param {number} rating - Avalia��o a ser definida
     */
    setRating(rating) {
        if (rating < 1 || rating > 5) {
            console.warn('RatingSystem: Avalia��o deve estar entre 1 e 5');
            return false;
        }

        const targetBadge = this.cache.badges[rating - 1];
        if (targetBadge) {
            this.handleBadgeClick(targetBadge, rating - 1);
            return true;
        }

        return false;
    },

    /**
     * Obt�m a avalia��o atual selecionada
     * @returns {number} Avalia��o atual
     */
    getCurrentSelectedRating() {
        const checkedInput = document.querySelector(`${this.config.SELECTORS.RATING_INPUT}:checked`);
        return checkedInput ? parseInt(checkedInput.value) : 0;
    },

    /**
     * Reseta o sistema de avalia��o
     */
    reset() {
        // Limpa sele��es
        const inputs = document.querySelectorAll(this.config.SELECTORS.RATING_INPUT);
        inputs.forEach(input => input.checked = false);

        // Restaura estado original
        this.highlightBadges(this.cache.currentRating);
    },

    /**
     * Destroi o sistema de avalia��o (remove eventos)
     */
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

// Auto-inicializa��o quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    // Inicializa apenas se os elementos existirem
    const ratingContainer = document.querySelector('.rating-container');
    if (ratingContainer) {
        window.RatingSystem.init();
    }
});