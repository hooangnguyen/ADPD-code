(function () {
    const contentHost = document.getElementById('dynamic-content');
    if (!contentHost) {
        console.error('Không tìm thấy #dynamic-content');
        return;
    }

    const defaultUrl = contentHost.dataset.defaultUrl;
    let isLoading = false;

    // Tạo loading indicator đẹp hơn
    function createLoadingIndicator() {
        return `
            <div style="padding: 60px; text-align: center;">
                <div style="display: inline-block; width: 50px; height: 50px; border: 4px solid #f3f3f3; 
                    border-top: 4px solid #3498db; border-radius: 50%; animation: spin 1s linear infinite;"></div>
                <p style="margin-top: 20px; color: #666; font-size: 16px;">Đang tải...</p>
            </div>
            <style>
                @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                }
            </style>
        `;
    }

    function setActive(link) {
        // Xóa active từ tất cả menu items
        const menuLinks = document.querySelectorAll('.menu-item a[data-partial-url]');
        menuLinks.forEach(l => l.classList.remove('active'));

        if (!link) {
            const dashboardLink = document.querySelector('.menu-item a[data-partial-url][href*="Dashboard"]');
            dashboardLink?.classList.add('active');
            return;
        }

        // Tìm link tương ứng trong menu
        let targetLink = link.closest('.menu-item') ? link : null;

        if (!targetLink) {
            // Tìm link trong menu có cùng href
            targetLink = Array.from(menuLinks).find(menuLink => {
                const menuHref = new URL(menuLink.href, window.location.origin).pathname;
                const linkHref = new URL(link.href, window.location.origin).pathname;
                return menuHref === linkHref;
            });
        }

        targetLink?.classList.add('active');
    }

    function attachPartialLinks() {
        // Xử lý tất cả các link có data-partial-url (bao gồm cả button trong dashboard)
        const partialLinks = document.querySelectorAll('[data-partial-url]');
        partialLinks.forEach(link => {
            // Kiểm tra xem link đã có event listener chưa
            if (link.dataset.listenerAttached === 'true') {
                return;
            }

            link.addEventListener('click', function(event) {
                event.preventDefault();
                event.stopPropagation();
                
                if (isLoading) {
                    return; // Ngăn nhiều request đồng thời
                }

                const url = link.dataset.partialUrl;
                if (url) {
                    loadPartial(url, link);
                }
            });

            // Đánh dấu đã attach listener
            link.dataset.listenerAttached = 'true';
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

        if (isLoading) {
            console.log('Đang có request đang xử lý, bỏ qua...');
            return;
        }

        isLoading = true;
        contentHost.innerHTML = createLoadingIndicator();

        try {
            console.log('Đang tải:', url);
            const response = await fetch(url, {
                headers: { 
                    'X-Requested-With': 'XMLHttpRequest',
                    'Accept': 'text/html'
                },
                credentials: 'same-origin'
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const html = await response.text();
            console.log('Đã nhận được HTML, độ dài:', html.length);
            
            // Thêm hiệu ứng fade in
            contentHost.style.opacity = '0';
            contentHost.innerHTML = html;
            
            // Thực thi các script tags trong HTML mới được load
            const scripts = contentHost.querySelectorAll('script');
            scripts.forEach(oldScript => {
                const newScript = document.createElement('script');
                Array.from(oldScript.attributes).forEach(attr => {
                    newScript.setAttribute(attr.name, attr.value);
                });
                newScript.appendChild(document.createTextNode(oldScript.innerHTML));
                oldScript.parentNode.replaceChild(newScript, oldScript);
            });
            
            // Fade in animation
            setTimeout(() => {
                contentHost.style.transition = 'opacity 0.3s ease-in';
                contentHost.style.opacity = '1';
            }, 50);

            // Re-attach event listeners sau khi load content mới
            attachPartialLinks();

            // Cập nhật URL trong browser
            if (pushState && sourceLink) {
                const newUrl = sourceLink.href || url;
                history.pushState({ partialUrl: url, href: newUrl }, '', newUrl);
            }

            // Cập nhật active state
            setActive(sourceLink);

            // Scroll to top
            window.scrollTo({ top: 0, behavior: 'smooth' });

        } catch (error) {
            console.error('Lỗi khi tải nội dung:', error);
            contentHost.innerHTML = `
                <div style="padding: 40px; text-align: center; color: #e74c3c;">
                    <div style="font-size: 48px; margin-bottom: 20px;">⚠️</div>
                    <h3 style="color: #e74c3c; margin-bottom: 10px;">Lỗi khi tải trang</h3>
                    <p style="color: #666; margin-bottom: 20px;">${error.message}</p>
                    <button onclick="location.reload()" 
                        style="padding: 10px 20px; background: #3498db; color: white; 
                        border: none; border-radius: 5px; cursor: pointer; font-size: 14px;">
                        Làm mới trang
                    </button>
                </div>
            `;
        } finally {
            isLoading = false;
        }
    }

    // Xử lý form submit trong dynamic content
    document.addEventListener('submit', async function(event) {
        const form = event.target;
        if (form.tagName === 'FORM' && form.closest('#dynamic-content')) {
            const formAction = form.getAttribute('action');
            if (formAction && formAction.includes('RegisterCourse')) {
                event.preventDefault();
                
                if (isLoading) {
                    return;
                }

                isLoading = true;
                const submitButton = form.querySelector('button[type="submit"], input[type="submit"]');
                const originalText = submitButton?.textContent || submitButton?.value;
                
                if (submitButton) {
                    submitButton.disabled = true;
                    if (submitButton.textContent) {
                        submitButton.textContent = 'Đang xử lý...';
                    } else {
                        submitButton.value = 'Đang xử lý...';
                    }
                }
                
                try {
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
                        await loadPartial(registerStudyUrl, null);
                    } else {
                        const errorText = await response.text();
                        alert('Có lỗi xảy ra khi đăng ký môn học. Vui lòng thử lại.');
                        console.error('Lỗi:', errorText);
                    }
                } catch (error) {
                    console.error('Lỗi khi submit form:', error);
                    alert('Có lỗi xảy ra. Vui lòng thử lại.');
                } finally {
                    isLoading = false;
                    if (submitButton) {
                        submitButton.disabled = false;
                        if (submitButton.textContent) {
                            submitButton.textContent = originalText;
                        } else {
                            submitButton.value = originalText;
                        }
                    }
                }
            }
        }
    });

    // Xử lý browser back/forward buttons
    window.addEventListener('popstate', event => {
        const targetUrl = event.state?.partialUrl || defaultUrl;
        if (targetUrl) {
            loadPartial(targetUrl, null, false);
        }
    });

    // Khởi tạo khi DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            attachPartialLinks();
            if (defaultUrl) {
                history.replaceState({ partialUrl: defaultUrl, href: window.location.href }, '', window.location.href);
            }
        });
    } else {
        attachPartialLinks();
        if (defaultUrl) {
            history.replaceState({ partialUrl: defaultUrl, href: window.location.href }, '', window.location.href);
        }
    }
})();