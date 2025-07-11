// Simplified Movie Details Manager
class MovieDetailsManager {
    constructor() {
        this.swiperInstances = []
        this.observers = new Map()
        this.eventListeners = []
        this.isInitialized = false
        this.init()
    }

    async init() {
        try {
            console.log("Initializing MovieDetailsManager...")

            // Initialize core features
            await this.initializeTheme()
            await this.initializeCarousels()
            await this.initializeScrollAnimations()
            await this.initializeParallax()
            await this.initializeComments()
            await this.initializeStaffSection()

            this.isInitialized = true
            console.log("MovieDetailsManager initialized successfully")

        } catch (error) {
            console.error("Initialization error:", error)
        }
    }

    // Theme initialization using ColorThief
    async initializeTheme() {
        const poster = document.querySelector("#moviePosterImage")
        if (!poster || typeof ColorThief === "undefined") return

        const applyTheme = (img) => {
            try {
                const colorThief = new ColorThief()
                const palette = colorThief.getPalette(img, 3)
                const [primary, secondary, accent] = palette

                // Apply CSS custom properties
                const root = document.documentElement
                root.style.setProperty("--theme-primary", `rgb(${primary.join(",")})`)
                root.style.setProperty("--theme-secondary", secondary ? `rgb(${secondary.join(",")})` : `rgb(${primary.join(",")})`)
                root.style.setProperty("--theme-accent", accent ? `rgb(${accent.join(",")})` : `rgb(${primary.join(",")})`)
                root.style.setProperty("--theme-glow", `rgba(${primary.join(",")}, 0.3)`)
                root.style.setProperty("--theme-transition", "all 800ms ease-in-out")

            } catch (error) {
                console.error("ColorThief error:", error)
            }
        }

        if (poster.complete) {
            applyTheme(poster)
        } else {
            poster.addEventListener("load", () => applyTheme(poster))
        }
    }

    // Initialize Swiper carousels
    async initializeCarousels() {
        if (typeof Swiper === "undefined") return

        // Cast carousel
        const castContainer = document.querySelector(".cast-swiper")
        if (castContainer) {
            const castSwiper = new Swiper(castContainer, {
                slidesPerView: "auto",
                spaceBetween: 20,
                grabCursor: true,
                lazy: { loadPrevNext: true },
                slidesPerGroup: 4,
                navigation: {
                    nextEl: ".cast-nav-next",
                    prevEl: ".cast-nav-prev",
                },
                breakpoints: {
                    320: { slidesPerView: 2, spaceBetween: 15, slidesPerGroup: 2 },
                    480: { slidesPerView: 3, spaceBetween: 15, slidesPerGroup: 3 },
                    768: { slidesPerView: 4, spaceBetween: 20, slidesPerGroup: 4 },
                    992: { slidesPerView: 5, spaceBetween: 20, slidesPerGroup: 4 },
                    1200: { slidesPerView: 6, spaceBetween: 20, slidesPerGroup: 4 },
                },
                on: {
                    init: function () { updateNavButtons(this) },
                    slideChange: function () { updateNavButtons(this) }
                }
            })
            this.swiperInstances.push(castSwiper)
        }

        // Other carousels
        const otherContainers = document.querySelectorAll(".swiper-container:not(.cast-swiper)")
        otherContainers.forEach(container => {
            const swiper = new Swiper(container, {
                slidesPerView: "auto",
                spaceBetween: 20,
                grabCursor: true,
                lazy: { loadPrevNext: true },
                pagination: {
                    el: container.querySelector(".swiper-pagination"),
                    clickable: true,
                },
                breakpoints: {
                    320: { slidesPerView: 2, spaceBetween: 15 },
                    576: { slidesPerView: 3, spaceBetween: 20 },
                    768: { slidesPerView: 4, spaceBetween: 25 },
                    992: { slidesPerView: 5, spaceBetween: 30 },
                    1200: { slidesPerView: 6, spaceBetween: 30 },
                }
            })
            this.swiperInstances.push(swiper)
        })

        // Navigation button helper
        function updateNavButtons(swiperInstance) {
            const prevBtn = document.querySelector(".cast-nav-prev")
            const nextBtn = document.querySelector(".cast-nav-next")

            if (prevBtn) {
                prevBtn.style.opacity = swiperInstance.isBeginning ? "0.3" : "1"
                prevBtn.style.pointerEvents = swiperInstance.isBeginning ? "none" : "auto"
            }
            if (nextBtn) {
                nextBtn.style.opacity = swiperInstance.isEnd ? "0.3" : "1"
                nextBtn.style.pointerEvents = swiperInstance.isEnd ? "none" : "auto"
            }
        }
    }

    // Scroll animations with Intersection Observer
    async initializeScrollAnimations() {
        const targets = document.querySelectorAll(".reveal-on-scroll")
        if (!targets.length) return

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, index) => {
                if (entry.isIntersecting) {
                    setTimeout(() => {
                        entry.target.classList.add("is-visible")
                    }, index * APP_CONFIG.TIMING.ITEM_STAGGER_DELAY)
                    observer.unobserve(entry.target)
                }
            })
        }, {
            threshold: APP_CONFIG.ANIMATION.INTERSECTION_THRESHOLD,
            rootMargin: APP_CONFIG.ANIMATION.INTERSECTION_ROOT_MARGIN
        })

        targets.forEach(target => observer.observe(target))
        this.observers.set("scroll-animations", observer)
    }

    // Parallax effect with optimized performance
    async initializeParallax() {
        const hero = document.querySelector(".movie-hero")
        if (!hero) return

        // Use CSS custom properties for better performance
        let ticking = false

        const handleScroll = () => {
            if (!ticking) {
                requestAnimationFrame(() => {
                    const scrollY = window.scrollY
                    // Use CSS custom property instead of direct transform
                    hero.style.setProperty('--scroll-y', `${scrollY * 0.5}px`)
                    ticking = false
                })
                ticking = true
            }
        }

        // Use passive listener for better scroll performance
        window.addEventListener("scroll", handleScroll, { passive: true })
        this.eventListeners.push({ event: "scroll", handler: handleScroll })

        // Add CSS support for the parallax effect
        this.addParallaxCSS()
    }

    // Add CSS for optimized parallax effect
    addParallaxCSS() {
        const styleId = 'parallax-optimization'
        if (document.getElementById(styleId)) return

        const style = document.createElement('style')
        style.id = styleId
        style.textContent = `
            .movie-hero {
                transform: translateY(var(--scroll-y, 0px));
                will-change: transform;
                /* Use CSS containment for better performance */
                contain: layout style paint;
            }
            
            /* Enable hardware acceleration and optimize for scroll performance */
            @media (prefers-reduced-motion: no-preference) {
                .movie-hero {
                    /* Use translate3d for hardware acceleration */
                    transform: translate3d(0, var(--scroll-y, 0px), 0);
                }
            }
            
            /* Respect user's motion preferences */
            @media (prefers-reduced-motion: reduce) {
                .movie-hero {
                    transform: none !important;
                }
            }
        `
        document.head.appendChild(style)
    }

    // Initialize expandable comments
    async initializeComments() {
        const commentTexts = document.querySelectorAll(".comment-text p")
        if (!commentTexts.length) return

        commentTexts.forEach(textElement => {
            const textContent = textElement.textContent || textElement.innerText
            if (textContent.length > APP_CONFIG.LIMITS.COMMENT_PREVIEW_LENGTH) {
                new ExpandableComment(textElement, textContent)
            }
        })
    }

    // Staff section toggle
    async initializeStaffSection() {
        const toggleBtn = document.querySelector("#staffToggleBtn")
        const staffGrid = document.querySelector("#staffGrid")
        const toggleContainer = document.querySelector(".staff-toggle-container")
        const hiddenMembers = document.querySelectorAll(".staff-hidden")

        if (!toggleBtn || !staffGrid || !hiddenMembers.length || !toggleContainer) return

        let isExpanded = false

        // Function to calculate the button position after the first 4 members
        const updateButtonPosition = () => {
            if (isExpanded) {
                // When expanded, position the button at the end normally
                toggleContainer.classList.remove("initial-position")
                toggleContainer.classList.add("expanded-position")
                staffGrid.classList.remove("has-toggle-button")
                staffGrid.classList.add("expanded")
            } else {
                // When collapsed, position the button after the first 4 members
                const visibleMembers = staffGrid.querySelectorAll('.staff-member.staff-visible')
                const initialMembers = Array.from(staffGrid.querySelectorAll('.staff-member')).slice(0, 4)

                if (initialMembers.length > 0) {
                    // Find the last visible initial member
                    const lastVisibleInitialMember = initialMembers[initialMembers.length - 1]
                    const rect = lastVisibleInitialMember.getBoundingClientRect()
                    const containerRect = staffGrid.getBoundingClientRect()

                    // Calculate the relative position within the container
                    const relativeTop = rect.bottom - containerRect.top + 20

                    toggleContainer.classList.remove("expanded-position")
                    toggleContainer.classList.add("initial-position")
                    toggleContainer.style.top = `${relativeTop}px`
                    staffGrid.classList.add("has-toggle-button")
                    staffGrid.classList.remove("expanded")
                }
            }
        }

        // Initialize the button position
        if (toggleContainer) {
            setTimeout(updateButtonPosition, 100)
        } else {
            // If there's no button, ensure there's no extra spacing
            staffGrid.classList.remove("has-toggle-button")
        }

        // Update button position when the window is resized
        window.addEventListener('resize', updateButtonPosition)

        const toggleStaff = () => {
            isExpanded = !isExpanded
            const icon = toggleBtn.querySelector(".staff-toggle-icon")

            // Convert NodeList to Array to use reverse()
            const hiddenMembersArray = Array.from(hiddenMembers)

            if (isExpanded) {
                // Expand – show hidden members (from first to last)
                hiddenMembersArray.forEach((member, index) => {
                    setTimeout(() => {
                        // Remove the hidden class and add expanding for animation
                        member.classList.remove("staff-hidden")
                        member.classList.add("staff-expanding")

                        // After the animation, remove expanding and add visible
                        setTimeout(() => {
                            member.classList.remove("staff-expanding")
                            member.classList.add("staff-visible")
                        }, 400) // Animation duration
                    }, index * 100) // Stagger delay between items
                })

                icon.classList.replace("fa-chevron-down", "fa-chevron-up")
                toggleBtn.setAttribute("aria-expanded", "true")
                toggleBtn.setAttribute("title", "Show fewer team members")

                // Update the button position after a short delay
                setTimeout(updateButtonPosition, 200)

            } else {
                // Collapse – hide members (from last to first – REVERSED ORDER)
                hiddenMembersArray.reverse().forEach((member, index) => {
                    setTimeout(() => {
                        // Remove visible and add collapsing for animation
                        member.classList.remove("staff-visible")
                        member.classList.add("staff-collapsing")

                        // After the animation, remove collapsing and add hidden
                        setTimeout(() => {
                            member.classList.remove("staff-collapsing")
                            member.classList.add("staff-hidden")
                        }, 400)
                    }, index * 80)
                })

                icon.classList.replace("fa-chevron-up", "fa-chevron-down")
                toggleBtn.setAttribute("aria-expanded", "false")
                toggleBtn.setAttribute("title", "Show more team members")

                // Immediately updates the button position to return to the initial state
                setTimeout(updateButtonPosition, 100)
            }
        }

        // Event listeners
        toggleBtn.addEventListener("click", toggleStaff)
        toggleBtn.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault()
                toggleStaff()
            }
        })

        toggleBtn.addEventListener("click", () => {
            setTimeout(() => {
                if (isExpanded) {
                    const lastVisibleMember = staffGrid.querySelector('.staff-member:last-child')
                    if (lastVisibleMember) {
                        lastVisibleMember.scrollIntoView({
                            behavior: 'smooth',
                            block: 'nearest'
                        })
                    }
                } else {
                    toggleBtn.scrollIntoView({
                        behavior: 'smooth',
                        block: 'nearest'
                    })
                }
            }, 200)
        })
    }

    // Utility: Throttle function
    throttle(func, limit) {
        let inThrottle
        return function () {
            const args = arguments
            const context = this
            if (!inThrottle) {
                func.apply(context, args)
                inThrottle = true
                setTimeout(() => inThrottle = false, limit)
            }
        }
    }

    // Cleanup method
    destroy() {
        try {
            // Remove event listeners
            this.eventListeners.forEach(({ event, handler }) => {
                window.removeEventListener(event, handler)
            })

            // Disconnect observers
            this.observers.forEach(observer => observer.disconnect())

            // Destroy Swiper instances
            this.swiperInstances.forEach(swiper => {
                if (swiper && typeof swiper.destroy === "function") {
                    swiper.destroy(true, true)
                }
            })

            // Remove parallax CSS
            const parallaxStyle = document.getElementById('parallax-optimization')
            if (parallaxStyle) {
                parallaxStyle.remove()
            }

            this.isInitialized = false
            console.log("MovieDetailsManager destroyed")
        } catch (error) {
            console.error("Error during cleanup:", error)
        }
    }
}

// Simplified Expandable Comment class
class ExpandableComment {
    constructor(element, textContent) {
        this.element = element
        this.fullText = textContent
        this.shortText = textContent.substring(0, APP_CONFIG.LIMITS.COMMENT_PREVIEW_LENGTH) + "..."
        this.isExpanded = false
        this.init()
    }

    init() {
        this.button = document.createElement("button")
        this.button.className = "btn btn-link btn-sm p-0 ms-1"
        this.button.textContent = "Ver mais"
        this.button.setAttribute("type", "button")
        this.button.setAttribute("aria-expanded", "false")

        this.button.addEventListener("click", (e) => {
            e.preventDefault()
            this.toggle()
        })

        this.collapse()
    }

    toggle() {
        this.isExpanded = !this.isExpanded
        if (this.isExpanded) {
            this.expand()
        } else {
            this.collapse()
        }
    }

    expand() {
        this.element.textContent = this.fullText
        this.button.textContent = "Ver menos"
        this.button.setAttribute("aria-expanded", "true")
        this.element.appendChild(document.createTextNode(" "))
        this.element.appendChild(this.button)
    }

    collapse() {
        this.element.textContent = this.shortText
        this.button.textContent = "Ver mais"
        this.button.setAttribute("aria-expanded", "false")
        this.element.appendChild(document.createTextNode(" "))
        this.element.appendChild(this.button)
    }
}

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
    window.movieDetailsManager = new MovieDetailsManager()
})

// Cleanup on page unload
window.addEventListener("beforeunload", () => {
    if (window.movieDetailsManager) {
        window.movieDetailsManager.destroy()
    }
})

// Handle page visibility changes for performance
document.addEventListener("visibilitychange", () => {
    if (document.hidden) {
        console.log("Page hidden - pausing animations")
    } else {
        console.log("Page visible - resuming animations")
    }
})