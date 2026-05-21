(function () {
    document.addEventListener('DOMContentLoaded', function () {
        const paymentMethodSelect = document.getElementById('paymentMethodSelect');
        const transactionNumberField = document.getElementById('transactionNumberField');
        const transactionNumberInput = document.getElementById('transactionNumberInput');

        if (!paymentMethodSelect || !transactionNumberField || !transactionNumberInput) {
            return;
        }

        function syncTransactionNumberField() {
            const isCash = paymentMethodSelect.value === 'Cash';

            transactionNumberField.style.display = isCash ? 'none' : '';

            if (isCash) {
                transactionNumberInput.value = '';
            }
        }

        paymentMethodSelect.addEventListener('change', syncTransactionNumberField);

        syncTransactionNumberField();
    });
})();