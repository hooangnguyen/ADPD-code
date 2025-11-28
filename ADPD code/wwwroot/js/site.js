// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
function showTab(tabName) {

    var upcomingTab = document.getElementById('upcoming-tab');
    var overdueTab = document.getElementById('overdue-tab');
    var tabBtns = document.querySelectorAll('.tab-btn');

    tabBtns.forEach(function (btn) {
        btn.classList.remove('active');
    });

    if (tabName === 'upcoming') {
        upcomingTab.style.display = '';
        overdueTab.style.display = 'none';
        tabBtns[0].classList.add('active');
    } else {
        upcomingTab.style.display = 'none';
        overdueTab.style.display = '';
        tabBtns[1].classList.add('active');
    }
}
// Write your JavaScript code.