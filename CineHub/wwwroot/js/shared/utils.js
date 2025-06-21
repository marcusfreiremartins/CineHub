// Unified HTTP POST requests
async function makePostRequest(url, data) {
    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: typeof data === 'string' ? data : new URLSearchParams(data)
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Request error:', error);
        throw error;
    }
}

// Function to animate and remove elements
function animateAndRemove(element, callback = null) {
    if (!element) return;

    element.style.transition = `opacity ${APP_CONFIG.TIMING.ANIMATION_DURATION}ms ease`;
    element.style.opacity = '0';

    setTimeout(() => {
        if (element.parentNode) {
            element.remove();
        }
        if (callback) callback();
    }, APP_CONFIG.TIMING.ANIMATION_DURATION);
}

// Functions to show/hide Bootstrap modals
function showModal(modalId) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) {
        console.error(`Modal ${modalId} not found`);
        return null;
    }

    const modal = new bootstrap.Modal(modalElement);
    modal.show();
    return modal;
}

function hideModal(modalId) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) return;

    const modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) modal.hide();
}

// Toast function
function showToast(message, type = 'info', duration = APP_CONFIG.TIMING.TOAST_DURATION) {
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container';
        toastContainer.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            max-width: 350px;
        `;
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.style.cssText = `
        background: ${type === 'success' ? '#d4edda' : type === 'error' ? '#f8d7da' : '#d1ecf1'};
        border: 1px solid ${type === 'success' ? '#c3e6cb' : type === 'error' ? '#f5c6cb' : '#bee5eb'};
        color: ${type === 'success' ? '#155724' : type === 'error' ? '#721c24' : '#0c5460'};
        padding: 12px 16px;
        margin-bottom: 10px;
        border-radius: 4px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        opacity: 0;
        transform: translateX(100%);
        transition: all 0.3s ease;
    `;
    toast.textContent = message;

    toastContainer.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '1';
        toast.style.transform = 'translateX(0)';
    }, 10);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, 300);
    }, duration);
}

// HTTP utilities
const HttpUtils = {
    async fetchWithDefaults(url, options = {}) {
        const defaultOptions = {
            headers: APP_CONFIG.API.HEADERS,
            ...options
        };

        const response = await fetch(url, defaultOptions);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response;
    },

    parseHtmlResponse(html) {
        const parser = new DOMParser();
        return parser.parseFromString(html, 'text/html');
    },

    getErrorMessage(error) {
        if (error.message.includes('404')) {
            return APP_CONFIG.ERROR_MESSAGES.NOT_FOUND;
        } else if (error.message.includes('500')) {
            return APP_CONFIG.ERROR_MESSAGES.SERVER_ERROR;
        } else if (error.message.includes('Failed to fetch')) {
            return APP_CONFIG.ERROR_MESSAGES.NETWORK_ERROR;
        }
        return APP_CONFIG.ERROR_MESSAGES.GENERIC_ERROR;
    }
};

// URL utilities
const UrlUtils = {
    extractIdFromUrl() {
        const pathParts = window.location.pathname.split('/');
        const movieIndex = pathParts.indexOf('Movies');

        if (movieIndex !== -1 && pathParts[movieIndex + 1] === 'Details' && pathParts[movieIndex + 2]) {
            return parseInt(pathParts[movieIndex + 2]);
        }

        return null;
    },

    getCurrentPage() {
        const urlParams = new URLSearchParams(window.location.search);
        return parseInt(urlParams.get('page')) || 1;
    },

    updateUrl(params) {
        const url = new URL(window.location);

        Object.keys(params).forEach(key => {
            if (params[key] !== null && params[key] !== undefined) {
                url.searchParams.set(key, params[key]);
            } else {
                url.searchParams.delete(key);
            }
        });

        if (window.history && window.history.pushState) {
            window.history.pushState({ page: params.page || 1 }, '', url.toString());
        }
    }
};

// Animation utilities
const AnimationUtils = {
    async fadeElement(element, direction = 'in') {
        if (!element) return;

        const duration = APP_CONFIG.TIMING.FADE_DURATION;

        if (direction === 'out') {
            element.style.transition = `opacity ${duration}ms ease`;
            element.style.opacity = '0';

            return new Promise(resolve => {
                setTimeout(resolve, duration);
            });
        } else {
            element.style.opacity = '0';
            element.style.transition = `opacity ${duration}ms ease`;

            setTimeout(() => {
                element.style.opacity = '1';
            }, 10);

            return new Promise(resolve => {
                setTimeout(resolve, duration);
            });
        }
    },

    staggerElements(elements) {
        elements.forEach((element, index) => {
            setTimeout(() => {
                element.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
                element.style.opacity = '1';
                element.style.transform = 'translateY(0)';
            }, index * APP_CONFIG.TIMING.ITEM_STAGGER_DELAY);
        });
    }
};

// Scroll utilities
const ScrollUtils = {
    scrollToElement(selector, offset = 0) {
        const element = document.querySelector(selector);
        if (element) {
            const elementPosition = element.offsetTop - offset;
            window.scrollTo({
                top: elementPosition,
                behavior: 'smooth'
            });
        }
    }
};

// Security utilities
const SecurityUtils = {
    sanitizeHtmlContent(html) {
        const temp = document.createElement('div');
        temp.innerHTML = html;

        // Remove dangerous elements
        APP_CONFIG.SECURITY.DANGEROUS_ELEMENTS.forEach(tag => {
            const elements = temp.querySelectorAll(tag);
            elements.forEach(el => el.remove());
        });

        // Remove dangerous attributes
        const allElements = temp.querySelectorAll('*');
        allElements.forEach(el => {
            APP_CONFIG.SECURITY.DANGEROUS_ATTRIBUTES.forEach(attr => {
                if (el.hasAttribute(attr)) {
                    el.removeAttribute(attr);
                }
            });
        });

        return temp.innerHTML;
    },

    sanitizeText(text) {
        const temp = document.createElement('div');
        temp.textContent = text;
        return temp.innerHTML;
    }
};

// UI Templates
const UITemplates = {
    getLoadingTemplate(message = 'Carregando...') {
        return `
            <div class="d-flex justify-content-center align-items-center p-4">
                <div class="spinner-border text-primary me-3" role="status">
                    <span class="visually-hidden">Carregando...</span>
                </div>
                <span class="text-muted">${message}</span>
            </div>
        `;
    },

    getErrorTemplate(message, options = {}) {
        const retryCallback = options.customRetryCallback || 'location.reload()';

        return `
            <div class="alert alert-danger text-center p-4" role="alert">
                <div class="mb-3">
                    <i class="fas fa-exclamation-triangle fa-2x text-danger mb-2"></i>
                    <h5 class="alert-heading">Ops! Algo deu errado</h5>
                </div>
                <p class="mb-3">${message}</p>
                <button type="button" class="btn btn-outline-danger" onclick="${retryCallback}">
                    <i class="fas fa-redo me-1"></i>
                    Tentar novamente
                </button>
            </div>
        `;
    }
};

// Enhanced accessibility utilities
const AccessibilityUtils = {
    enhanceRatingElements: function () {
        const ratingElements = document.querySelectorAll('.rating-stars');
        ratingElements.forEach(element => {
            if (!element.getAttribute('aria-label')) {
                element.setAttribute('aria-label', 'Avaliação do filme');
            }
        });
    },

    addTooltips: function () {
        const tooltipElements = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltipElements.forEach(element => {
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                new bootstrap.Tooltip(element);
            }
        });
    },

    addHoverEffects: function (element) {
        if (!element) return;

        element.addEventListener('mouseenter', function () {
            this.style.transition = 'all 0.2s ease';
            this.style.transform = 'translateY(-1px)';
        });

        element.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    },

    addKeyboardNavigation: function (element, callback) {
        if (!element || !callback) return;

        element.setAttribute('tabindex', '0');
        element.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                callback();
            }
        });
    }
};