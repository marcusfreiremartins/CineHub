class MovieCarousel {
    constructor(containerId, itemsPerPage = 5) {
        this.container = document.getElementById(containerId);
        this.track = this.container.querySelector('.carousel-track');
        this.prevBtn = this.container.querySelector('.carousel-prev');
        this.nextBtn = this.container.querySelector('.carousel-next');
        this.items = this.container.querySelectorAll('.movie-card-carousel');

        this.itemsPerPage = itemsPerPage;
        this.currentPage = 0;
        this.totalPages = Math.ceil(this.items.length / this.itemsPerPage);

        // Properties for layout control
        this.cardWidth = 280;
        this.cardGap = 24;
        this.carouselPadding = 64;

        this.init();
    }

    init() {
        // Set up button events
        this.prevBtn.addEventListener('click', () => this.goToPrevPage());
        this.nextBtn.addEventListener('click', () => this.goToNextPage());

        // Set track width and layout
        this.updateLayout();

        // Update button states
        this.updateButtons();

        // Add touch events for mobile
        this.addTouchEvents();

        // Resize when screen size changes
        window.addEventListener('resize', () => {
            // Debounce resize events
            clearTimeout(this.resizeTimeout);
            this.resizeTimeout = setTimeout(() => this.updateLayout(), 150);
        });
    }

    updateLayout() {
        // Get container dimensions
        const containerRect = this.container.getBoundingClientRect();
        const availableWidth = containerRect.width - this.carouselPadding;

        // Set responsive card sizes
        let currentCardWidth = this.cardWidth;

        if (window.innerWidth <= 480) {
            currentCardWidth = 200;
            this.cardWidth = 200;
        } else if (window.innerWidth <= 768) {
            currentCardWidth = 240;
            this.cardWidth = 240;
        } else if (window.innerWidth <= 1200) {
            currentCardWidth = 260;
            this.cardWidth = 260;
        } else {
            currentCardWidth = 280;
            this.cardWidth = 280;
        }

        // Calculate how many items fit based on screen width
        const maxItemsBasedOnWidth = Math.floor(availableWidth / (currentCardWidth + this.cardGap));

        // Set responsive itemsPerPage based on screen width with max constraint
        if (window.innerWidth <= 480) {
            this.itemsPerPage = Math.max(1, Math.min(1, maxItemsBasedOnWidth));
        } else if (window.innerWidth <= 768) {
            this.itemsPerPage = Math.max(1, Math.min(2, maxItemsBasedOnWidth));
        } else if (window.innerWidth <= 1024) {
            this.itemsPerPage = Math.max(1, Math.min(3, maxItemsBasedOnWidth));
        } else if (window.innerWidth <= 1200) {
            this.itemsPerPage = Math.max(1, Math.min(4, maxItemsBasedOnWidth));
        } else {
            this.itemsPerPage = Math.max(1, Math.min(5, maxItemsBasedOnWidth));
        }

        // Ensure at least 1 item per page
        this.itemsPerPage = Math.max(1, this.itemsPerPage);

        // Recalculate total pages
        this.totalPages = Math.ceil(this.items.length / this.itemsPerPage);

        // Adjust current page if necessary
        if (this.currentPage >= this.totalPages) {
            this.currentPage = Math.max(0, this.totalPages - 1);
        }

        // Set the track width to fit all items
        const totalTrackWidth = this.items.length * (currentCardWidth + this.cardGap) - this.cardGap;
        this.track.style.width = `${totalTrackWidth}px`;

        // Apply consistent width to the cards
        this.items.forEach(item => {
            item.style.width = `${currentCardWidth}px`;
            item.style.minWidth = `${currentCardWidth}px`;
            item.style.maxWidth = `${currentCardWidth}px`;
            item.style.flex = `0 0 ${currentCardWidth}px`;
        });

        // Update transform and buttons
        this.updateTransform();
        this.updateButtons();
    }

    goToNextPage() {
        if (this.currentPage < this.totalPages - 1) {
            this.currentPage++;
            this.updateTransform();
            this.updateButtons();
        }
    }

    goToPrevPage() {
        if (this.currentPage > 0) {
            this.currentPage--;
            this.updateTransform();
            this.updateButtons();
        }
    }

    updateTransform() {
        // Calculate translation based on current page and items per page
        const itemsToMove = this.currentPage * this.itemsPerPage;
        const translateX = itemsToMove * (this.cardWidth + this.cardGap);

        this.track.style.transform = `translateX(-${translateX}px)`;
    }

    updateButtons() {
        // Check if we are on the first page
        const isFirstPage = this.currentPage === 0;

        // Check if we are on the last page or if remaining items fit into current view
        const remainingItems = this.items.length - (this.currentPage * this.itemsPerPage);
        const isLastPage = this.currentPage >= this.totalPages - 1 || remainingItems <= this.itemsPerPage;

        this.prevBtn.disabled = isFirstPage;
        this.nextBtn.disabled = isLastPage;

        // Hide buttons if there are not enough items to paginate
        const showControls = this.items.length > this.itemsPerPage;
        this.prevBtn.style.display = showControls ? 'flex' : 'none';
        this.nextBtn.style.display = showControls ? 'flex' : 'none';
    }

    addTouchEvents() {
        let startX = 0;
        let currentX = 0;
        let isDragging = false;
        let startTransform = 0;

        this.track.addEventListener('touchstart', (e) => {
            startX = e.touches[0].clientX;
            isDragging = true;

            // Get current transform value
            const style = window.getComputedStyle(this.track);
            const matrix = new DOMMatrixReadOnly(style.transform);
            startTransform = matrix.m41;
        });

        this.track.addEventListener('touchmove', (e) => {
            if (!isDragging) return;
            e.preventDefault();

            currentX = e.touches[0].clientX;
            const diffX = currentX - startX;

            // Apply temporary transform during drag
            this.track.style.transform = `translateX(${startTransform + diffX}px)`;
        });

        this.track.addEventListener('touchend', (e) => {
            if (!isDragging) return;
            isDragging = false;

            const diffX = startX - currentX;
            const threshold = 50;

            // Reset to proper position
            this.updateTransform();

            // Navigate if threshold met
            if (Math.abs(diffX) > threshold) {
                if (diffX > 0 && !this.nextBtn.disabled) {
                    this.goToNextPage();
                } else if (diffX < 0 && !this.prevBtn.disabled) {
                    this.goToPrevPage();
                }
            }
        });

        // Prevent default touch behavior on carousel
        this.track.addEventListener('touchcancel', () => {
            if (isDragging) {
                isDragging = false;
                this.updateTransform();
            }
        });
    }

    // Method to refresh carousel (useful for dynamic content updates)
    refresh() {
        this.items = this.container.querySelectorAll('.movie-card-carousel');
        this.updateLayout();
    }

    // Method to go to a specific page
    goToPage(pageIndex) {
        if (pageIndex >= 0 && pageIndex < this.totalPages) {
            this.currentPage = pageIndex;
            this.updateTransform();
            this.updateButtons();
        }
    }
}

// Initialize all carousels on DOMContentLoaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize all carousels on the page
    const carouselContainers = document.querySelectorAll('.movie-carousel-container');

    carouselContainers.forEach((container) => {
        if (container.id) {
            new MovieCarousel(container.id);
        }
    });
});

// Helper function to truncate text
function truncateText(text, maxLength) {
    if (text.length <= maxLength) return text;
    return text.substring(0, maxLength).trim() + '...';
}

// Export for module use if needed
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MovieCarousel;
}