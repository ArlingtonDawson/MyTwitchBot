async function loadCredits() {
    const response = await fetch('endstream.json');
    const data = await response.json();

    const container = document.getElementById('scrollContent');
    let html = '';

    // Subscribers section
    if (data.subscribers.length > 0) {
        html += buildSection('⭐', 'New Subscribers',
            data.subscribers.map(name =>
                `<div class="name">${name}</div>`).join(''));
    }

    // Gifters section
    if (data.gifters.length > 0) {
        html += buildSection('🎁', 'Gift Subscribers',
            data.gifters.map(g =>
                `<div class="name">${g.username}
                    <span class="gifter-count">x${g.count}</span>
                </div>`).join(''));
    }

    // Followers section
    if (data.followers.length > 0) {
        html += buildSection('❤️', 'New Followers',
            data.followers.map(name =>
                `<div class="name">${name}</div>`).join(''));
    }

    // Returning Viewers section
    if (data.returningViewers.length > 0) {
        html += buildSection('👀', 'Thanks For Chatting',
            data.returningViewers.map(name =>
                `<div class="name">${name}</div>`).join(''));
    }

    html += `
        <div class="divider"></div>
        <div class="stream-end">THANKS FOR WATCHING</div>`;

    container.innerHTML = html;

    // Calculate scroll duration based on number of entries
    const totalEntries = data.subscribers.length
        + data.gifters.length
        + data.followers.length;
    const duration = Math.min(30 + (totalEntries * 2), 180);

    container.style.animation = `scrollUp ${duration}s linear forwards`;

    // Inject the keyframe animation
    const style = document.createElement('style');
    style.textContent = `
        @keyframes scrollUp {
            from { transform: translateY(0); }
            to { transform: translateY(-100%); }
        }`;
    document.head.appendChild(style);
}

function buildSection(icon, title, namesHtml) {
    return `
        <div class="section-icon">${icon}</div>
        <div class="section-title">${title}</div>
        <div class="divider"></div>
        ${namesHtml}`;
}

loadCredits();