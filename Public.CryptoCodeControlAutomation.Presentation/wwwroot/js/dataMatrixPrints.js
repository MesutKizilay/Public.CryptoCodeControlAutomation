$(function () {
    const summary = $('#datamatrix-summary');

    const previewTemplate = `<div class="dz-preview dz-file-preview">
        <div class="dz-details">
          <div class="dz-thumbnail">
            <img data-dz-thumbnail>
            <span class="dz-nopreview">Ön izleme yok</span>
            <div class="dz-success-mark"></div>
            <div class="dz-error-mark"></div>
            <div class="dz-error-message"><span data-dz-errormessage></span></div>
            <div class="progress">
              <div class="progress-bar progress-bar-primary" role="progressbar" aria-valuemin="0" aria-valuemax="100" data-dz-uploadprogress></div>
            </div>
          </div>
          <div class="dz-filename" data-dz-name></div>
          <div class="dz-size" data-dz-size></div>
        </div>
        </div>`;

    const normalizeLine = (rawLine) => {
        let line = (rawLine || '').trim();

        if (line.charCodeAt(0) === 0xFEFF) {
            line = line.slice(1);
        }

        if (line.startsWith('"') && line.endsWith('"') && line.length >= 2) {
            line = line.slice(1, -1).replace(/""/g, '"');
        }

        return line;
    };

    const readFile = (file) => {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result || '');
            reader.onerror = () => reject(reader.error || new Error('Dosya okunamadı.'));
            reader.readAsText(file);
        });
    };

    const validateRows = (rows) => {
        const gs = '\u001D';
        const invalidRows = [];

        rows.forEach((line, index) => {
            const hasPrefix = /^01\d{14}21/.test(line);
            const has93 = line.includes(gs + '93');
            const has91 = line.includes(gs + '91');
            const has92 = line.includes(gs + '92');
            const hasGroup = has93 || (has91 && has92);

            if (!(hasPrefix && hasGroup)) {
                invalidRows.push(index + 1);
            }
        });

        return invalidRows;
    };

    const getFileName = (xhr) => {
        const disposition = xhr.getResponseHeader('Content-Disposition') || '';
        const utf8Match = disposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (utf8Match?.[1]) {
            return decodeURIComponent(utf8Match[1]);
        }

        const match = disposition.match(/filename="?([^"]+)"?/i);
        return match?.[1] || `datamatrix_${new Date().toISOString().slice(0, 19).replace(/[-:T]/g, '')}.pdf`;
    };

    const downloadBlob = (blob, fileName) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    };

    Dropzone.autoDiscover = false;
    window.dropzoneDataMatrixPrint = new Dropzone('#dropzone-datamatrix-print', {
        previewTemplate: previewTemplate,
        url: '/DataMatrixPrints/GeneratePdf',
        maxFilesize: 1000,
        acceptedFiles: '.csv',
        autoProcessQueue: false,
        addRemoveLinks: true,
        maxFiles: 1,
        dictMaxFilesExceeded: 'En fazla bir dosya yükleyebilirsiniz.',
        dictRemoveFile: 'Dosyayı Sil',
        dictFileTooBig: 'Dosya boyutu belirlenen aralığın (100 MB) dışındadır.',
        dictInvalidFileType: 'Lütfen .csv formatında dosya yükleyiniz.',
        parallelUploads: 1
    });

    dropzoneDataMatrixPrint.on('addedfile', function () {
        summary.text('');
    });

    dropzoneDataMatrixPrint.on('removedfile', function () {
        summary.text('');
    });

    dropzoneDataMatrixPrint.on('error', function (file, message) {
        Toast?.fire({ icon: 'error', title: message });
        this.removeFile(file);
    });

    $('#btnGenerateDataMatrixPdf').on('click', async function (e) {
        e.preventDefault();

        const $btn = $(this);
        if ($btn.data('submitting')) return;

        const file = dropzoneDataMatrixPrint.files[0];
        if (!file) {
            Toast?.fire({ icon: 'warning', title: 'Lütfen CSV dosyası yükleyiniz.' });
            return;
        }

        $btn.data('submitting', true).prop('disabled', true);
        const releaseSubmit = () => $btn.data('submitting', false).prop('disabled', false);

        try {
            const text = await readFile(file);
            const rows = text
                .toString()
                .split(/\r?\n/)
                .map(normalizeLine)
                .filter(line => line !== '');

            if (rows.length === 0) {
                Toast?.fire({ icon: 'error', title: 'Dosyada kod bulunamadı.' });
                summary.text('');
                releaseSubmit();
                return;
            }

            const invalidRows = validateRows(rows);
            if (invalidRows.length > 0) {
                const sample = invalidRows.slice(0, 10).join(', ');
                Toast?.fire({ icon: 'error', title: `Dosya içerisinde ${invalidRows.length} hatalı kod tespit edildi.` });
                summary.html(`<span class="text-danger">Hatalı satırlar: ${sample}${invalidRows.length > 10 ? '...' : ''}</span>`);
                releaseSubmit();
                return;
            }

            summary.html(`<span class="text-success">${rows.length} kod doğrulandı. PDF hazırlanıyor...</span>`);

            const formData = new FormData();
            formData.append('File', file);

            $.ajax({
                url: '/DataMatrixPrints/GeneratePdf',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (blob, status, xhr) {
                    downloadBlob(blob, getFileName(xhr));
                    summary.html(`<span class="text-success">${rows.length} kod için PDF oluşturuldu.</span>`);
                    Toast?.fire({ icon: 'success', title: 'PDF oluşturuldu.' });
                    releaseSubmit();
                },
                error: function () {
                    summary.html('<span class="text-danger">PDF oluşturulurken sorun oluştu.</span>');
                    Toast?.fire({ icon: 'error', title: 'PDF oluşturulurken sorun oluştu.' });
                    releaseSubmit();
                }
            });
        }
        catch {
            Toast?.fire({ icon: 'error', title: 'Dosya okunurken sorun oluştu.' });
            summary.text('');
            releaseSubmit();
        }
    });
});
