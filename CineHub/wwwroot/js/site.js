// Global application initializations on DOM ready
document.addEventListener('DOMContentLoaded', function () {
    // Check if toast container exists, create if not
    if (!document.getElementById('toast-container')) {
        const toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container';
        document.body.appendChild(toastContainer);
    }

    // Show welcome message if flag is set
    const shouldShowWelcome = document.body.getAttribute('data-show-welcome');
    if (shouldShowWelcome === 'true') {
        setTimeout(() => {
            showToast('Faça login para acessar todas as funcionalidades do CineHub! 🎬', 'info', 7000);
        }, 500);
    }

    // Navbar scroll animation
    const navbar = document.querySelector('.navbar');

    if (navbar) {
        let lastScrollTop = 0;
        let isScrolling = false;

        const handleScroll = () => {
            if (isScrolling) return;

            isScrolling = true;
            requestAnimationFrame(() => {
                const currentScrollTop = window.scrollY || document.documentElement.scrollTop;

                if (currentScrollTop > lastScrollTop && currentScrollTop > 100) {
                    navbar.classList.add('navbar-hidden');
                }
                else if (currentScrollTop < lastScrollTop) {
                    navbar.classList.remove('navbar-hidden');
                }
                else if (currentScrollTop <= 100) {
                    navbar.classList.remove('navbar-hidden');
                }

                lastScrollTop = currentScrollTop <= 0 ? 0 : currentScrollTop;
                isScrolling = false;
            });
        };

        // Throttle scroll events for better performance
        let scrollTimeout;
        window.addEventListener('scroll', () => {
            if (scrollTimeout) {
                clearTimeout(scrollTimeout);
            }
            scrollTimeout = setTimeout(handleScroll, 10);
        });
    }
});

// Security utilities
window.SecurityUtils = {
    // Sanitize plain text to prevent XSS
    sanitizeText(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    // Sanitize complex HTML content
    sanitizeHtmlContent(content, options = {}) {
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = content;

        const dangerousElements = options.dangerousElements || APP_CONFIG.DANGEROUS_ELEMENTS;
        dangerousElements.forEach(selector => {
            tempDiv.querySelectorAll(selector).forEach(el => el.remove());
        });

        tempDiv.querySelectorAll('*').forEach(element => {
            [...element.attributes].forEach(attr => {
                const isDangerous = attr.name.startsWith('on') ||
                    attr.name === 'javascript:' ||
                    APP_CONFIG.DANGEROUS_ATTRIBUTES.includes(attr.name);

                if (isDangerous) {
                    element.removeAttribute(attr.name);
                }
            });

            const classesToRemove = options.classesToRemove || APP_CONFIG.CLASSES_TO_REMOVE;
            classesToRemove.forEach(className => {
                element.classList.remove(className);
            });
        });

        return tempDiv.innerHTML;
    }
};

// Animation utilities
window.AnimationUtils = {
    fadeElement(element, direction, options = {}) {
        return new Promise(resolve => {
            if (!element) {
                resolve();
                return;
            }

            const duration = options.duration || APP_CONFIG.ANIMATION.FADE_DURATION;
            const inDuration = options.inDuration || APP_CONFIG.ANIMATION.FADE_IN_DURATION;

            if (direction === 'out') {
                element.style.cssText = `transition: opacity ${duration}ms ease-out, transform ${duration}ms ease-out; opacity: 0; transform: translateY(-10px);`;
                setTimeout(resolve, duration);
            } else if (direction === 'in') {
                element.style.cssText = 'opacity: 0; transform: translateY(10px);';
                element.offsetHeight;

                element.style.cssText = `transition: opacity ${inDuration}ms ease-out, transform ${inDuration}ms ease-out; opacity: 1; transform: translateY(0);`;

                setTimeout(() => {
                    element.style.cssText = '';
                    resolve();
                }, inDuration);
            }
        });
    },

    staggerElements(elements, options = {}) {
        const delay = options.delay || APP_CONFIG.ANIMATION.ITEM_STAGGER_DELAY;
        const duration = options.duration || APP_CONFIG.ANIMATION.FADE_IN_DURATION;

        elements.forEach((element, index) => {
            setTimeout(() => {
                element.style.cssText = `transition: opacity ${duration}ms ease-out, transform ${duration}ms ease-out; opacity: 1; transform: translateY(0);`;

                setTimeout(() => {
                    element.style.cssText = '';
                }, duration);
            }, index * delay);
        });
    }
};

// UI templates
window.UITemplates = {
    getLoadingTemplate(message = 'Carregando...') {
        return `
            <div class="text-center py-4 loading-state" role="status" aria-live="polite">
                <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
                    <span class="visually-hidden">${SecurityUtils.sanitizeText(message)}</span>
                </div>
                <p class="mt-3 text-muted">${SecurityUtils.sanitizeText(message)}</p>
            </div>
        `;
    },

    getErrorTemplate(message, options = {}) {
        const sanitizedMessage = SecurityUtils.sanitizeText(message);
        const showRetry = options.showRetry !== false;
        const retryCallback = options.retryCallback || 'location.reload()';
        const customRetryCallback = options.customRetryCallback;

        let buttons = `
            <button class="btn btn-outline-primary me-2" onclick="location.reload()" aria-label="Recarregar página">
                🔄 Recarregar Página
            </button>
        `;

        if (showRetry && customRetryCallback) {
            buttons += `
                <button class="btn btn-outline-secondary" onclick="${customRetryCallback}" aria-label="Tentar novamente">
                    🔄 Tentar Novamente
                </button>
            `;
        }

        return `
            <div class="text-center py-4 error-state" role="alert" aria-live="assertive">
                <div class="mb-3">
                    <span class="fs-1 text-danger error-icon" aria-hidden="true">⚠️</span>
                </div>
                <h5 class="text-danger error-title">${sanitizedMessage}</h5>
                <p class="text-muted error-message">Por favor, recarregue a página ou tente novamente mais tarde.</p>
                <div class="mt-3">
                    ${buttons}
                </div>
            </div>
        `;
    }
};

// URL utilities
window.UrlUtils = {
    getUrlParameter(name) {
        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    },

    getCurrentPage() {
        return parseInt(this.getUrlParameter('page')) || 1;
    },

    extractIdFromUrl() {
        const pathParts = window.location.pathname.split('/');
        const id = pathParts[pathParts.length - 1];
        return id && id.trim() !== '' ? id : null;
    },

    updateUrl(params) {
        const url = new URL(window.location);
        Object.entries(params).forEach(([key, value]) => {
            if (value) {
                url.searchParams.set(key, value);
            } else {
                url.searchParams.delete(key);
            }
        });

        if (window.history?.pushState) {
            window.history.pushState({ ...params }, '', url.toString());
        }
    }
};

// Accessibility utilities
window.AccessibilityUtils = {
    enhanceRatingElements(selector = APP_CONFIG.SELECTORS.RATING_STARS) {
        const ratingElements = document.querySelectorAll(selector);
        ratingElements.forEach(element => {
            element.setAttribute('role', 'img');
            const title = element.getAttribute('title');
            if (title) {
                element.setAttribute('aria-label', `Avaliação: ${title}`);
            }
        });
    },

    addTooltips(selector = APP_CONFIG.SELECTORS.BADGES) {
        const elements = document.querySelectorAll(selector);
        elements.forEach(element => {
            element.setAttribute('data-bs-toggle', 'tooltip');
            element.setAttribute('data-bs-placement', 'top');
        });

        if (window.bootstrap?.Tooltip) {
            try {
                const tooltipElements = document.querySelectorAll('[data-bs-toggle="tooltip"]');
                [...tooltipElements].map(el => new window.bootstrap.Tooltip(el));
            } catch (error) {
                console.warn('Falha ao inicializar tooltips:', error);
            }
        }
    },

    addKeyboardNavigation(element, callback) {
        element.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                callback(e);
            }
        });
    },

    addHoverEffects(element, effects = {}) {
        const {
            enterTransform = 'translateY(-1px)',
            leaveTransform = 'translateY(0)'
        } = effects;

        if (!element.classList.contains('disabled')) {
            element.addEventListener('mouseenter', () => {
                element.style.transform = enterTransform;
            });

            element.addEventListener('mouseleave', () => {
                element.style.transform = leaveTransform;
            });
        }
    }
};

// HTTP request utilities
window.HttpUtils = {
    async fetchWithDefaults(url, options = {}) {
        const defaultOptions = {
            method: 'GET',
            headers: { ...APP_CONFIG.API.HEADERS },
            credentials: 'same-origin',
            ...options
        };

        const response = await fetch(url, defaultOptions);

        if (!response.ok) {
            throw new Error(`HTTP Error! Status: ${response.status} - ${response.statusText}`);
        }

        return response;
    },

    parseHtmlResponse(html) {
        if (!html || html.trim().length === 0) {
            throw new Error(APP_CONFIG.ERROR_MESSAGES.EMPTY_RESPONSE);
        }

        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');

        if (!doc || doc.querySelector('parsererror')) {
            throw new Error(APP_CONFIG.ERROR_MESSAGES.PARSE_ERROR);
        }

        return doc;
    },

    getErrorMessage(error) {
        if (error.name === 'TypeError' && error.message.includes('fetch')) {
            return APP_CONFIG.ERROR_MESSAGES.NETWORK_ERROR;
        } else if (error.message.includes('404')) {
            return APP_CONFIG.ERROR_MESSAGES.NOT_FOUND;
        } else if (error.message.includes('500')) {
            return APP_CONFIG.ERROR_MESSAGES.SERVER_ERROR;
        } else if (error.message === APP_CONFIG.ERROR_MESSAGES.EMPTY_RESPONSE) {
            return error.message;
        } else if (error.message === APP_CONFIG.ERROR_MESSAGES.PARSE_ERROR) {
            return error.message;
        }
        return APP_CONFIG.ERROR_MESSAGES.GENERIC_ERROR;
    }
};

// Scroll utilities
window.ScrollUtils = {
    scrollToElement(selector, options = {}) {
        const element = typeof selector === 'string' ? document.querySelector(selector) : selector;

        if (element) {
            const delay = options.delay || APP_CONFIG.ANIMATION.SCROLL_DELAY;
            const behavior = options.behavior || 'smooth';
            const block = options.block || 'start';
            const inline = options.inline || 'nearest';

            setTimeout(() => {
                element.scrollIntoView({
                    behavior,
                    block,
                    inline
                });
            }, delay);
        }
    }
};