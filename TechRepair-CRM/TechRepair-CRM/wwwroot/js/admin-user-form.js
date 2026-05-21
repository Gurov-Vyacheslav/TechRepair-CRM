(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const technicianSelect = document.getElementById('technicianSelect');
        const emailInput = document.getElementById('emailInput');
        const emailHint = document.getElementById('emailHint');

        if (!technicianSelect || !emailInput || !emailHint) {
            return;
        }

        function syncTechnicianEmail() {
            const selectedOption = technicianSelect.options[technicianSelect.selectedIndex];
            const technicianEmail = selectedOption?.dataset.email || '';

            if (technicianSelect.value) {
                emailInput.value = technicianEmail;
                emailInput.readOnly = true;
                emailHint.textContent = 'Email заблокирован, потому что берётся из карточки выбранного мастера.';
            } else {
                emailInput.readOnly = false;
                emailHint.textContent = 'Если выбран мастер, email будет взят из его карточки.';
            }
        }

        technicianSelect.addEventListener('change', syncTechnicianEmail);

        syncTechnicianEmail();
    });
})();