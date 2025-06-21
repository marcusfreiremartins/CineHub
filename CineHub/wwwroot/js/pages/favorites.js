// Remove a declaração APP_CONFIG daqui, use a do config.js

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

function showToast(message, type) {
    const toast = document.createElement("div")
    toast.className = `toast ${type}`
    toast.textContent = message

    const container = document.querySelector(APP_CONFIG.SELECTORS.TOAST_CONTAINER) || document.body
    container.appendChild(toast)

    setTimeout(() => {
        if (container.contains(toast)) {
            container.removeChild(toast)
        }
    }, APP_CONFIG.TIMING.TOAST_DURATION) // Usando a estrutura do config.js
}

// Toggle favorite status and remove the card without reload
async function toggleFavorite(movieId, button) {
    if (!movieId || !button) {
        console.error("movieId or button not provided")
        return
    }

    try {
        const card = button.closest(".movie-card-favorite") || button.closest(APP_CONFIG.SELECTORS.MOVIE_CARD)

        if (card) {
            button.classList.add("loading")
            button.disabled = true
            card.classList.add("loading")
        }

        // Usando o endpoint correto do config.js
        const result = await makePostRequest(APP_CONFIG.API.ENDPOINTS.TOGGLE_FAVORITE, `movieId=${movieId}`)

        if (result.success) {
            if (card) {
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
            if (card) {
                card.classList.remove("loading")
                button.classList.remove("loading")
                button.disabled = false
            }
            showToast(result.message || APP_CONFIG.ERROR_MESSAGES.GENERIC_ERROR, "error")
        }
    } catch (error) {
        console.error("Error in toggleFavorite:", error)

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

// Enhanced animation for favorite removal
function animateAndRemoveFavorite(element, callback) {
    if (!element) return

    const duration = APP_CONFIG.TIMING.FADE_DURATION + 200 // Usando a estrutura do config.js
    element.classList.add("removing")
    element.style.transition = `all ${duration}ms ease-out`

    setTimeout(() => {
        element.remove()
        if (typeof callback === "function") {
            callback()
        }
    }, duration)
}

// Update favorite movies counter with animation
function updateFavoriteCounter(count) {
    const counter = document.querySelector(APP_CONFIG.SELECTORS.COUNTER)
    if (counter) {
        counter.style.transition = `all ${APP_CONFIG.TIMING.FADE_DURATION}ms ease`
        counter.style.transform = "scale(0.9)"
        counter.style.opacity = "0.7"

        setTimeout(() => {
            counter.querySelector("small").textContent = `${count} filme(s) favorito(s)`
            counter.style.transform = "scale(1)"
            counter.style.opacity = "1"
        }, APP_CONFIG.TIMING.FADE_DURATION / 2)
    }

    const discoverMoreSection = document.getElementById("discoverMoreSection")
    if (discoverMoreSection && count === 0) {
        discoverMoreSection.style.transition = `opacity ${APP_CONFIG.TIMING.FADE_DURATION}ms ease`
        discoverMoreSection.style.opacity = "0"
        setTimeout(() => {
            discoverMoreSection.style.display = "none"
        }, APP_CONFIG.TIMING.FADE_DURATION)
    }
}

// Show empty state with fade
function showEmptyFavoriteState() {
    const container = document.querySelector(".row")
    if (container) {
        container.style.transition = `opacity ${APP_CONFIG.TIMING.FADE_DURATION}ms ease`
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
            setTimeout(() => {
                container.style.opacity = "1"
            }, 50)
        }, APP_CONFIG.TIMING.FADE_DURATION)
    }
}

// Bulk selection
function initializeBulkActions() {
    const selectAllBtn = document.getElementById("selectAllFavorites")
    const removeSelectedBtn = document.getElementById("removeSelectedFavorites")

    if (selectAllBtn && removeSelectedBtn) {
        const selectedCards = new Set()

        selectAllBtn.addEventListener("click", () => {
            const cards = document.querySelectorAll(".movie-card-favorite")
            const isSelectingAll = selectedCards.size !== cards.length

            cards.forEach((card, index) => {
                setTimeout(() => {
                    if (isSelectingAll) {
                        card.classList.add("selected")
                        selectedCards.add(card.dataset.movieId)
                    } else {
                        card.classList.remove("selected")
                        selectedCards.delete(card.dataset.movieId)
                    }
                }, index * APP_CONFIG.TIMING.ITEM_STAGGER_DELAY)
            })

            setTimeout(() => updateBulkActionButtons(selectedCards.size),
                cards.length * APP_CONFIG.TIMING.ITEM_STAGGER_DELAY + 100)
        })

        removeSelectedBtn.addEventListener("click", async () => {
            if (selectedCards.size === 0) return
            const confirmed = confirm(`Remover ${selectedCards.size} filme(s) dos favoritos?`)
            if (!confirmed) return

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
                }, i * APP_CONFIG.TIMING.ITEM_STAGGER_DELAY)
            }

            selectedCards.clear()
            setTimeout(() => updateBulkActionButtons(0),
                movieIds.length * APP_CONFIG.TIMING.ITEM_STAGGER_DELAY + 500)
        })
    }
}

// Update bulk action button labels
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

// Check sessionStorage for toast
function checkAndShowPendingToast() {
    const message = sessionStorage.getItem("toastMessage")
    const type = sessionStorage.getItem("toastType")

    if (message && type) {
        sessionStorage.removeItem("toastMessage")
        sessionStorage.removeItem("toastType")

        setTimeout(() => showToast(message, type), APP_CONFIG.TIMING.SCROLL_DELAY)
    }
}

// Hover effects
function initializeHoverEffects() {
    document.querySelectorAll(".movie-card-favorite").forEach((card) => {
        const overlay = card.querySelector(".movie-hover-overlay")
        const heartBtn = card.querySelector(".movie-heart-remove-btn")

        if (overlay && heartBtn) {
            card.addEventListener("mouseenter", () => {
                if (card.classList.contains("loading")) {
                    overlay.style.display = "none"
                } else {
                    overlay.style.display = "flex"
                }
            })
            card.addEventListener("mouseleave", () => {
                overlay.style.display = "flex"
            })
        }
    })
}

// Initialization
document.addEventListener("DOMContentLoaded", () => {
    checkAndShowPendingToast()
    initializeHoverEffects()
    initializeBulkActions()

    document.addEventListener("keydown", (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key === "a") {
            const selectAllBtn = document.getElementById("selectAllFavorites")
            if (selectAllBtn) {
                e.preventDefault()
                selectAllBtn.click()
            }
        }

        if (e.key === "Delete") {
            const removeSelectedBtn = document.getElementById("removeSelectedFavorites")
            if (removeSelectedBtn && !removeSelectedBtn.disabled) {
                removeSelectedBtn.click()
            }
        }
    })

    window.scrollToTopSmooth = () => {
        window.scrollTo({ top: 0, behavior: "smooth" })
    }

    if (window.AccessibilityUtils) {
        window.AccessibilityUtils.enhanceRatingElements()
        window.AccessibilityUtils.addTooltips()
    }
})

function animateAndRemove(element, callback) {
    if (!element) return

    const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches
    if (prefersReducedMotion) {
        element.remove()
        if (typeof callback === "function") callback()
        return
    }

    animateAndRemoveFavorite(element, callback)
}

// Export public functions
window.toggleFavorite = toggleFavorite
window.updateFavoriteCounter = updateFavoriteCounter
window.showEmptyFavoriteState = showEmptyFavoriteState
window.animateAndRemove = animateAndRemove