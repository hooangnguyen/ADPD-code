function setScore(submissionId, score) {
    const form = document.getElementById('form-' + submissionId);
    const input = form.querySelector('input[name="score"]');
    input.value = score;
}

function toggleGradeForm(submissionId) {
    const graded = document.getElementById('graded-' + submissionId);
    const form = document.getElementById('form-' + submissionId);

    if (form.style.display === 'none') {
        form.style.display = 'block';
        graded.style.display = 'none';
    } else {
        form.style.display = 'none';
        graded.style.display = 'block';
    }
}

function filterSubmissions(status) {
    const cards = document.querySelectorAll('.submission-card');
    const buttons = document.querySelectorAll('.filter-btn');

    // Update active button
    buttons.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    // Filter cards
    cards.forEach(card => {
        if (status === 'all') {
            card.style.display = 'block';
        } else {
            card.style.display = card.dataset.status === status ? 'block' : 'none';
        }
    });
}