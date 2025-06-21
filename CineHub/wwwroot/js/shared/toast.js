// ===== TOAST-SYSTEM.JS =====
// Toast notification system - CineHub

// Toast system namespace
window.ToastSystem = {
    // Cache and state
    cache: {
        container: null,
        toasts: new Map() // Tracks individual toasts
    },

    // Toast-specific configurations (complements APP_CONFIG)
    config: {
        SELECTORS: {
            CONTAINER: '#toast-container',
            TOAST: '.toast',
            CLOSE_BUTTON: '.toast-close',
            PROGRESS: '.toast-progress'
        },
        CLASSES: {
            CONTAINER: 'toast-container',
            TOAST: 'toast',
            CLOSE: 'toast-close',
            CONTENT: 'toast-content',
            ICON: 'toast-icon',
            PROGRESS: 'toast-progress',
            SHOW: 'show',
            HIDE: 'hide'
        },
        TYPES: {
            SUCCESS: 'success',
            WARNING: 'warning',
            ERROR: 'error',
            INFO: 'info'
        },
        TIMING: {
            ANIMATION_DURATION: 400,
            DEFAULT_DURATION: APP_CONFIG?.TIMING?.TOAST_DURATION || 5000
        },
        MAX_TOASTS: 5 // Max number of toasts at once
    },

    // SVG icons for each toast type
    icons: {
        success: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24" aria-hidden="true">
                    <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>
                </svg>`,
        warning: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24" aria-hidden="true">
                    <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z"/>
                </svg>`,
        error: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24" aria-hidden="true">
                    <path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/>
                </svg>`,
        info: `<svg viewBox="0 0 24 24" fill="currentColor" width="24" height="24" aria-hidden="true">
                    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z"/>
                </svg>`
    },

    // Initializes the toast system
    init() {
        try {
            this.ensureContainer();
            this.bindGlobalEvents();
            console.log('ToastSystem: Initialized successfully');
            return true;
        } catch (error) {
            console.error('ToastSystem: Error on init:', error);
            return false;
        }
    },

    // Makes sure the container exists
    ensureContainer() {
        this.cache.container = document.querySelector(this.config.SELECTORS.CONTAINER);

        if (!this.cache.container) {
            console.warn('ToastSystem: Container not found, creating one...');
            this.createContainer();
        }
    },

    // Creates the toast container element
    createContainer() {
        this.cache.container = document.createElement('div');
        this.cache.container.id = 'toast-container';
        this.cache.container.className = this.config.CLASSES.CONTAINER;
        this.cache.container.setAttribute('aria-live', 'polite');
        this.cache.container.setAttribute('aria-label', 'Notifications');
        document.body.appendChild(this.cache.container);
    },

    // Sets up global event listeners
    bindGlobalEvents() {
        document.addEventListener('click', (e) => {
            if (e.target.matches(this.config.SELECTORS.CLOSE_BUTTON)) {
                this.hideToast(e.target);
            }
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.hideAllToasts();
            }
        });
    },

    // Creates and shows a toast
    show(message, type = this.config.TYPES.INFO, duration = this.config.TIMING.DEFAULT_DURATION, options = {}) {
        try {
            if (!message || typeof message !== 'string') {
                console.warn('ToastSystem: Invalid message');
                return null;
            }

            if (!this.isValidType(type)) {
                console.warn(`ToastSystem: Invalid type "${type}", using "info"`);
                type = this.config.TYPES.INFO;
            }

            this.enforceToastLimit();

            const toast = this.createToast(message, type, duration, options);

            this.cache.container.appendChild(toast);
            this.cache.toasts.set(toast, { type, duration, startTime: Date.now() });

            this.animateIn(toast);

            if (duration > 0) {
                this.scheduleAutoHide(toast, duration);
            }

            this.dispatchToastEvent('toastShown', { toast, message, type, duration });

            return toast;
        } catch (error) {
            console.error('ToastSystem: Error showing toast:', error);
            return null;
        }
    },

    // Creates the toast element
    createToast(message, type, duration, options = {}) {
        const toast = document.createElement('div');
        const toastId = `toast-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

        toast.id = toastId;
        toast.className = `${this.config.CLASSES.TOAST} ${type}`;
        toast.setAttribute('data-type', type);
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', type === this.config.TYPES.ERROR ? 'assertive' : 'polite');

        const sanitizedMessage = window.SecurityUtils ?
            window.SecurityUtils.sanitizeText(message) :
            this.escapeHtml(message);

        toast.innerHTML = this.getToastTemplate(sanitizedMessage, type, options);
        this.bindToastEvents(toast, options);

        return toast;
    },

    // Builds the inner template for a toast
    getToastTemplate(message, type, options = {}) {
        const showProgress = options.showProgress !== false;
        const showCloseButton = options.showCloseButton !== false;

        return `
            <div class="${this.config.CLASSES.ICON}">
                ${this.icons[type] || this.icons.info}
            </div>
            <div class="${this.config.CLASSES.CONTENT}">
                ${message}
            </div>
            ${showCloseButton ? `
                <button class="${this.config.CLASSES.CLOSE}" type="button" aria-label="Close notification">
                    &times;
                </button>
            ` : ''}
            ${showProgress ? `<div class="${this.config.CLASSES.PROGRESS}"></div>` : ''}
        `;
    },

    // Sets up toast-specific event listeners
    bindToastEvents(toast, options = {}) {
        if (options.pauseOnHover !== false) {
            toast.addEventListener('mouseenter', () => this.pauseAutoHide(toast));
            toast.addEventListener('mouseleave', () => this.resumeAutoHide(toast));
        }

        if (options.onClick && typeof options.onClick === 'function') {
            toast.addEventListener('click', (e) => {
                if (!e.target.matches(this.config.SELECTORS.CLOSE_BUTTON)) {
                    options.onClick(toast, e);
                }
            });
        }
    },

    // Animates the toast appearance
    animateIn(toast) {
        requestAnimationFrame(() => {
            toast.classList.add(this.config.CLASSES.SHOW);
        });
    },

    // Schedules auto-hide for a toast
    scheduleAutoHide(toast, duration) {
        const timeoutId = setTimeout(() => {
            this.hideToast(toast);
        }, duration);

        const toastData = this.cache.toasts.get(toast);
        if (toastData) {
            toastData.timeoutId = timeoutId;
        }
    },

    // Pauses the auto-hide timer
    pauseAutoHide(toast) {
        const toastData = this.cache.toasts.get(toast);
        if (toastData && toastData.timeoutId) {
            clearTimeout(toastData.timeoutId);
            toastData.pausedAt = Date.now();
        }
    },

    // Resumes the auto-hide timer
    resumeAutoHide(toast) {
        const toastData = this.cache.toasts.get(toast);
        if (toastData && toastData.pausedAt) {
            const elapsed = toastData.pausedAt - toastData.startTime;
            const remaining = Math.max(0, toastData.duration - elapsed);

            if (remaining > 0) {
                toastData.timeoutId = setTimeout(() => {
                    this.hideToast(toast);
                }, remaining);
            }

            delete toastData.pausedAt;
        }
    },

    // Hides a single toast
    hideToast(toastOrButton) {
        const toast = toastOrButton.matches ?
            (toastOrButton.matches(this.config.SELECTORS.TOAST) ? toastOrButton : toastOrButton.closest(this.config.SELECTORS.TOAST)) :
            toastOrButton;

        if (!toast || !toast.parentNode) return;

        const toastData = this.cache.toasts.get(toast);
        if (toastData && toastData.timeoutId) {
            clearTimeout(toastData.timeoutId);
        }

        toast.classList.remove(this.config.CLASSES.SHOW);
        toast.classList.add(this.config.CLASSES.HIDE);

        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
                this.cache.toasts.delete(toast);
                this.dispatchToastEvent('toastHidden', { toast });
            }
        }, this.config.TIMING.ANIMATION_DURATION);
    },

    // Hides all active toasts
    hideAllToasts() {
        Array.from(this.cache.toasts.keys()).forEach((toast) => this.hideToast(toast));
    },

    // Enforces the maximum number of toasts
    enforceToastLimit() {
        const currentToasts = Array.from(this.cache.toasts.keys());
        if (currentToasts.length >= this.config.MAX_TOASTS) {
            this.hideToast(currentToasts[0]);
        }
    },

    // Checks if the given toast type is valid
    isValidType(type) {
        return Object.values(this.config.TYPES).includes(type);
    },

    // Escapes special HTML chars
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    // Dispatches a custom event
    dispatchToastEvent(eventName, detail) {
        const event = new CustomEvent(eventName, { detail, bubbles: true });
        document.dispatchEvent(event);
    },

    // Gets current statistics for toasts
    getStats() {
        return {
            active: this.cache.toasts.size,
            byType: Array.from(this.cache.toasts.values()).reduce((acc, data) => {
                acc[data.type] = (acc[data.type] || 0) + 1;
                return acc;
            }, {})
        };
    }
};

// Compatibility with existing code
window.showToast = function (message, type = 'info', duration = null) {
    if (!window.ToastSystem.cache.container) {
        window.ToastSystem.init();
    }

    const finalDuration = duration !== null ? duration : window.ToastSystem.config.TIMING.DEFAULT_DURATION;
    return window.ToastSystem.show(message, type, finalDuration);
};

window.hideToast = function (toastOrButton) {
    return window.ToastSystem.hideToast(toastOrButton);
};

// Auto-initialize on DOMContentLoaded
document.addEventListener('DOMContentLoaded', function () {
    window.ToastSystem.init();
});
