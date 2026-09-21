(function () {
    const input = document.getElementById('test-code-input');
    const lastCodeEl = document.getElementById('test-last-code');
    const debugEl = document.getElementById('test-debug');

    if (input) input.focus();

    const normalizeCode = (value) => {
        return (value || '').replace(/\|9(1|2|3)/g, '\u001D9$1');
    };

    const updateDebug = (value) => {
        if (!debugEl) return;
        const rawValue = value || '';
        const codes = Array.from(rawValue).map(ch => ch.charCodeAt(0));
        const hasGs = codes.includes(29);
        const escaped = rawValue.replace(/\u001d/g, '[GS]');

        const prepared = normalizeCode(rawValue);
        const preparedHasGs = Array.from(prepared).some(ch => ch.charCodeAt(0) === 29);
        const preparedEscaped = prepared.replace(/\u001d/g, '[GS]');

        debugEl.textContent =
            `Raw Len: ${rawValue.length}\n` +
            `Raw Has GS(29): ${hasGs}\n` +
            `Raw Codes: ${codes.join(', ')}\n` +
            `Raw Escaped: ${escaped}\n` +
            `Prepared Has GS(29): ${preparedHasGs}\n` +
            `Prepared Escaped: ${preparedEscaped}`;
    };

    updateDebug('');

    input?.addEventListener('input', function () {
        updateDebug(input.value || '');
    });

    input?.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter') return;
        e.preventDefault();

        const codes = Array.from(input.value).map(ch => ch.charCodeAt(0));
        lastCodeEl.textContent = input.value//codes

        const codeRaw = (input.value || '').trim();
        if (!codeRaw) return;
        const code = normalizeCode(codeRaw);

        //lastCodeEl.textContent = code;
        //input.value = '';

        $.ajax({
            url: '/Test/SendCode',
            type: 'POST',
            //contentType: 'application/json',
            data: { code: code },
            success: function (res) {
                //lastCodeEl.textContent = res?.code || code;
                input.value = '';
                input.focus();
                //updateDebug('');
                //updateDebug(res?.code);
                Toast.fire({ icon: 'success', title: 'Kod alındı.' });
            },
            error: function (xhr) {
                parseErrorResponse(xhr);
            }
        });
    });
})();