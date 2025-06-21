// Namespace para o sistema de avaliação
window.RatingSystem = {
    // Cache dos elementos
    cache: {
        badges: null,
        container: null,
        currentRating: 0
    },

    // Configurações específicas do rating (complementa APP_CONFIG)
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
     * Inicializa o sistema de avaliação
     */
    init() {
        try {
            this.cacheElements();

            if (!this.validateElements()) {
                console.warn('RatingSystem: Elementos necessários não encontrados');
                return false;
            }

            this.getCurrentRating();
            this.bindEvents();
            this.highlightBadges(this.cache.currentRating);

            console.log('RatingSystem: Inicializado com sucesso');
            return true;
        } catch (error) {
            console.error('RatingSystem: Erro na inicialização:', error);
            return false;
        }
    },

    /**
     * Armazena referências dos elementos em cache
     */
    cacheElements() {
        this.cache.container = document.querySelector(this.config.SELECTORS.CONTAINER);
        this.cache.badges = document.querySelectorAll(this.config.SELECTORS.BADGES);
    },

    /**
     * Valida se os elementos necessários existem
     */
    validateElements() {
        return this.cache.container && this.cache.badges.length > 0;
    },

    /**
     * Obtém a avaliação atual do data attribute
     */
    getCurrentRating() {
        const ratingAttr = this.cache.container.getAttribute(this.config.ATTRIBUTES.CURRENT_RATING);
        this.cache.currentRating = parseInt(ratingAttr) || 0;
    },

    /**
     * Destaca badges baseado na avaliação fornecida
     * @param {number} rating - Número da avaliação (1-5)
     */
    highlightBadges(rating) {
        if (!this.cache.badges) return;

        this.cache.badges.forEach((badgeLabel, index) => {
            const badge = badgeLabel.querySelector(this.config.SELECTORS.BADGE_ELEMENT);
            if (!badge) return;

            // Remove classes anteriores
            badge.classList.remove(this.config.CLASSES.PRIMARY, this.config.CLASSES.SECONDARY);

            // Adiciona classe baseada na posição
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
            // Evento de hover - destaca badges até o hovereado
            badgeLabel.addEventListener('mouseenter', () => {
                this.highlightBadges(index + 1);
            });

            // Evento de click - seleciona a avaliação
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
     * @param {number} index - Índice do badge
     */
    handleBadgeClick(badgeLabel, index) {
        const input = badgeLabel.querySelector('input');
        if (input) {
            input.checked = true;
            this.highlightBadges(index + 1);

            // Dispara evento customizado para outras partes da aplicação
            this.dispatchRatingChangeEvent(index + 1);
        }
    },

    /**
     * Restaura o estado original baseado na seleção atual
     */
    restoreOriginalState() {
        const checkedInput = document.querySelector(`${this.config.SELECTORS.RATING_INPUT}:checked`);
        const checkedValue = checkedInput ? parseInt(checkedInput.value) : this.cache.currentRating;
        this.highlightBadges(checkedValue);
    },

    /**
     * Melhora a acessibilidade dos badges
     * @param {Element} badgeLabel - Elemento do badge
     * @param {number} index - Índice do badge
     */
    enhanceAccessibility(badgeLabel, index) {
        const rating = index + 1;
        badgeLabel.setAttribute('role', 'button');
        badgeLabel.setAttribute('tabindex', '0');
        badgeLabel.setAttribute('aria-label', `Avaliar com ${rating} estrela${rating > 1 ? 's' : ''}`);
        badgeLabel.setAttribute('title', `${rating} estrela${rating > 1 ? 's' : ''}`);
    },

    /**
     * Dispara evento customizado quando a avaliação muda
     * @param {number} rating - Nova avaliação
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
     * Define programaticamente uma avaliação
     * @param {number} rating - Avaliação a ser definida
     */
    setRating(rating) {
        if (rating < 1 || rating > 5) {
            console.warn('RatingSystem: Avaliação deve estar entre 1 e 5');
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
     * Obtém a avaliação atual selecionada
     * @returns {number} Avaliação atual
     */
    getCurrentSelectedRating() {
        const checkedInput = document.querySelector(`${this.config.SELECTORS.RATING_INPUT}:checked`);
        return checkedInput ? parseInt(checkedInput.value) : 0;
    },

    /**
     * Reseta o sistema de avaliação
     */
    reset() {
        // Limpa seleções
        const inputs = document.querySelectorAll(this.config.SELECTORS.RATING_INPUT);
        inputs.forEach(input => input.checked = false);

        // Restaura estado original
        this.highlightBadges(this.cache.currentRating);
    },

    /**
     * Destroi o sistema de avaliação (remove eventos)
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

// Auto-inicialização quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    // Inicializa apenas se os elementos existirem
    const ratingContainer = document.querySelector('.rating-container');
    if (ratingContainer) {
        window.RatingSystem.init();
    }
});