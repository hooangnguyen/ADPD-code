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
                    credentials: 'same-origin'
                });

                if (res.ok) {
                    const html = await res.text();
                    dropdown.innerHTML = html;

                    // Count unread items
                    const items = dropdown.querySelectorAll('.notification-item');
                    let unread = 0;
                    items.forEach(it => {
                        const isRead = it.dataset.isread;
                        if (isRead === 'false' || isRead === 'False') unread++;
                    });

                    if (unread > 0) {
                        badge.textContent = unread;
                        badge.style.display = '';
                    } else {
                        badge.style.display = 'none';
                    }

                    // If user opened dropdown, mark all as read on server and update UI
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

        async function markAllRead() {
            try {
                const res = await fetch('/Student/MarkAllNotificationsRead', {
                    method: 'POST',
                    credentials: 'same-origin'
                });
                if (res.ok) {
                    // update UI: remove unread class and hide badge
                    dropdown.querySelectorAll('.notification-item').forEach(it => {
                        it.dataset.isread = 'true';
                        it.classList.remove('notification-unread');
                    });
                    badge.style.display = 'none';
                }
            } catch (err) {
                console.error('[notifications] markAllRead error', err);
            }
        }

        async function markSingleRead(id, element) {
            try {
                const res = await fetch('/Student/MarkNotificationRead?id=' + encodeURIComponent(id), {
                    method: 'POST',
                    credentials: 'same-origin'
                });
                if (res.ok) {
                    element.dataset.isread = 'true';
                    element.classList.remove('notification-unread');
                    // adjust badge
                    const remaining = dropdown.querySelectorAll('.notification-item').length;
                    const unreadNow = Array.from(dropdown.querySelectorAll('.notification-item')).filter(it => it.dataset.isread === 'false' || it.dataset.isread === 'False').length;
                    if (unreadNow > 0) {
                        badge.textContent = unreadNow;
                        badge.style.display = '';
                    } else {
                        badge.style.display = 'none';
                    }
                }
            } catch (err) {
                console.error('[notifications] markSingleRead error', err);
            }
        }

        function openDropdown() {
            dropdown.style.display = 'block';
            btn.setAttribute('aria-expanded', 'true');
            opened = true;
            loadNotifications().then(() => {
                // after loading UI, mark all read on server and update UI
                markAllRead();
            });
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

        // Delegate click on notification items to mark single read (and optionally navigate)
        document.addEventListener('click', function (e) {
            const item = e.target.closest('.notification-item');
            if (item && dropdown.contains(item)) {
                const id = item.dataset.id;
                // mark that notification read
                if (id) markSingleRead(id, item);
                // optional: you can navigate to details here, e.g. window.location = '/Student/Notifications/Details/' + id;
            }

            if (opened && !dropdown.contains(e.target) && e.target !== btn) {
                closeDropdown();
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && opened) closeDropdown();
        });
    })();
});