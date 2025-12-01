// Filter courses
function filterCourses() {
    const searchText = document.getElementById('searchInput').value.toLowerCase();
    const semester = document.getElementById('semesterFilter').value;
    const year = document.getElementById('yearFilter').value;
    const rows = document.querySelectorAll('#courseTableBody tr');

    let visibleCount = 0;
    let totalCredits = 0;
    let totalScore = 0;
    let scoreCount = 0;

    rows.forEach(row => {
        const courseName = row.dataset.name;
        const courseCode = row.dataset.code;
        const rowSemester = row.dataset.semester;
        const rowYear = row.dataset.year;

        let show = true;

        // Filter by search
        if (searchText && !courseName.includes(searchText) && !courseCode.includes(searchText)) {
            show = false;
        }

        // Filter by semester
        if (semester && rowSemester !== semester) {
            show = false;
        }

        // Filter by year
        if (year && rowYear !== year) {
            show = false;
        }

        row.style.display = show ? '' : 'none';

        if (show) {
            visibleCount++;
            // Calculate stats
            const creditsText = row.querySelector('.credits-badge').textContent;
            const credits = parseInt(creditsText.match(/\d+/)[0]);
            totalCredits += credits;

            const scoreText = row.querySelector('.score-display').textContent.trim();
            if (scoreText !== 'Chưa có') {
                const score = parseFloat(scoreText);
                totalScore += score;
                scoreCount++;
            }
        }
    });

    // Update summary - Cập nhật các thẻ thống kê dựa trên kết quả lọc
    document.getElementById('totalCourses').textContent = visibleCount;
    document.getElementById('totalCredits').textContent = totalCredits;
    const avgScoreElement = document.getElementById('avgScore');
    avgScoreElement.textContent = scoreCount > 0 ? (totalScore / scoreCount).toFixed(2) : '0.0';
}

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    // Set default filter values to latest semester/year
    const latestSemester = '@latestSemester';
    const latestYear = '@latestAcademicYear';

    if (latestSemester) {
        document.getElementById('semesterFilter').value = latestSemester;
    }
    if (latestYear) {
        document.getElementById('yearFilter').value = latestYear;
    }

    // Run filter on page load
    filterCourses();
});