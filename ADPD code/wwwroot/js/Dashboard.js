(function () {
    const contentHost = document.getElementById('dynamic-content');
    const menuLinks = document.querySelectorAll('.menu-item a[data-partial-url]');
    const partialLinks = document.querySelectorAll('[data-partial-url]');
    const defaultUrl = contentHost.dataset.defaultUrl;

    function setActive(link) {
        menuLinks.forEach(l => l.classList.remove('active'));

        if (!link) {
            const dashboardLink = document.querySelector('.menu-item a[data-partial-url][href*="Dashboard"]');
            dashboardLink?.classList.add('active');
            return;
        }

        let targetLink = link.closest('.menu-item') ? link : null;

        if (!targetLink) {
            targetLink = Array.from(menuLinks).find(menuLink => menuLink.href === link.href);
        }

        targetLink?.classList.add('active');
    }

    async function loadPartial(url, sourceLink, pushState = true) {
        if (!url) {
            return;
        }

        contentHost.innerHTML = '<div class="loading-state">Đang tải...</div>';

        try {
            const response = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            });

            if (!response.ok) {
                throw new Error('Không thể tải nội dung.');
            }

            const html = await response.text();
            contentHost.innerHTML = html;

            if (pushState) {
                history.pushState({ partialUrl: url, href: sourceLink?.href }, '', sourceLink?.href || window.location.href);
            }

            setActive(sourceLink);
        } catch (error) {
            contentHost.innerHTML = `<div class="error-state">${error.message}</div>`;
        }
    }

    partialLinks.forEach(link => {
        link.addEventListener('click', event => {
            event.preventDefault();
            loadPartial(link.dataset.partialUrl, link);
        });
    });

    window.addEventListener('popstate', event => {
        const targetUrl = event.state?.partialUrl || defaultUrl;
        loadPartial(targetUrl, null, false);
    });

    history.replaceState({ partialUrl: defaultUrl, href: window.location.href }, '', window.location.href);
})();