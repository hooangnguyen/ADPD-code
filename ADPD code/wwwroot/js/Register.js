//code register 
document.getElementById('registerForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const password = document.querySelector('input[name="password"]').value;
    const confirmPassword = document.querySelector('input[name="confirmPassword"]').value;

    if (password !== confirmPassword) {
        alert('Mật khẩu xác nhận không khớp!');
        return;
    }

    // Lấy dữ liệu form
    const formData = new FormData(this);
    const data = Object.fromEntries(formData);

    console.log('Dữ liệu đăng ký:', data);
    alert('Đăng ký thành công! (Đây là demo, bạn cần kết nối với backend ASP.NET)');

    // Ở đây bạn sẽ gửi dữ liệu đến server ASP.NET
    // fetch('/api/register', {
    //     method: 'POST',
    //     headers: { 'Content-Type': 'application/json' },
    //     body: JSON.stringify(data)
    // }).then(response => response.json())
    //   .then(data => console.log(data));
});
