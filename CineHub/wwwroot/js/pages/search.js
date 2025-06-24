document.addEventListener('DOMContentLoaded', () => {
    const searchForm = document.getElementById('searchForm');
    const resultsContainerSelector = '#search-results-section';
    const cardAnimationClass = '.content-card-animation';

    const paginator = new AjaxPaginator(resultsContainerSelector, cardAnimationClass);

    paginator.initialize();

    if (searchForm) {
        searchForm.addEventListener('submit', (event) => {
            event.preventDefault();

            const formData = new FormData(searchForm);
            const params = new URLSearchParams();

            for (let [key, value] of formData.entries()) {
                if (value) {
                    params.append(key, value);
                }
            }

            const url = `${searchForm.getAttribute('action')}?${params.toString()}`;

            paginator.loadPage(url);
        });
    }
});