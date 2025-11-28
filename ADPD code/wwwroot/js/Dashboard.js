(function () {
    const contentHost = document.getElementById('dynamic-content');
    if (!contentHost) {
        console.error('Không tìm thấy #dynamic-content');
        return;
    }

    const defaultUrl = contentHost.dataset.defaultUrl;

    function setActive(link) {
        const menuLinks = document.querySelectorAll('.menu-item a[data-partial-url]');
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

    function attachPartialLinks() {
        const partialLinks = document.querySelectorAll('[data-partial-url]');
        partialLinks.forEach(link => {
            // Remove existing listeners by cloning
            const newLink = link.cloneNode(true);
            link.parentNode?.replaceChild(newLink, link);
            
            newLink.addEventListener('click', event => {
                event.preventDefault();
                const url = newLink.dataset.partialUrl;
                if (url) {
                    loadPartial(url, newLink);
                }
            });
        });
    }

    async function loadPartial(url, sourceLink, pushState = true) {
        if (!url) {
            console.error('URL không hợp lệ');
            return;
        }

        if (!contentHost) {
            console.error('Không tìm thấy #dynamic-content');
            return;
        }

        contentHost.innerHTML = '<div style="padding: 40px; text-align: center;">Đang tải...</div>';

        try {
            console.log('Đang tải:', url);
            const response = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();
            console.log('Đã nhận được HTML, độ dài:', html.length);
            
            contentHost.innerHTML = html;

            // Re-attach event listeners sau khi load content mới
            attachPartialLinks();

            if (pushState && sourceLink) {
                history.pushState({ partialUrl: url, href: sourceLink.href }, '', sourceLink.href);
            }

            setActive(sourceLink);
        } catch (error) {
            console.error('Lỗi khi tải nội dung:', error);
            contentHost.innerHTML = `<div style="padding: 40px; text-align: center; color: red;">
                <h3>Lỗi</h3>
                <p>${error.message}</p>
                <p>Vui lòng thử lại hoặc làm mới trang.</p>
            </div>`;
        }
    }

    // Xử lý form submit trong dynamic content
    document.addEventListener('submit', async function(event) {
        const form = event.target;
        if (form.tagName === 'FORM' && form.closest('#dynamic-content')) {
            const formAction = form.getAttribute('action');
            if (formAction && formAction.includes('RegisterCourse')) {
                event.preventDefault();
                
                const formData = new FormData(form);
                const response = await fetch(formAction, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    credentials: 'same-origin'
                });

                if (response.ok) {
                    // Reload RegisterStudy sau khi đăng ký thành công
                    const registerStudyUrl = formAction.replace('RegisterCourse', 'RegisterStudy') + '?partial=true';
                    loadPartial(registerStudyUrl, null);
                } else {
                    alert('Có lỗi xảy ra khi đăng ký môn học. Vui lòng thử lại.');
                }
            }
        }
    });

    window.addEventListener('popstate', event => {
        const targetUrl = event.state?.partialUrl || defaultUrl;
        loadPartial(targetUrl, null, false);
    });

    // Khởi tạo
    attachPartialLinks();
    if (defaultUrl) {
        history.replaceState({ partialUrl: defaultUrl, href: window.location.href }, '', window.location.href);
    }
})();