// File upload functionality
(function() {
    'use strict';
    
    const uploadArea = document.getElementById('uploadArea');
    const fileInput = document.getElementById('fileInput');
    const fileList = document.getElementById('fileList');
    const submitBtn = document.getElementById('submitBtn');
    const submitForm = document.getElementById('submitForm');
    
    if (!uploadArea || !fileInput || !fileList || !submitBtn || !submitForm) {
        console.warn('SubmitAssignment.js: Một số phần tử không tìm thấy');
        return;
    }
    
    let selectedFile = null;
    
    // Get hasSubmitted from form data attribute
    const hasSubmitted = submitForm.dataset.hasSubmitted === 'true';
    
    // Click to upload
    uploadArea.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        fileInput.click();
    });
    
    // Prevent default drag behaviors
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        uploadArea.addEventListener(eventName, (e) => {
            e.preventDefault();
            e.stopPropagation();
        });
    });
    
    // Drag and drop
    uploadArea.addEventListener('dragover', (e) => {
        uploadArea.classList.add('dragover');
    });
    
    uploadArea.addEventListener('dragleave', () => {
        uploadArea.classList.remove('dragover');
    });
    
    uploadArea.addEventListener('drop', (e) => {
        uploadArea.classList.remove('dragover');
        
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            // Create a new FileList-like object and assign to input
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(files[0]);
            fileInput.files = dataTransfer.files;
            handleFileSelect(files[0]);
        }
    });
    
    // File input change
    fileInput.addEventListener('change', (e) => {
        if (e.target.files && e.target.files.length > 0) {
            handleFileSelect(e.target.files[0]);
        }
    });
    
    function handleFileSelect(file) {
        // Validate file type
        const allowedExtensions = ['.doc', '.docx', '.xls', '.xlsx', '.pdf', '.zip', '.rar'];
        const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
        
        if (!allowedExtensions.includes(fileExtension)) {
            alert('Định dạng file không được hỗ trợ! Chỉ chấp nhận: Word, Excel, PDF, ZIP, RAR');
            fileInput.value = '';
            return;
        }
        
        // Validate file size (10MB max)
        const maxSize = 10 * 1024 * 1024; // 10MB
        if (file.size > maxSize) {
            alert('File quá lớn! Kích thước tối đa là 10MB');
            fileInput.value = '';
            return;
        }
        
        selectedFile = file;
        displayFile(file);
    }
    
    function displayFile(file) {
        const fileSize = (file.size / 1024 / 1024).toFixed(2); // Convert to MB
        const fileExtension = file.name.split('.').pop().toLowerCase();
        
        let fileIcon = '📄';
        if (['doc', 'docx'].includes(fileExtension)) fileIcon = '📘';
        else if (['xls', 'xlsx'].includes(fileExtension)) fileIcon = '📗';
        else if (fileExtension === 'pdf') fileIcon = '📕';
        else if (['zip', 'rar'].includes(fileExtension)) fileIcon = '📦';
        
        fileList.innerHTML = `
            <div class="file-item">
                <div class="file-info">
                    <div class="file-icon">${fileIcon}</div>
                    <div class="file-details">
                        <div class="file-name">${escapeHtml(file.name)}</div>
                        <div class="file-size">${fileSize} MB</div>
                    </div>
                </div>
                <button type="button" class="remove-file-btn" onclick="removeFile()">
                    🗑️ Xóa
                </button>
            </div>
        `;
    }
    
    // Make removeFile available globally
    window.removeFile = function() {
        selectedFile = null;
        fileInput.value = '';
        fileList.innerHTML = '';
    };
    
    // Helper function to escape HTML
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
    
    // Form validation
    submitForm.addEventListener('submit', (e) => {
        // Check if file is required (not submitted before)
        if (!hasSubmitted) {
            // Check if file input has a file
            if (!fileInput.files || fileInput.files.length === 0) {
                e.preventDefault();
                alert('Vui lòng chọn file để nộp bài!');
                uploadArea.style.borderColor = '#e74c3c';
                setTimeout(() => {
                    uploadArea.style.borderColor = '';
                }, 2000);
                return false;
            }
        }
        
        // Disable submit button to prevent double submission
        submitBtn.disabled = true;
        submitBtn.innerHTML = '⏳ Đang nộp bài...';
    });
})();
