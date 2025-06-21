// site.js - Main site functionality

document.addEventListener('DOMContentLoaded', function () {
    // Check if the toast container exists, create it if not
    if (!document.getElementById('toast-container')) {
        const toastContainer = document.createElement('div');
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

    // Show welcome message if the flag is set
    const shouldShowWelcome = document.body.getAttribute('data-show-welcome');
    if (shouldShowWelcome === 'true') {
        setTimeout(() => {
            showToast('Faça login para acessar todas as funcionalidades do CineHub! 🎬', 'info', 7000);
        }, APP_CONFIG.TIMING.WELCOME_DELAY);
    }

    // Initialize navbar manager
    initializeNavbar();

    // Initialize accessibility features
    initializeAccessibility();
});

// Navbar manager with scroll
function initializeNavbar() {
    const navbar = document.querySelector(APP_CONFIG.SELECTORS.NAVBAR);

    if (!navbar) return;

    let lastScrollTop = 0;
    let isScrolling = false;
    let scrollTimeout;

    const handleScroll = () => {
        if (isScrolling) return;

        isScrolling = true;
        requestAnimationFrame(() => {
            const currentScrollTop = window.scrollY || document.documentElement.scrollTop;

            if (currentScrollTop > lastScrollTop && currentScrollTop > APP_CONFIG.LIMITS.NAVBAR_HIDE_THRESHOLD) {
                navbar.classList.add('navbar-hidden');
            } else if (currentScrollTop < lastScrollTop) {
                navbar.classList.remove('navbar-hidden');
            } else if (currentScrollTop <= APP_CONFIG.LIMITS.NAVBAR_HIDE_THRESHOLD) {
                navbar.classList.remove('navbar-hidden');
            }

            lastScrollTop = currentScrollTop <= 0 ? 0 : currentScrollTop;
            isScrolling = false;
        });
    };

    // Throttle scroll events for better performance
    window.addEventListener('scroll', () => {
        if (scrollTimeout) {
            clearTimeout(scrollTimeout);
        }
        scrollTimeout = setTimeout(handleScroll, APP_CONFIG.TIMING.SCROLL_THROTTLE);
    });
}

// Initialize accessibility features
function initializeAccessibility() {
    // AccessibilityUtils is now defined in utils.js, so we can use it directly
    if (typeof AccessibilityUtils !== 'undefined') {
        AccessibilityUtils.enhanceRatingElements();
        AccessibilityUtils.addTooltips();
    }
}