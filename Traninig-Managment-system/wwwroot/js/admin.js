/* =========================================================
   TrainHub Prime UI — Minimal JS (No business logic)
   Handles: Sidebar, Accordion, Profile menu, Modals, Mobile,
            Theme toggle (dark/light) + persistence
   ========================================================= */

(() => {
    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));

    const sidebar = $('#sidebar');
    const dashboard = $('#dashboardRoot') || $('.dashboard');
    const modalBackdrop = $('#modalBackdrop');
    const profileMenu = $('#profileMenu');
    const profileBtn = $('[data-action="toggle-profile-menu"]');
    const mobileOverlay = $('#mobileOverlay');
    const themeBtn = $('[data-action="toggle-theme"]');

    // ------------ Theme (Dark/Light) ------------
    const THEME_KEY = 'trainhub-theme';

    function setTheme(next) {
        document.body.classList.remove('theme-dark', 'theme-light');
        document.body.classList.add(next);
        try { localStorage.setItem(THEME_KEY, next); } catch { }
    }

    // default: theme-dark (already in HTML) but respect saved value
    try {
        const saved = localStorage.getItem(THEME_KEY);
        if (saved === 'theme-light' || saved === 'theme-dark') setTheme(saved);
    } catch { }

    themeBtn?.addEventListener('click', () => {
        const isDark = document.body.classList.contains('theme-dark');
        setTheme(isDark ? 'theme-light' : 'theme-dark');
    });

    // ------------ Sidebar collapse (desktop) ------------
    $('#sidebarToggle')?.addEventListener('click', () => {
        sidebar?.classList.toggle('sidebar--collapsed');
        // optional helper class (your CSS supports it)
        dashboard?.classList.toggle('dashboard--sidebar-collapsed');
    });

    // ------------ Accordion (sidebar sections) ------------
    function toggleSection(btn) {
        if (!btn) return;
        const expanded = btn.getAttribute('aria-expanded') === 'true';
        btn.setAttribute('aria-expanded', String(!expanded));
    }

    $$('[data-action="toggle-section"]').forEach(btn => {
        btn.addEventListener('click', () => toggleSection(btn));
    });

    // ------------ Mobile menu + overlay ------------
    function openMobileMenu() {
        if (!sidebar) return;
        sidebar.classList.add('sidebar--mobile-open');
        if (mobileOverlay) {
            mobileOverlay.hidden = false;
            mobileOverlay.classList.add('mobile-overlay--visible');
        }
    }

    function closeMobileMenu() {
        if (!sidebar) return;
        sidebar.classList.remove('sidebar--mobile-open');
        if (mobileOverlay) {
            mobileOverlay.classList.remove('mobile-overlay--visible');
            // delay so fade-out is visible
            window.setTimeout(() => { mobileOverlay.hidden = true; }, 180);
        }
    }

    $('[data-action="open-mobile-menu"]')?.addEventListener('click', openMobileMenu);
    mobileOverlay?.addEventListener('click', closeMobileMenu);

    // Close mobile menu if viewport grows
    window.addEventListener('resize', () => {
        if (window.innerWidth > 768) closeMobileMenu();
    });

    // ------------ Profile dropdown ------------
    function openProfileMenu() {
        if (!profileMenu || !profileBtn) return;
        profileBtn.setAttribute('aria-expanded', 'true');
        profileMenu.hidden = false;
    }

    function closeProfileMenu() {
        if (!profileMenu || !profileBtn) return;
        profileBtn.setAttribute('aria-expanded', 'false');
        profileMenu.hidden = true;
    }

    function toggleProfileMenu() {
        if (!profileMenu || !profileBtn) return;
        const expanded = profileBtn.getAttribute('aria-expanded') === 'true';
        if (expanded) closeProfileMenu();
        else openProfileMenu();
    }

    profileBtn?.addEventListener('click', (e) => {
        e.stopPropagation();
        toggleProfileMenu();
    });

    document.addEventListener('click', (e) => {
        // close if clicked outside dropdown
        const dropdown = $('#profileDropdown');
        if (!dropdown) return;
        if (!dropdown.contains(e.target)) closeProfileMenu();
    });

    // ------------ Modals (dialog) + backdrop ------------
    const openDialogs = new Set();

    function syncBackdrop() {
        if (!modalBackdrop) return;
        const anyOpen = openDialogs.size > 0;
        modalBackdrop.hidden = !anyOpen;
    }

    function openModalById(id) {
        const dlg = document.getElementById(id);
        if (!dlg || typeof dlg.showModal !== 'function') return;

        try {
            dlg.showModal();
            openDialogs.add(dlg);
            syncBackdrop();
            closeProfileMenu(); // nice UX
            closeMobileMenu();  // nice UX
        } catch {
            // If already open or blocked, ignore
        }
    }

    function closeModal(dlg) {
        if (!dlg) return;
        try { dlg.close(); } catch { }
        openDialogs.delete(dlg);
        syncBackdrop();
    }

    function closeTopModal() {
        const last = Array.from(openDialogs).pop();
        if (last) closeModal(last);
    }

    // Close when clicking backdrop
    modalBackdrop?.addEventListener('click', () => closeTopModal());

    // Close when dialog itself fires close
    $$('dialog.modal').forEach(dlg => {
        dlg.addEventListener('close', () => {
            openDialogs.delete(dlg);
            syncBackdrop();
        });
        // Click outside dialog content closes it (native behavior workaround)
        dlg.addEventListener('click', (e) => {
            const rect = dlg.getBoundingClientRect();
            const inDialog =
                e.clientX >= rect.left && e.clientX <= rect.right &&
                e.clientY >= rect.top && e.clientY <= rect.bottom;
            if (!inDialog) closeModal(dlg);
        });
    });

    // Event delegation for modal open/close buttons
    document.addEventListener('click', (e) => {
        const openBtn = e.target.closest('[data-action="open-modal"]');
        const closeBtn = e.target.closest('[data-action="close-modal"]');

        if (openBtn) {
            const id = openBtn.getAttribute('data-modal');
            if (id) openModalById(id);
            return;
        }

        if (closeBtn) {
            const dlg = closeBtn.closest('dialog.modal');
            if (dlg) closeModal(dlg);
            return;
        }
    });

    // ESC closes profile + top modal + mobile menu
    document.addEventListener('keydown', (e) => {
        if (e.key !== 'Escape') return;
        closeProfileMenu();
        closeMobileMenu();
        closeTopModal();
    });

    // ------------ Optional: tiny toast helper (silent) ------------
    // You can call window.TrainHubToast('Saved!', 'success')
    window.TrainHubToast = (msg, type = 'info') => {
        const container = $('#toastContainer');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast toast--${type}`;
        toast.setAttribute('role', 'alert');

        const icon = document.createElement('span');
        icon.className = 'toast__icon';
        icon.setAttribute('aria-hidden', 'true');

        const p = document.createElement('p');
        p.className = 'toast__message';
        p.textContent = msg;

        const x = document.createElement('button');
        x.className = 'toast__close';
        x.setAttribute('aria-label', 'Dismiss notification');
        x.innerHTML = '&times;';

        x.addEventListener('click', () => {
            toast.classList.remove('toast--visible');
            window.setTimeout(() => toast.remove(), 220);
        });

        toast.append(icon, p, x);
        container.appendChild(toast);

        // animate in
        requestAnimationFrame(() => toast.classList.add('toast--visible'));

        // auto close
        window.setTimeout(() => {
            if (!toast.isConnected) return;
            toast.classList.remove('toast--visible');
            window.setTimeout(() => toast.remove(), 220);
        }, 3200);
    };
})();
