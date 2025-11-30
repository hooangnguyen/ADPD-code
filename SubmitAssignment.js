/* PSEUDOCODE:
 - Goal: Remove Razor inline syntax from static .js file which caused TS errors.
 - Expect Razor page to set submission flag in one of two ways before this script runs:
    1) Set a global variable `window.hasSubmitted = true|false` in the Razor page.
    2) Or set a data attribute on the form element: `<form id="submitForm" data-has-submitted="true">`.
 - Implementation:
    - At runtime, read the flag from `window.hasSubmitted` if defined.
    - If not defined, read `document.getElementById('submitForm').dataset.hasSubmitted`.
    - Convert the value to a boolean safely (handle strings "true"/"false" and actual booleans).
    - On form submit:
        - If the user has not previously submitted and no file is selected, prevent submission and alert.
        - Otherwise disable the submit button and change its label to an uploading state.
    - Keep existing drag/drop and file handling unchanged.
 - Notes:
    - Do NOT use Razor inline `@(...)` inside this static file.
    - This file should be loaded after the Razor page sets `window.hasSubmitted` or the form's data attribute.
*/

 // File upload functionality
const uploadArea = document.getElementById('uploadArea');
const fileInput = document.getElementById('fileInput');
const fileList = document.getElementById('fileList');
const submitBtn = document.getElementById('submitBtn');
let selectedFile = null;

// Click to upload
if (uploadArea && fileInput) {
    uploadArea.addEventListener('click', () => {
        fileInput.click();
    });
}

// Drag and drop
if (uploadArea) {
    uploadArea.addEventListener('dragover', (e) => {
        e.preventDefault();
        uploadArea.classList.add('dragover');
    });

    uploadArea.addEventListener('dragleave', () => {
        uploadArea.classList.remove('dragover');
    });

    uploadArea.addEventListener('drop', (e) => {
        e.preventDefault();
        uploadArea.classList.remove('dragover');

        const files = e.dataTransfer.files;
        if (files.length > 0) {
            // assign files to input for form submission
            if (fileInput) fileInput.files = files;
            handleFileSelect(files[0]);
        }
    });
}

// File input change
if (fileInput) {
    fileInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
            handleFileSelect(e.target.files[0]);
        }
    });
}

function handleFileSelect(file) {
    selectedFile = file;
    displayFile(file);
}

function displayFile(file) {
    if (!fileList) return;
    const fileSize = (file.size / 1024).toFixed(2);
    const fileExtension = file.name.split('.').pop().toLowerCase();

    let fileIcon = '??';
    if (['doc', 'docx'].includes(fileExtension)) fileIcon = '??';
    else if (['xls', 'xlsx'].includes(fileExtension)) fileIcon = '??';
    else if (fileExtension === 'pdf') fileIcon = '??';
    else if (['zip', 'rar'].includes(fileExtension)) fileIcon = '??';

    fileList.innerHTML = `
            <div class="file-item">
                <div class="file-info">
                    <div class="file-icon">${fileIcon}</div>
                    <div class="file-details">
                        <div class="file-name">${file.name}</div>
                        <div class="file-size">${fileSize} KB</div>
                    </div>
                </div>
                <button type="button" class="remove-file-btn" onclick="removeFile()">
                    ??? Xóa
                </button>
            </div>
        `;
}

function removeFile() {
    selectedFile = null;
    if (fileInput) fileInput.value = '';
    if (fileList) fileList.innerHTML = '';
}

// Helper to coerce various representations to boolean
function toBoolean(val) {
    if (typeof val === 'boolean') return val;
    if (typeof val === 'string') return val.toLowerCase() === 'true';
    return Boolean(val);
}

// Form validation
(function setupFormValidation() {
    const formElem = document.getElementById('submitForm');
    if (!formElem) return;

    formElem.addEventListener('submit', (e) => {
        // Read flag from window.hasSubmitted if present, otherwise fallback to data-has-submitted attribute on the form.
        const globalFlag = (typeof window.hasSubmitted !== 'undefined') ? window.hasSubmitted : undefined;
        const dataAttr = formElem.dataset ? formElem.dataset.hasSubmitted : undefined;

        const hasExistingSubmission = (typeof globalFlag !== 'undefined')
            ? toBoolean(globalFlag)
            : (typeof dataAttr !== 'undefined' ? toBoolean(dataAttr) : false);

        if (!hasExistingSubmission && !selectedFile) {
            e.preventDefault();
            alert('Vui lòng ch?n file ?? n?p bài!');
            return false;
        }

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '? ?ang n?p bài...';
        }
    });
})();