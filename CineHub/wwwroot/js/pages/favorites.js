const APP_CONFIG = {
    API: {
        HEADERS: {},
        API_ENDPOINTS: {
            TOGGLE_FAVORITE: "/api/toggle-favorite",
        },
    },
    SELECTORS: {
        TOAST_CONTAINER: ".toast-container",
        MOVIE_CARD: ".movie-card",
        COUNTER: ".favorite-counter",
    },
    TOAST_DURATION: 3000,
    ANIMATION: {
        FADE_DURATION: 500,
        ITEM_STAGGER_DELAY: 100,
        SCROLL_DELAY: 200,
    },
    ERROR_MESSAGES: {
        GENERIC_ERROR: "Ocorreu um erro.",
        NETWORK_ERROR: "Erro de rede.",
    },
}

async function makePostRequest(url, data) {
    const response = await fetch(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded",
            ...APP_CONFIG.API.HEADERS,
        },
        body: data,
    })
    return response.json()
}

function showToast(message, type) {
    const toast = document.createElement("div")
    toast.className = `toast ${type}`
    toast.textContent = message

    // Use toast container from config if available
    const container = document.querySelector(APP_CONFIG.SELECTORS.TOAST_CONTAINER) || document.body
    container.appendChild(toast)

    setTimeout(() => {
        if (container.contains(toast)) {
            container.removeChild(toast)
        }
    }, APP_CONFIG.TOAST_DURATION)
}

// Function to toggle favorite and remove from UI without reload
async function toggleFavorite(movieId, button) {
    if (!movieId || !button) {
        console.error("movieId ou button não fornecidos")
        return
    }

    try {
        // Find the movie card
        const card = button.closest(".movie-card-favorite") || button.closest(APP_CONFIG.SELECTORS.MOVIE_CARD)

        if (card) {
            // Add loading state to heart button
            button.classList.add("loading")
            button.disabled = true

            // Add loading state to card
            card.classList.add("loading")
        }

        const result = await makePostRequest(APP_CONFIG.API_ENDPOINTS.TOGGLE_FAVORITE, `movieId=${movieId}`)

        if (result.success) {
            if (card) {
                // Enhanced animation for removal using config timing
                animateAndRemoveFavorite(card, () => {
                    const remainingCards = document.querySelectorAll(".movie-card-favorite").length

                    updateFavoriteCounter(remainingCards)

                    if (remainingCards === 0) {
                        showEmptyFavoriteState()
                    }

                    showToast(result.message || "💔 Filme removido dos favoritos!", "success")
                })
            } else {
                showToast(result.message || "💔 Filme removido dos favoritos!", "success")
            }
        } else {
            // Remove loading state on error
            if (card) {
                card.classList.remove("loading")
                button.classList.remove("loading")
                button.disabled = false
            }
            showToast(result.message || APP_CONFIG.ERROR_MESSAGES.GENERIC_ERROR, "error")
        }
    } catch (error) {
        console.error("Erro no toggleFavorite:", error)

        // Remove loading state on error
        const card = button.closest(".movie-card-favorite") || button.closest(APP_CONFIG.SELECTORS.MOVIE_CARD)
        if (card) {
            card.classList.remove("loading")
            const heartBtn = card.querySelector(".movie-heart-remove-btn")
            if (heartBtn) {
                heartBtn.classList.remove("loading")
                heartBtn.disabled = false
            }
        }

        showToast(APP_CONFIG.ERROR_MESSAGES.NETWORK_ERROR, "error")
    }
}

// Enhanced animation function for favorite removal using config timing
function animateAndRemoveFavorite(element, callback) {
    if (!element) return

    // Use animation duration from config
    const duration = APP_CONFIG.ANIMATION.FADE_DURATION + 200 // Slightly longer for removal

    // Add removing class for animation
    element.classList.add("removing")
    element.style.transition = `all ${duration}ms ease-out`

    // Wait for animation to complete
    setTimeout(() => {
        element.remove()
        if (typeof callback === "function") {
            callback()
        }
    }, duration)
}

// Update the favorite movies counter with animation using config
function updateFavoriteCounter(count) {
    const counter = document.querySelector(APP_CONFIG.SELECTORS.COUNTER)
    if (counter) {
        // Animate counter update using config timing
        counter.style.transition = `all ${APP_CONFIG.ANIMATION.FADE_DURATION}ms ease`
        counter.style.transform = "scale(0.9)"
        counter.style.opacity = "0.7"

        setTimeout(() => {
            counter.querySelector("small").textContent = `${count} filme(s) favorito(s)`
            counter.style.transform = "scale(1)"
            counter.style.opacity = "1"
        }, APP_CONFIG.ANIMATION.FADE_DURATION / 2)
    }

    const discoverMoreSection = document.getElementById("discoverMoreSection")
    if (discoverMoreSection) {
        if (count === 0) {
            // Fade out discover more section using config timing
            discoverMoreSection.style.transition = `opacity ${APP_CONFIG.ANIMATION.FADE_DURATION}ms ease`
            discoverMoreSection.style.opacity = "0"
            setTimeout(() => {
                discoverMoreSection.style.display = "none"
            }, APP_CONFIG.ANIMATION.FADE_DURATION)
        }
    }
}

// Enhanced empty state with better styling using config timing
function showEmptyFavoriteState() {
    const container = document.querySelector(".row")
    if (container) {
        // Fade out current content using config timing
        container.style.transition = `opacity ${APP_CONFIG.ANIMATION.FADE_DURATION}ms ease`
        container.style.opacity = "0"

        setTimeout(() => {
            container.innerHTML = `
                <div class="col-12">
                    <div class="text-center py-5" id="emptyFavoritesState">
                        <div class="display-1 text-muted mb-4">💔</div>
                        <h3 class="text-muted mb-3">Você não tem mais favoritos</h3>
                        <p class="text-muted mb-4">Todos os filmes foram removidos dos seus favoritos. Que tal explorar mais?</p>
                        <div class="d-flex flex-wrap justify-content-center gap-2">
                            <a href="/Movies/Popular" class="btn btn-primary">
                                <i class="fas fa-fire me-1"></i>
                                Filmes Populares
                            </a>
                            <a href="/Movies/TopRated" class="btn btn-outline-primary">
                                <i class="fas fa-trophy me-1"></i>
                                Melhores Avaliados
                            </a>
                            <a href="/Search" class="btn btn-outline-secondary">
                                <i class="fas fa-search me-1"></i>
                                Pesquisar
                            </a>
                        </div>
                    </div>
                </div>
            `

            // Fade in new content using config timing
            setTimeout(() => {
                container.style.opacity = "1"
            }, 50)
        }, APP_CONFIG.ANIMATION.FADE_DURATION)
    }
}

// Bulk actions for favorites using config
function initializeBulkActions() {
    const selectAllBtn = document.getElementById("selectAllFavorites")
    const removeSelectedBtn = document.getElementById("removeSelectedFavorites")

    if (selectAllBtn && removeSelectedBtn) {
        const selectedCards = new Set()

        selectAllBtn.addEventListener("click", () => {
            const cards = document.querySelectorAll(".movie-card-favorite")
            const isSelectingAll = selectedCards.size !== cards.length

            cards.forEach((card, index) => {
                // Use stagger delay from config
                setTimeout(() => {
                    if (isSelectingAll) {
                        card.classList.add("selected")
                        selectedCards.add(card.dataset.movieId)
                    } else {
                        card.classList.remove("selected")
                        selectedCards.delete(card.dataset.movieId)
                    }
                }, index * APP_CONFIG.ANIMATION.ITEM_STAGGER_DELAY)
            })

            // Update buttons after all animations
            setTimeout(
                () => {
                    updateBulkActionButtons(selectedCards.size)
                },
                cards.length * APP_CONFIG.ANIMATION.ITEM_STAGGER_DELAY + 100,
            )
        })

        removeSelectedBtn.addEventListener("click", async () => {
            if (selectedCards.size === 0) return

            const confirmed = confirm(`Remover ${selectedCards.size} filme(s) dos favoritos?`)
            if (!confirmed) return

            // Remove selected favorites with stagger
            const movieIds = Array.from(selectedCards)
            for (let i = 0; i < movieIds.length; i++) {
                const movieId = movieIds[i]
                setTimeout(async () => {
                    const card = document.querySelector(`[data-movie-id="${movieId}"]`)
                    if (card) {
                        const removeBtn = card.querySelector(".movie-remove-favorite-btn")
                        if (removeBtn) {
                            await toggleFavorite(Number.parseInt(movieId), removeBtn)
                        }
                    }
                }, i * APP_CONFIG.ANIMATION.ITEM_STAGGER_DELAY)
            }

            selectedCards.clear()
            setTimeout(
                () => {
                    updateBulkActionButtons(0)
                },
                movieIds.length * APP_CONFIG.ANIMATION.ITEM_STAGGER_DELAY + 500,
            )
        })
    }
}

function updateBulkActionButtons(selectedCount) {
    const removeSelectedBtn = document.getElementById("removeSelectedFavorites")
    const selectAllBtn = document.getElementById("selectAllFavorites")

    if (removeSelectedBtn) {
        removeSelectedBtn.disabled = selectedCount === 0
        removeSelectedBtn.textContent =
            selectedCount > 0 ? `Remover ${selectedCount} selecionado(s)` : "Remover selecionados"
    }

    if (selectAllBtn) {
        const totalCards = document.querySelectorAll(".movie-card-favorite").length
        selectAllBtn.textContent = selectedCount === totalCards ? "Desmarcar todos" : "Selecionar todos"
    }
}

// Checks for a pending toast in sessionStorage after reload
function checkAndShowPendingToast() {
    const message = sessionStorage.getItem("toastMessage")
    const type = sessionStorage.getItem("toastType")

    if (message && type) {
        sessionStorage.removeItem("toastMessage")
        sessionStorage.removeItem("toastType")

        setTimeout(() => {
            showToast(message, type)
        }, APP_CONFIG.ANIMATION.SCROLL_DELAY)
    }
}

// Enhanced hover experience for favorite cards
function initializeHoverEffects() {
    document.querySelectorAll(".movie-card-favorite").forEach((card) => {
        const overlay = card.querySelector(".movie-hover-overlay")
        const heartBtn = card.querySelector(".movie-heart-remove-btn")

        if (overlay && heartBtn) {
            // Prevent overlay from showing during loading
            card.addEventListener("mouseenter", () => {
                if (card.classList.contains("loading")) {
                    overlay.style.display = "none"
                } else {
                    overlay.style.display = "flex"
                }
            })

            card.addEventListener("mouseleave", () => {
                overlay.style.display = "flex" // Reset to flex for CSS to handle
            })
        }
    })
}

// Enhanced initialization using config
document.addEventListener("DOMContentLoaded", () => {
    checkAndShowPendingToast()
    initializeHoverEffects()
    initializeBulkActions()

    // Add keyboard shortcuts
    document.addEventListener("keydown", (e) => {
        // Ctrl/Cmd + A to select all favorites
        if ((e.ctrlKey || e.metaKey) && e.key === "a") {
            const selectAllBtn = document.getElementById("selectAllFavorites")
            if (selectAllBtn) {
                e.preventDefault()
                selectAllBtn.click()
            }
        }

        // Delete key to remove selected favorites
        if (e.key === "Delete") {
            const removeSelectedBtn = document.getElementById("removeSelectedFavorites")
            if (removeSelectedBtn && !removeSelectedBtn.disabled) {
                removeSelectedBtn.click()
            }
        }
    })

    // Add smooth scroll to top after removing favorites using config timing
    window.scrollToTopSmooth = () => {
        window.scrollTo({
            top: 0,
            behavior: "smooth",
        })
    }

    // Initialize accessibility enhancements from config
    if (window.AccessibilityUtils) {
        window.AccessibilityUtils.enhanceRatingElements()
        window.AccessibilityUtils.addTooltips()
    }
})

// Animation function that respects reduced motion preferences
function animateAndRemove(element, callback) {
    if (!element) return

    // Check for reduced motion preference
    const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches

    if (prefersReducedMotion) {
        // Skip animation if user prefers reduced motion
        element.remove()
        if (typeof callback === "function") {
            callback()
        }
        return
    }

    // Use the enhanced animation for favorites
    animateAndRemoveFavorite(element, callback)
}

// Export functions for global use (maintaining compatibility)
window.toggleFavorite = toggleFavorite
window.updateFavoriteCounter = updateFavoriteCounter
window.showEmptyFavoriteState = showEmptyFavoriteState
window.animateAndRemove = animateAndRemove