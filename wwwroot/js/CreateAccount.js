/* ── Avatar preview ── */
function previewFile(e) {
    const file = e.target.files[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) { alert('File must be under 2 MB'); e.target.value = ''; return; }
    if (!file.type.match('image.*')) { alert('Please select an image file'); e.target.value = ''; return; }
    const r = new FileReader();
    r.onload = ev => {
        const img = document.getElementById('previewImage');
        img.src = ev.target.result;
        img.style.animation = 'ca-fade .4s ease';
    };
    r.readAsDataURL(file);
}

/* ── Toggle password visibility ── */
function togglePwd(inputId, iconId) {
    const inp  = document.getElementById(inputId);
    const icon = document.getElementById(iconId);
    const show = inp.type === 'password';
    inp.type = show ? 'text' : 'password';
    icon.className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
}

/* ── Password strength ── */
document.addEventListener('DOMContentLoaded', function () {
    const pwd = document.getElementById('passwordInput');
    if (pwd) pwd.addEventListener('input', checkStrength);

    // Native validation feedback
    document.querySelectorAll('.needs-validation').forEach(form => {
        form.addEventListener('submit', e => {
            if (!form.checkValidity()) { e.preventDefault(); e.stopPropagation(); }
            form.classList.add('was-validated');
        }, false);
    });
});

function checkStrength() {
    const val      = document.getElementById('passwordInput').value;
    const hasLen   = val.length >= 8;
    const hasUpper = /[A-Z]/.test(val);
    const hasLower = /[a-z]/.test(val);
    const hasNum   = /[0-9]/.test(val);
    const hasSpc   = /[!@#$%^&*(),.?":{}|<>]/.test(val);

    setCheck('chk-len',     hasLen);
    setCheck('chk-upper',   hasUpper);
    setCheck('chk-lower',   hasLower);
    setCheck('chk-num',     hasNum);
    setCheck('chk-special', hasSpc);

    const score = [hasLen, hasUpper, hasLower, hasNum, hasSpc].filter(Boolean).length;
    const bar   = document.getElementById('strengthBar');
    const badge = document.getElementById('strengthBadge');

    badge.className = 'ca-strength-badge';

    if (!val.length) {
        bar.style.width = '0%'; bar.style.background = 'var(--text-light)';
        badge.textContent = 'Not entered';
    } else if (score <= 2) {
        bar.style.width = '20%'; bar.style.background = 'var(--danger)';
        badge.textContent = 'Weak'; badge.classList.add('weak');
    } else if (score === 3) {
        bar.style.width = '50%'; bar.style.background = 'var(--warning)';
        badge.textContent = 'Fair'; badge.classList.add('fair');
    } else if (score === 4) {
        bar.style.width = '75%'; bar.style.background = 'var(--accent)';
        badge.textContent = 'Good'; badge.classList.add('good');
    } else {
        bar.style.width = '100%'; bar.style.background = 'var(--success)';
        badge.textContent = 'Strong'; badge.classList.add('strong');
    }
}

function setCheck(id, met) {
    const el = document.getElementById(id);
    el.classList.toggle('met', met);
    el.querySelector('i').className = met ? 'bi bi-check-circle-fill' : 'bi bi-circle';
}

/* ── Fade animation ── */
const s = document.createElement('style');
s.textContent = '@@keyframes ca-fade { from { opacity:.5; transform:scale(.95); } to { opacity:1; transform:scale(1); } }';
document.head.appendChild(s);