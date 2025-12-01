let uploadedFiles = [];

// Khởi tạo ngày mặc định
function initDates() {
    const now = new Date();
    const startDate = now.toISOString().slice(0, 16);
    document.getElementById('startDate').value = startDate;

    // Hạn nộp mặc định là 2 tuần sau
    const dueDate = new Date(now.getTime() + 14 * 24 * 60 * 60 * 1000);
    document.getElementById('dueDate').value = dueDate.toISOString().slice(0, 16);
}

// Xử lý upload files
function handleFiles(event) {
    const files = Array.from(event.target.files);

    files.forEach(file => {
        if (file.size > 50 * 1024 * 1024) {
            alert('❌ File "' + file.name + '" quá lớn! Vui lòng chọn file dưới 50MB');
            return;
        }
        uploadedFiles.push(file);
    });

    displayFiles();
}

// Hiển thị danh sách files
function displayFiles() {
    const fileList = document.getElementById('fileList');
    fileList.innerHTML = '';

    uploadedFiles.forEach((file, index) => {
        const fileDiv = document.createElement('div');
        fileDiv.className = 'file-item';
        fileDiv.innerHTML = `
                            <div class="file-icon">📄</div>
                            <div class="file-info">
                                <div class="file-name">${file.name}</div>
                                <div class="file-size">${(file.size / 1024 / 1024).toFixed(2)} MB</div>
                            </div>
                            <button type="button" class="remove-btn" onclick="removeFile(${index})">×</button>
                        `;
        fileList.appendChild(fileDiv);
    });
}

// Xóa file
function removeFile(index) {
    uploadedFiles.splice(index, 1);
    displayFiles();
}

// Lưu nháp
function saveDraft() {
    const formData = collectFormData();
    localStorage.setItem('assignmentDraft', JSON.stringify(formData));
    alert('💾 Đã lưu nháp thành công!\n\nBạn có thể tiếp tục chỉnh sửa sau.');
}

// Thu thập dữ liệu form
function collectFormData() {
    return {
        title: document.getElementById('title').value,
        subject: document.getElementById('subject').value,
        class: document.getElementById('class').value,
        description: document.getElementById('description').value,
        startDate: document.getElementById('startDate').value,
        dueDate: document.getElementById('dueDate').value,
        maxScore: document.getElementById('maxScore').value,
        submissionLimit: document.getElementById('submissionLimit').value,
        requirements: document.getElementById('requirements').value,
        files: uploadedFiles.map(f => ({ name: f.name, size: f.size })),
        settings: {
            allowLate: document.getElementById('allowLate').checked,
            showScore: document.getElementById('showScore').checked,
            allowResubmit: document.getElementById('allowResubmit').checked,
            sendNotification: document.getElementById('sendNotification').checked,
            requireComment: document.getElementById('requireComment').checked
        },
        createdAt: new Date().toISOString()
    };
}

// Submit form
document.getElementById('assignmentForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const formData = collectFormData();

    // Validate
    if (!formData.title || !formData.subject || !formData.class || !formData.description) {
        alert('⚠️ Vui lòng điền đầy đủ thông tin bắt buộc!');
        return;
    }

    if (new Date(formData.dueDate) <= new Date(formData.startDate)) {
        alert('⚠️ Hạn nộp phải sau ngày giao bài!');
        return;
    }

    console.log('Dữ liệu bài tập:', formData);

    // Giả lập gửi dữ liệu
    alert(`✅ Đã giao bài tập thành công!

        📝 ${formData.title}
        📚 Môn: ${formData.subject}
        👥 Lớp: ${formData.class}
        📅 Hạn nộp: ${new Date(formData.dueDate).toLocaleString('vi-VN')}
        🎯 Điểm: ${formData.maxScore}
        📎 Tài liệu: ${uploadedFiles.length} file

        ${formData.settings.sendNotification ? '📧 Đã gửi thông báo cho sinh viên' : ''}`);

    // Xóa form sau khi gửi thành công
    // document.getElementById('assignmentForm').reset();
    // uploadedFiles = [];
    // displayFiles();
});

// Khôi phục nháp nếu có
window.onload = function () {
    initDates();

    const draft = localStorage.getItem('assignmentDraft');
    if (draft) {
        const confirmed = confirm('📄 Tìm thấy bản nháp đã lưu. Bạn có muốn khôi phục không?');
        if (confirmed) {
            const data = JSON.parse(draft);
            document.getElementById('title').value = data.title || '';
            document.getElementById('subject').value = data.subject || '';
            document.getElementById('class').value = data.class || '';
            document.getElementById('description').value = data.description || '';
            document.getElementById('requirements').value = data.requirements || '';
            // ... khôi phục các trường khác
        }
    }
};