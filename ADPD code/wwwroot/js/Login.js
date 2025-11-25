document.getElementById('loginForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const remember = document.getElementById('remember').checked;

    // Validate
    if (!username || !password) {
        showError('Vui lòng nhập đầy đủ thông tin!');
        return;
    }

    if (password.length < 6) {
        showError('Mật khẩu phải có ít nhất 6 ký tự!');
        return;
    }

    // Tạo object dữ liệu
    const loginData = {
        username: username,
        password: password,
        remember: remember
    };

    console.log('Đăng nhập với:', loginData);

    // Giả lập đăng nhập thành công
    showSuccess('Đăng nhập thành công! Đang chuyển hướng...');

    setTimeout(() => {
        // Redirect đến trang chủ hoặc dashboard
        window.location.href = 'index.html';
    }, 1500);

    // KẾT NỐI VỚI ASP.NET API:
    /*
    fetch('/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(loginData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showSuccess('Đăng nhập thành công!');
            localStorage.setItem('token', data.token);
            setTimeout(() => {
                window.location.href = '/Home/Dashboard';
            }, 1500);
        } else {
            showError(data.message || 'Tên đăng nhập hoặc mật khẩu không đúng!');
        }
    })
    .catch(error => {
        showError('Có lỗi xảy ra. Vui lòng thử lại!');
        console.error('Error:', error);
    });
    */
});

function showError(message) {
    const errorDiv = document.getElementById('errorMessage');
    const successDiv = document.getElementById('successMessage');

    successDiv.style.display = 'none';
    errorDiv.textContent = message;
    errorDiv.style.display = 'block';

    setTimeout(() => {
        errorDiv.style.display = 'none';
    }, 5000);
}

function showSuccess(message) {
    const errorDiv = document.getElementById('errorMessage');
    const successDiv = document.getElementById('successMessage');

    errorDiv.style.display = 'none';
    successDiv.textContent = message;
    successDiv.style.display = 'block';
}

// Social login buttons
document.querySelectorAll('.social-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        alert('Tính năng đăng nhập qua mạng xã hội đang được phát triển!');
    });
});

// Toggle password visibility (optional enhancement)
document.getElementById('password').addEventListener('dblclick', function () {
    this.type = this.type === 'password' ? 'text' : 'password';
});