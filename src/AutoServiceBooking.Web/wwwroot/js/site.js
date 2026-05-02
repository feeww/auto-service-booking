(() => {
    const navLinks = Array.from(document.querySelectorAll('[data-nav-link]'));
    const sections = navLinks
        .map((link) => document.getElementById(link.dataset.navLink))
        .filter(Boolean);

    const setActiveLink = (sectionId) => {
        navLinks.forEach((link) => {
            link.classList.toggle('active', link.dataset.navLink === sectionId);
        });
    };

    if (sections.length === 0) {
        return;
    }

    const updateActiveLink = () => {
        const markerPosition = window.scrollY + 130;
        const pageBottom = window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 8;

        if (pageBottom) {
            setActiveLink(sections[sections.length - 1].id);
            return;
        }

        const currentSection = sections.reduce((current, section) => {
            return section.offsetTop <= markerPosition ? section : current;
        }, sections[0]);

        setActiveLink(currentSection.id);
    };

    window.addEventListener('scroll', updateActiveLink, { passive: true });
    window.addEventListener('hashchange', updateActiveLink);
    updateActiveLink();
})();
