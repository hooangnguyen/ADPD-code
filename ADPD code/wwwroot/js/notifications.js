document.addEventListener('DOMContentLoaded', function () {
    (function () {
        const btn = document.getElementById('notificationBtn');
        const dropdown = document.getElementById('notificationDropdown');
        const badge = document.getElementById('notificationBadge');

        if (!btn || !dropdown) {
            console.debug('[notifications] notificationBtn or notificationDropdown not found in DOM');
            return;
        }

        let opened = false;

        async function loadNotifications() {
            try {
                dropdown.innerHTML = '<div style="padding:12px;">Loading...</div>';
                const res = await fetch('/Student/GetNotifications', {
                    method: 'GET',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    credentials: 'same-origin' // ensures session cookie is sent
                });

                if (res.ok) {
                    const html = await res.text();
                    dropdown.innerHTML = html;

                    const items = dropdown.querySelectorAll('.notification-item');
                    if (items.length > 0) {
                        badge.textContent = items.length;
                        badge.style.display = '';
                    } else {
                        badge.style.display = 'none';
                    }
                } else if (res.status === 401) {
                    dropdown.innerHTML = '<div style="padding:12px;">Chưa đăng nhập</div>';
                    console.warn('[notifications] unauthorized (401)');
                } else {
                    dropdown.innerHTML = '<div style="padding:12px;color:#c00;">Lỗi tải thông báo</div>';
                    console.error('[notifications] fetch failed', res.status);
                }
            } catch (err) {
                console.error('[notifications] fetch error', err);
                dropdown.innerHTML = '<div style="padding:12px;color:#c00;">Lỗi kết nối</div>';
            }
        }

        function openDropdown() {
            dropdown.style.display = 'block';
            btn.setAttribute('aria-expanded', 'true');
            opened = true;
            loadNotifications();
        }

        function closeDropdown() {
            dropdown.style.display = 'none';
            btn.setAttribute('aria-expanded', 'false');
            opened = false;
        }

        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            if (opened) closeDropdown();
            else openDropdown();
        });

        document.addEventListener('click', function (e) {
            if (opened && !dropdown.contains(e.target) && e.target !== btn) {
                closeDropdown();
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && opened) closeDropdown();
        });
    })();
});