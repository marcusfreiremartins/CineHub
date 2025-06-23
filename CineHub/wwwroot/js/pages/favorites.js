// Function to make POST requests
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

// Function to update favorites counter
function updateFavoriteCounter(count) {
    const counter = document.querySelector('.favorite-counter');
    if (counter) {
        const duration = window.APP_CONFIG?.TIMING?.FADE_DURATION || 300;
        counter.style.transition = `all ${duration}ms ease`;
        counter.style.transform = "scale(0.9)";
        counter.style.opacity = "0.7";

        setTimeout(() => {
            const smallElement = counter.querySelector("small");
            if (smallElement) {
                smallElement.textContent = `${count} favorite movie(s)`;
            }
            counter.style.transform = "scale(1)";
            counter.style.opacity = "1";
        }, duration / 2);
    }

    const discoverMoreSection = document.getElementById("discoverMoreSection");
    if (discoverMoreSection && count === 0) {
        const duration = window.APP_CONFIG?.TIMING?.FADE_DURATION || 300;
        discoverMoreSection.style.transition = `opacity ${duration}ms ease`;
        discoverMoreSection.style.opacity = "0";
        setTimeout(() => {
            discoverMoreSection.style.display = "none";
        }, duration);
    }
}

// Function to show empty favorite state
function showEmptyFavoriteState() {
    const container = document.querySelector(".row");
    if (container) {
        const duration = window.APP_CONFIG?.TIMING?.FADE_DURATION || 300;
        container.style.transition = `opacity ${duration}ms ease`;
        container.style.opacity = "0";

        setTimeout(() => {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center py-5" id="emptyFavoritesState">
                        <div class="display-1 text-muted mb-4">💔</div>
                        <h3 class="text-muted mb-3">You have no more favorites</h3>
                        <p class="text-muted mb-4">All movies have been removed from your favorites. How about exploring more?</p>
                        <div class="d-flex flex-wrap justify-content-center gap-2">
                            <a href="/Movies/Popular" class="btn btn-primary">
                                <i class="fas fa-fire me-1"></i>
                                Popular Movies
                            </a>
                            <a href="/Movies/TopRated" class="btn btn-outline-primary">
                                <i class="fas fa-trophy me-1"></i>
                                Top Rated
                            </a>
                            <a href="/Search" class="btn btn-outline-secondary">
                                <i class="fas fa-search me-1"></i>
                                Search
                            </a>
                        </div>
                    </div>
                </div>
            `;
            setTimeout(() => {
                container.style.opacity = "1";
            }, 50);
        }, duration);
    }
}

// Corrected FLIP animation function
function animateAndRemoveFavorite(cardToRemove) {
    return new Promise((resolve) => {

        // Find the correct container (.row)
        const parentContainer = cardToRemove.closest('.row');
        if (!parentContainer) {
            cardToRemove.remove();
            resolve();
            return;
        }

        // 1. FIRST: Capture initial positions of all cards and their containers
        const allCardContainers = [...parentContainer.querySelectorAll('.col-lg-3, .col-md-4, .col-sm-6')];
        const cardToRemoveContainer = cardToRemove.closest('.col-lg-3, .col-md-4, .col-sm-6');

        if (!cardToRemoveContainer) {
            cardToRemove.remove();
            resolve();
            return;
        }

        const siblingContainers = allCardContainers.filter(container => container !== cardToRemoveContainer);

        // If there's only one container (the one to remove), no need to animate
        if (siblingContainers.length === 0) {
            const animationDuration = window.APP_CONFIG?.TIMING?.ANIMATION_DURATION || 500;
            cardToRemove.classList.add('removing');

            setTimeout(() => {
                cardToRemoveContainer.remove();
                resolve();
            }, animationDuration);
            return;
        }

        // Capture the initial positions of the remaining cards
        const firstPositions = new Map();
        siblingContainers.forEach((container, index) => {
            const card = container.querySelector('.movie-card-favorite');
            if (card) {
                const rect = card.getBoundingClientRect();
                firstPositions.set(container, {
                    left: rect.left,
                    top: rect.top,
                    width: rect.width,
                    height: rect.height
                });
            }
        });

        // 2. Apply removal animation
        const animationDuration = window.APP_CONFIG?.TIMING?.ANIMATION_DURATION || 500;
        cardToRemove.classList.add('removing');

        // 3. Remove the container after animation
        setTimeout(() => {
            cardToRemoveContainer.remove();

            // Force reflow so Bootstrap reorders the layout
            parentContainer.offsetHeight;

            // 4. Wait one frame to capture new positions after reflow
            requestAnimationFrame(() => {

                const lastPositions = new Map();
                siblingContainers.forEach((container, index) => {
                    if (container.parentElement) {
                        const card = container.querySelector('.movie-card-favorite');
                        if (card) {
                            const rect = card.getBoundingClientRect();
                            lastPositions.set(container, {
                                left: rect.left,
                                top: rect.top
                            });
                        }
                    }
                });

                // 5. INVERT & PLAY
                let animatingCards = 0;
                siblingContainers.forEach((container, index) => {
                    if (!container.parentElement) return;

                    const card = container.querySelector('.movie-card-favorite');
                    if (!card) return;

                    const first = firstPositions.get(container);
                    const last = lastPositions.get(container);

                    if (!first || !last) return;

                    const deltaX = first.left - last.left;
                    const deltaY = first.top - last.top;

                    if (Math.abs(deltaX) > 5 || Math.abs(deltaY) > 5) {
                        animatingCards++;

                        card.style.transform = `translate(${deltaX}px, ${deltaY}px)`;
                        card.style.transition = 'none';

                        requestAnimationFrame(() => {
                            card.style.transition = 'transform 0.6s cubic-bezier(0.4, 0, 0.2, 1)';
                            card.style.transform = 'translate(0, 0)';

                            const cleanup = () => {
                                card.style.transform = '';
                                card.style.transition = '';
                                card.removeEventListener('transitionend', cleanup);

                                animatingCards--;

                                if (animatingCards === 0) {
                                    resolve();
                                }
                            };
                            card.addEventListener('transitionend', cleanup, { once: true });
                            setTimeout(cleanup, 800);
                        });
                    }
                });

                if (animatingCards === 0) {
                    resolve();
                }
            });

        }, animationDuration);
    });
}

// Main toggleFavorite function
async function toggleFavorite(movieId, button) {
    console.log('toggleFavorite called with:', { movieId, button });

    if (!movieId || !button) {
        console.error("movieId or button not provided:", { movieId, button });
        return;
    }

    try {
        const card = button.closest(".movie-card-favorite") || button.closest(".movie-card-global");

        if (!card) {
            console.error("Card not found for the button");
            return;
        }

        // Loading state
        if (card) {
            button.classList.add("loading");
            button.disabled = true;
            card.classList.add("loading");
        }

        const endpoint = window.APP_CONFIG?.API?.ENDPOINTS?.TOGGLE_FAVORITE || '/Movies/ToggleFavorite';
        console.log('Making request to:', endpoint);

        const result = await makePostRequest(endpoint, `movieId=${movieId}`);
        console.log('API result:', result);

        if (result.success) {

            // Animate removal with reordering
            await animateAndRemoveFavorite(card);

            // Count remaining cards AFTER removal
            const remainingCards = document.querySelectorAll(".movie-card-favorite").length;

            // Update counter
            updateFavoriteCounter(remainingCards);

            // Show empty state if needed
            if (remainingCards === 0) {
                showEmptyFavoriteState();
            }

        } else {
            console.log('API error:', result.message);

            if (card) {
                card.classList.remove("loading");
                button.classList.remove("loading");
                button.disabled = false;
            }
            const errorMsg = result.message || window.APP_CONFIG?.ERROR_MESSAGES?.GENERIC_ERROR || "Error removing favorite";
            showToast(errorMsg, "error");
        }

    } catch (error) {
        console.error("Error in toggleFavorite:", error);

        // Remove loading in case of error
        const card = button.closest(".movie-card-favorite") || button.closest(".movie-card-global");
        if (card) {
            card.classList.remove("loading");
            const heartBtn = card.querySelector(".movie-heart-remove-btn");
            if (heartBtn) {
                heartBtn.classList.remove("loading");
                heartBtn.disabled = false;
            }
        }

        const networkError = window.APP_CONFIG?.ERROR_MESSAGES?.NETWORK_ERROR || "Connection error";
        showToast(networkError, "error");
    }
}

// Function to check for pending toast
function checkAndShowPendingToast() {
    const message = sessionStorage.getItem("toastMessage");
    const type = sessionStorage.getItem("toastType");

    if (message && type) {
        sessionStorage.removeItem("toastMessage");
        sessionStorage.removeItem("toastType");

        const delay = window.APP_CONFIG?.TIMING?.SCROLL_DELAY || 100;
        setTimeout(() => showToast(message, type), delay);
    }
}

// Function to initialize hover effects
function initializeHoverEffects() {
    document.querySelectorAll(".movie-card-favorite").forEach((card) => {
        const overlay = card.querySelector(".movie-hover-overlay");
        const heartBtn = card.querySelector(".movie-heart-remove-btn");

        if (overlay && heartBtn) {
            card.addEventListener("mouseenter", () => {
                if (card.classList.contains("loading")) {
                    overlay.style.display = "none";
                } else {
                    overlay.style.display = "flex";
                }
            });
            card.addEventListener("mouseleave", () => {
                if (!card.classList.contains("loading")) {
                    overlay.style.display = "none";
                }
            });
        }
    });
}

// Function for bulk actions
function initializeBulkActions() {
    const selectAllBtn = document.getElementById("selectAllFavorites");
    const removeSelectedBtn = document.getElementById("removeSelectedFavorites");

    if (selectAllBtn && removeSelectedBtn) {
        const selectedCards = new Set();

        selectAllBtn.addEventListener("click", () => {
            const cards = document.querySelectorAll(".movie-card-favorite");
            const isSelectingAll = selectedCards.size !== cards.length;

            cards.forEach((card, index) => {
                const delay = (window.APP_CONFIG?.TIMING?.ITEM_STAGGER_DELAY || 50);
                setTimeout(() => {
                    if (isSelectingAll) {
                        card.classList.add("selected");
                        selectedCards.add(card.dataset.movieId);
                    } else {
                        card.classList.remove("selected");
                        selectedCards.delete(card.dataset.movieId);
                    }
                }, index * delay);
            });

            const totalDelay = cards.length * (window.APP_CONFIG?.TIMING?.ITEM_STAGGER_DELAY || 50) + 100;
            setTimeout(() => updateBulkActionButtons(selectedCards.size), totalDelay);
        });

        removeSelectedBtn.addEventListener("click", async () => {
            if (selectedCards.size === 0) return;
            const confirmed = confirm(`Remove ${selectedCards.size} favorite movie(s)?`);
            if (!confirmed) return;

            const movieIds = Array.from(selectedCards);
            const staggerDelay = window.APP_CONFIG?.TIMING?.ITEM_STAGGER_DELAY || 50;

            for (let i = 0; i < movieIds.length; i++) {
                const movieId = movieIds[i];
                setTimeout(async () => {
                    const card = document.querySelector(`[data-movie-id="${movieId}"]`);
                    if (card) {
                        const removeBtn = card.querySelector(".movie-heart-remove-btn");
                        if (removeBtn) {
                            await toggleFavorite(Number.parseInt(movieId), removeBtn);
                        }
                    }
                }, i * staggerDelay);
            }

            selectedCards.clear();
            setTimeout(() => updateBulkActionButtons(0), movieIds.length * staggerDelay + 500);
        });
    }
}

// Function to update bulk action buttons
function updateBulkActionButtons(selectedCount) {
    const removeSelectedBtn = document.getElementById("removeSelectedFavorites");
    const selectAllBtn = document.getElementById("selectAllFavorites");

    if (removeSelectedBtn) {
        removeSelectedBtn.disabled = selectedCount === 0;
        removeSelectedBtn.textContent =
            selectedCount > 0 ? `Remove ${selectedCount} selected` : "Remove selected";
    }

    if (selectAllBtn) {
        const totalCards = document.querySelectorAll(".movie-card-favorite").length;
        selectAllBtn.textContent = selectedCount === totalCards ? "Deselect all" : "Select all";
    }
}

// Helper function for animation
function animateAndRemove(element, callback) {
    if (!element) return;

    const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (prefersReducedMotion) {
        element.remove();
        if (typeof callback === "function") callback();
        return;
    }

    animateAndRemoveFavorite(element).then(() => {
        if (typeof callback === "function") callback();
    });
}

// INITIALIZATION
document.addEventListener("DOMContentLoaded", () => {

    // Ensure APP_CONFIG is available
    if (typeof window.APP_CONFIG === 'undefined') {
        console.warn('APP_CONFIG not found, using defaults');
        window.APP_CONFIG = {};
    }

    checkAndShowPendingToast();
    initializeHoverEffects();
    initializeBulkActions();

    // Keyboard shortcuts
    document.addEventListener("keydown", (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key === "a") {
            const selectAllBtn = document.getElementById("selectAllFavorites");
            if (selectAllBtn) {
                e.preventDefault();
                selectAllBtn.click();
            }
        }

        if (e.key === "Delete") {
            const removeSelectedBtn = document.getElementById("removeSelectedFavorites");
            if (removeSelectedBtn && !removeSelectedBtn.disabled) {
                removeSelectedBtn.click();
            }
        }
    });

    // Smooth scroll function
    window.scrollToTopSmooth = () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    // Accessibility enhancements
    if (window.AccessibilityUtils) {
        window.AccessibilityUtils.enhanceRatingElements();
        window.AccessibilityUtils.addTooltips();
    }
});

// Export functions for global access
window.toggleFavorite = toggleFavorite;
window.updateFavoriteCounter = updateFavoriteCounter;
window.showEmptyFavoriteState = showEmptyFavoriteState;
window.animateAndRemove = animateAndRemove;