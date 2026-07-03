$(function () {
    // SalesOrderItem page script (cleaned)
    const POLL_INTERVAL_MS = 5000;
    const openJobRows = new Set();
    const pollingRows = new Set();
    let pendingOpenId = null;
    let editSapValidatedAt = null;
    let editOriginalStatus = null;
    const dt = $('#salesOrderItemTable').DataTable({
        processing: true,
        serverSide: true,
        //autoWidth: false,
        rowId: function (row) { return `soi-${row.salesOrderItemId}`; },
        ajax: function (dtReq, callback) {
            const pageIndex = Math.trunc(dtReq.start / dtReq.length);
            const pageSize = dtReq.length;
            const searchValue = dtReq.search?.value?.trim() || "";

            const sort = (dtReq.order || []).map(o => ({
                field: dtReq.columns[o.column].data,
                direction: o.dir
            }));

            let dynamicQuery = { sort: sort, filter: null };

            if (searchValue) {
                dynamicQuery.filter = {
                    field: 'SalesOrderNo',
                    operator: 'contains',
                    value: searchValue,
                    logic: 'or',
                    filters: [
                        { field: 'MaterialNo', operator: 'contains', value: searchValue },
                        { field: 'SalesItemNo', operator: 'contains', value: searchValue }
                    ]
                };
            }

            const withDeleted = $('#chkShowDeleted').is(':checked');

            $.ajax({
                url: '/SalesOrderItems/GetList?Index=' + pageIndex + '&Size=' + pageSize + '&withDeleted=' + withDeleted,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(dynamicQuery),
                success: function (res) {
                    const items = res.items ?? [];
                    callback({ draw: dtReq.draw, recordsTotal: res.noOfItem ?? 0, recordsFiltered: res.noOfItem ?? 0, data: items });
                },
                error: function () {
                    callback({ draw: dtReq.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                }
            });
        },

        columns: [
            {
                data: null,
                title: 'Detay',
                orderable: false,
                searchable: false,
                //width: '14px',
                className: 'dt-control text-center',
                render: function () {
                    return '<button class="btn btn-sm btn-outline-primary js-toggle-jobs" title="Detaylar"><i class="bx bx-chevron-down"></i></button>';
                }
            },
            { data: 'salesOrderItemId', title: 'Id', visible: true },
            { data: 'salesOrderNo', title: 'Satış Sipariş No' },
            { data: 'salesItemNo', title: 'Kalem Numarası' },
            { data: 'materialNo', title: 'Mamül No' },
            { data: 'gtin', title: 'GTIN' },
            { data: 'sapPlannedUnitQty', title: 'Birim Miktarı' },
            { data: 'sapCaseQty', title: 'Koli Miktarı' },
            {
                data: 'status',
                title: 'Durum',
                render: function (d, type, row) {
                    switch (d) {
                        case 0:
                            return '<span class="badge bg-success">Aktif</span>';
                        case 1:
                            return '<span class="badge bg-primary">Tamamlandı</span>';
                        case 2:
                            return '<span class="badge bg-danger">İptal edildi</span>';
                        case 3:
                            return '<span class="badge bg-secondary">Pasif</span>';
                        default:
                            return '<span class="badge bg-light text-dark">Bilinmiyor</span>';
                    }
                }
            },
            {
                data: null,
                title: 'İşlemler',
                orderable: false,
                searchable: false,
                width: '140px',
                render: function (row) {
                    return `
                        <div class="dt-actions">
                          <a href="#" class="btn-action js-edit" data-id="${row.salesOrderItemId}" title="Düzenle">
                            <i class="bx bx-edit"></i>
                          </a>
                          <a href="#" class="btn-action js-delete" data-id="${row.salesOrderItemId}" title="Sil">
                            <i class="bx bx-trash"></i>
                          </a>
                        </div>`;
                }
            }
        ],

        dom:
            "<'row mx-2'<'col-md-2'l>" +
            "<'col-md-10 dt-action-buttons d-flex align-items-center justify-content-md-end justify-content-center flex-wrap gap-2 mb-3 mb-md-0'fB>>" +
            "<'row'<'col-12 table-responsive'tr>>" +
            "<'row mx-2 mt-2'<'col-sm-12 col-md-6'i><'col-sm-12 col-md-6'p>>",

        buttons: [
            {
                extend: 'collection',
                text: '<i class="bx bx-upload me-1"></i>Dışa Aktar',
                className: 'btn btn-secondary btn-label-secondary mx-3',
                autoClose: true,
                buttons: [
                    { extend: 'copyHtml5', exportOptions: { columns: [2, 3, 4, 5, 6, 7, 8] } },
                    { extend: 'excelHtml5', title: 'Siparişler', exportOptions: { columns: [2, 3, 4, 5, 6, 7, 8] } },
                    { extend: 'csvHtml5', title: 'Siparişler', exportOptions: { columns: [2, 3, 4, 5, 6, 7, 8] } },
                    { extend: 'pdfHtml5', title: 'Siparişler', exportOptions: { columns: [2, 3, 4, 5, 6, 7, 8] }, orientation: 'landscape' },
                    { extend: 'print', title: 'Siparişler', exportOptions: { columns: [2, 3, 4, 5, 6, 7, 8] } }
                ]
            },
            {
                text: '<i class="bx bx-plus me-0 me-lg-2"></i><span class="d-none d-lg-inline-block">Yeni Sipariş</span>',
                className: 'btn btn-primary',
                attr: {
                    id: 'btnAddLabel',
                    'data-bs-toggle': 'offcanvas',
                    'data-bs-target': '#offcanvasAddLabel'
                }
            }
        ],

        order: [[1, 'desc']],
        lengthMenu: [10, 25, 50, 100],
        language: { url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" }
    });

    dt.on('draw', function () {
        if (pendingOpenId) {
            const row = dt.row(`#soi-${pendingOpenId}`);
            if (row && row.data()) {
                const $tr = $(row.node());
                if (!row.child.isShown()) {
                    row.child('<div class="p-2 text-muted">Yükleniyor...</div>').show();
                    $tr.addClass('shown');
                    openJobRows.add(pendingOpenId);
                    pollingRows.add(pendingOpenId);
                    refreshUploadJobs(pendingOpenId, row);
                }
            }
            pendingOpenId = null;
        }
    });

    dt.on('init', function () {
        const $filter = $('.dataTables_filter');
        $filter.find('label').contents().filter(function () { return this.nodeType === 3; }).remove();
        $filter.find('input').attr('placeholder', 'S. Order No, M. No...').addClass('form-control me-2');
    });

    dt.on('init', function () {
        const $filter = $('#salesOrderItemTable_filter');

        if ($filter.find('#chkShowDeleted').length === 0) {
            const $wrap = $(`
            <div class="d-flex align-items-center me-2">
                <div class="form-check mb-0">
                    <input class="form-check-input" type="checkbox" id="chkShowDeleted">
                    <label class="form-check-label" for="chkShowDeleted">Silinenleri Getir</label>
                </div>
            </div>
            `);

            $wrap.insertBefore($filter);
        }
    });

    $(document).on('change', '#chkShowDeleted', function () {
        dt.ajax.reload(null, true);
    });

    $('#add-shelfLifeUnit').select2({
        placeholder: 'Birim seçiniz',
        allowClear: true,
        width: '100%',
        dropdownParent: $('#offcanvasAddLabel')
    });

    $('#edit-shelfLifeUnit').select2({
        placeholder: 'Birim seçiniz',
        allowClear: true,
        width: '100%',
        dropdownParent: $('#offcanvasEditLabel')
    });

    $('#edit-status').select2({
        minimumResultsForSearch: Infinity,
        width: '100%',
        dropdownParent: $('#offcanvasEditLabel')
    });

    // ============================
    // Helpers
    // ============================
    function clearAddSapFields() {
        $('#add-materialNo').val('');
        $('#add-gtin').val('');
        $('#add-plannedUnitQty').val('');
        $('#add-caseQty').val('');
        $('#add-shelfLifeValue').val('');
        $('#add-shelfLifeUnit').val('').trigger('change');
        $('#add-message').val('');
    }

    function clearEditSapFields() {
        $('#edit-materialNo').val('');
        $('#edit-gtin').val('');
        $('#edit-plannedUnitQty').val('');
        $('#edit-caseQty').val('');
        $('#edit-shelfLifeValue').val('');
        $('#edit-shelfLifeUnit').val('').trigger('change');
        // edit modalÄ±nda message input yoksa sorun deÄŸil; varsa temizlemek istersen aÃ§:
        // $('#edit-message').val('');
    }

    function statusBadge(status) {
        switch (status) {
            case 0:
                return '<span class="badge bg-label-primary">Yeni</span>';
            case 1:
                return '<span class="badge bg-label-warning">İçe Aktarılıyor</span>';
            case 2:
                return '<span class="badge bg-label-success">Tamamlandı</span>';
            case 3:
                return '<span class="badge bg-label-danger">Başarısız</span>';
            case 4:
                return '<span class="badge bg-label-danger">Silindi</span>';
            default:
                return '<span class="badge bg-label-secondary">Bilinmiyor</span>';
        }
    }

    function formatUploadJobs(jobs) {
        if (!jobs || jobs.length === 0) {
            return '<div class="card shadow-none border mb-0"><div class="card-body py-3 text-muted">Yüklenmiş iş emri dosyası bulunamadı.</div></div>';
        }

        const rows = jobs.map(j => `
            <tr>
                <td>${j.uploadJobId}</td>
                <td class="text-truncate" style="max-width: 220px;" title="${j.fileName ?? ''}">${j.fileName ?? ''}</td>
                <td>${statusBadge(j.status)}</td>
                <td>${j.totalRows ?? ''}</td>
                <td>${j.insertedRows ?? ''}</td>
                <td>${j.startedAt ? new Date(j.startedAt).toLocaleString('tr-TR') : ''}</td>
                <td>${j.finishedAt ? new Date(j.finishedAt).toLocaleString('tr-TR') : ''}</td>
                <td class="text-truncate" style="max-width: 260px;">${j.errorText ?? ''}</td>
            </tr>`).join('');

        return `
            <div class="card shadow-none border mb-0">                
                <div class="table-responsive text-nowrap">
                    <table class="table table-sm table-striped table-hover mb-0 uploadjob-table">
                        <thead class="table-light">
                            <tr>
                                <th>Id</th>
                                <th>Dosya Adı</th>
                                <th>Durum</th>
                                <th>Toplam</th>
                                <th>Kayıt Edilen</th>
                                <th>Başlama</th>
                                <th>Bitiş</th>
                                <th>Hata</th>
                            </tr>
                        </thead>
                        <tbody class="table-border-bottom-0">${rows}</tbody>
                    </table>
                </div>
            </div>`;
    }
    function fetchUploadJobs(salesOrderItemId) {
        return $.ajax({
            url: '/UploadJobs/GetBySalesOrderItemId',
            type: 'GET',
            data: { id: salesOrderItemId }
        });
    }

    function refreshUploadJobs(salesOrderItemId, row) {
        return fetchUploadJobs(salesOrderItemId)
            .done(function (jobs) {
                if (!row.child.isShown()) {
                    return;
                }
                row.child(formatUploadJobs(jobs)).show();
            })
            .fail(function () {
                if (!row.child.isShown()) {
                    return;
                }
                row.child('<div class="p-2 text-danger">Yüklenen iş emri bilgisine ulaşılamadı.</div>').show();
            });
    }

    // ============================
    // Add - prepare form
    // ============================
    async function loadNextSalesOrderNo() {
        $('#salesOrderNo').val('');
        $('#salesItemNo').val('');

        try {
            const res = await $.get('/SalesOrderItems/GetNextSalesOrderNo');
            if (res?.salesOrderNo) {
                $('#salesOrderNo').val(res.salesOrderNo);
            }
        }
        catch (err) {
            $('#salesOrderNo').val('00000001');
        }

        $('#salesItemNo').val('1');
    }

    $(document).on('click', '#btnAddLabel, #btnAddSalesOrderItem', async function () {
        clearAddSapFields();

        if (dropzoneAdd) dropzoneAdd.removeAllFiles();
        await loadNextSalesOrderNo();
    });

    // Add submit
    $('#btnSaveSalesOrderItem').on('click', async function (e) {
        e.preventDefault();
        const $btn = $(this);
        if ($btn.data('submitting')) return;
        $btn.data('submitting', true).prop('disabled', true);
        const releaseSubmit = () => $btn.data('submitting', false).prop('disabled', false);

        const plannedQtyRaw = $('#add-plannedUnitQty').val();
        const plannedQty = plannedQtyRaw ? Number(plannedQtyRaw) : 0;

        if (dropzoneAdd && dropzoneAdd.files.length > 0 && plannedQty > 0) {
            const file = dropzoneAdd.files[0];
            try {
                const text = await new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onload = () => resolve(reader.result || "");
                    reader.onerror = () => reject(reader.error || new Error("Dosya okunamadi."));
                    reader.readAsText(file);
                });

                const rows = text
                    .toString()
                    .split(/\r?\n/)
                    .filter(l => l.trim() !== "");

                const gs = '\u001D';
                let invalidCount = 0;
                for (const rawLine of rows) {
                    let line = rawLine.trim();
                    if (line.charCodeAt(0) === 0xFEFF) {
                        line = line.slice(1);
                    }
                    if (line.startsWith('"') && line.endsWith('"') && line.length >= 2) {
                        line = line.slice(1, -1).replace(/""/g, '"');
                    }
                    // line = line.replace(/\\u001d/gi, gs).replace(/\\x1d/gi, gs);

                    const hasPrefix = /^01\d{14}21/.test(line);
                    const has93 = line.includes(gs + "93");
                    const has91 = line.includes(gs + "91");
                    const has92 = line.includes(gs + "92");
                    const hasGroup = has93 || (has91 && has92);

                    if (!(hasPrefix && hasGroup)) {
                        invalidCount++;
                    }
                }

                if (invalidCount > 0) {
                    Toast?.fire({ icon: 'error', title: `Dosya içerisinde ${invalidCount} hatalı kod tespit edildi.` });
                    releaseSubmit();
                    return;
                }

                const lineCount = rows.length;
                const expected = Math.ceil(plannedQty * 1.05);

                if (lineCount < expected) {
                    Toast?.fire({
                        icon: 'error',
                        title: `Dosyadaki kod adedi yetersiz. Planlanan: ${plannedQty} | Dosya: ${lineCount} | Beklenen: ${expected}`
                    });

                    releaseSubmit();
                    return;
                }

                if (lineCount !== expected) {
                    const result = await Swal.fire({
                        title: 'Dosyadaki kod adedi fire miktarından(%5) fazladır!',
                        text: `Planlanan: ${plannedQty} | Dosya: ${lineCount} | Beklenen: ${expected}. Yüklemeye devam edilsin mi?`,
                        icon: 'warning',
                        showCancelButton: true,
                        confirmButtonText: 'Devam',
                        cancelButtonText: 'Vazgeç'
                    });

                    if (!result.isConfirmed) {
                        releaseSubmit();
                        return;
                    }
                }
            }
            catch (err) {
                Toast?.fire({ icon: 'error', title: 'Dosya okunurken sorun oluştu. Lütfen işlem yapmak istediğiniz dosyanın bilgisayarınızda kapalı olduğundan emin olunuz.' });
                dropzoneAdd.removeAllFiles();
                releaseSubmit();
                return;
            }
        }

        const formData = new FormData();
        formData.append('salesOrderNo', $('#salesOrderNo').val()?.trim() || '');
        formData.append('salesItemNo', $('#salesItemNo').val()?.trim() || '');
        formData.append('materialNo', $('#add-materialNo').val()?.trim() || '');
        formData.append('gtin', $('#add-gtin').val()?.trim() || '');
        formData.append('sapPlannedUnitQty', $('#add-plannedUnitQty').val() || 0);
        formData.append('sapCaseQty', $('#add-caseQty').val() || null);
        formData.append('shelfLifeValue', $('#add-shelfLifeValue').val() ?? '');
        formData.append('shelfLifeUnit', $('#add-shelfLifeUnit').val() ?? '');
        //formData.append('sapValidatedAt', new Date().toISOString());

        if (dropzoneAdd && dropzoneAdd.files.length > 0) {
            formData.append('File', dropzoneAdd.files[0]);
        }

        $.ajax({
            url: '/SalesOrderItems/Create',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (res) {
                bootstrap.Offcanvas.getOrCreateInstance('#offcanvasAddLabel').hide();

                $('#salesOrderNo').val('');
                $('#salesItemNo').val('');
                clearAddSapFields();

                if (dropzoneAdd) dropzoneAdd.removeAllFiles();
                if (res?.salesOrderItemId) {
                    pendingOpenId = res.salesOrderItemId;
                }
                dt.ajax.reload(null, false);
                Toast?.fire({ icon: 'success', title: 'Kod kayıt işlemine başlanıldı.' });
                releaseSubmit();
            },
            error: function (xhr) { parseErrorResponse?.(xhr); releaseSubmit(); }
        });
    });

    // ============================
    // Edit open
    // ============================
    $('#salesOrderItemTable').on('click', '.js-edit', function (e) {
        e.preventDefault();
        const id = $(this).data('id');

        $.get('/SalesOrderItems/GetById', { id: id })
            .done(function (res) {
                $('#edit-id').val(res.salesOrderItemId);
                $('#edit-salesOrderNo').val(res.salesOrderNo);
                $('#edit-salesItemNo').val(res.salesItemNo);
                $('#edit-materialNo').val(res.materialNo);
                $('#edit-gtin').val(res.gtin);
                $('#edit-plannedUnitQty').val(res.sapPlannedUnitQty);
                $('#edit-caseQty').val(res.sapCaseQty);
                $('#edit-shelfLifeValue').val(res.shelfLifeValue);
                $('#edit-shelfLifeUnit').val(res.shelfLifeUnit?.toString() ?? '').trigger('change');
                editOriginalStatus = Number(res.status);
                const statusValue = res.status?.toString() ?? '3';
                const statusIsSelectable = ['0', '1', '3'].includes(statusValue);
                $('#edit-status')
                    .prop('disabled', !statusIsSelectable)
                    .val(statusIsSelectable ? statusValue : '3')
                    .trigger('change');
                editSapValidatedAt = res.sapValidatedAt || new Date().toISOString();

                bootstrap.Offcanvas.getOrCreateInstance('#offcanvasEditLabel').show();
            })
            .fail(function (xhr) { parseErrorResponse(xhr); });
    });

    // Edit submit
    $('#btnUpdateSalesOrderItem').on('click', async function (e) {
        e.preventDefault();
        const $btn = $(this);
        if ($btn.data('submitting')) return;
        $btn.data('submitting', true).prop('disabled', true);
        const releaseSubmit = () => $btn.data('submitting', false).prop('disabled', false);

        const salesOrderItemId = parseInt($('#edit-id').val(), 10);
        const selectedStatusValue = $('#edit-status').prop('disabled') ? null : $('#edit-status').val();
        const selectedStatus = selectedStatusValue === null || selectedStatusValue === ''
            ? editOriginalStatus
            : Number(selectedStatusValue);

        const payload = {
            salesOrderItemId: salesOrderItemId,
            salesOrderNo: $('#edit-salesOrderNo').val()?.trim(),
            salesItemNo: $('#edit-salesItemNo').val()?.trim(),
            materialNo: $('#edit-materialNo').val()?.trim(),
            gtin: $('#edit-gtin').val()?.trim(),
            sapPlannedUnitQty: $('#edit-plannedUnitQty').val(),
            sapCaseQty: $('#edit-caseQty').val(),
            shelfLifeValue: $('#edit-shelfLifeValue').val(),
            shelfLifeUnit: $('#edit-shelfLifeUnit').val(),
            sapValidatedAt: editSapValidatedAt
        };

        try {
            await $.ajax({
                url: '/SalesOrderItems/Update',
                type: 'POST',
                data: payload
            });

            if (Number.isFinite(selectedStatus) && selectedStatus !== editOriginalStatus) {
                await $.ajax({
                    url: '/SalesOrderItems/UpdateStatus',
                    type: 'POST',
                    data: {
                        salesOrderItemId: salesOrderItemId,
                        status: selectedStatus
                    }
                });
            }

            bootstrap.Offcanvas.getOrCreateInstance('#offcanvasEditLabel').hide();
            dt.ajax.reload(null, false);
            Toast?.fire({ icon: 'success', title: 'Kayıt güncellendi.' });
        }
        catch (xhr) {
            parseErrorResponse?.(xhr);
        }
        finally {
            releaseSubmit();
        }
    });

    // ============================
    // Delete
    // ============================
    $('#salesOrderItemTable').on('click', '.js-delete', function (e) {
        e.preventDefault();
        const id = $(this).data('id');

        Swal.fire({
            title: 'Kaydı silmek istediğinizden emin misiniz?',
            text: 'Bu işlem birkaç dakika sürebilir, lütfen bekleyiniz.',
            icon: 'warning',
            showCancelButton: true,
            cancelButtonText: 'Vazgeç',
            confirmButtonText: 'Sil',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            showLoaderOnConfirm: true,
            backdrop: true,
            allowOutsideClick: () => !Swal.isLoading(),
            preConfirm: async () => {
                try {
                    await $.ajax({
                        url: '/SalesOrderItems/Delete',
                        type: 'POST',
                        data: { id: id }
                    });
                    return true;
                }
                catch (xhr) {
                    parseErrorResponse?.(xhr);
                    return false;
                }
            }
        }).then((r) => {
            if (!r.isConfirmed) return;
            dt.ajax.reload(null, false);
            Toast?.fire({ icon: 'success', title: 'Kayıt başarıyla silindi.' });
        });
    });

    // ============================
    // UploadJobs child rows
    // ============================
    $('#salesOrderItemTable tbody').on('click', 'td.dt-control', function (e) {
        e.preventDefault();
        const tr = $(this).closest('tr');
        const row = dt.row(tr);
        const data = row.data();
        if (!data) return;

        const salesOrderItemId = data.salesOrderItemId;
        if (row.child.isShown()) {
            row.child.hide();
            tr.removeClass('shown');
            openJobRows.delete(salesOrderItemId);
            pollingRows.delete(salesOrderItemId);
        } else {
            row.child('<div class="p-2 text-muted">Yükleniyor...</div>').show();
            tr.addClass('shown');
            openJobRows.add(salesOrderItemId);
            pollingRows.add(salesOrderItemId);
            refreshUploadJobs(salesOrderItemId, row);
        }
    });

    setInterval(function () {
        if (pollingRows.size === 0) return;
        pollingRows.forEach(function (salesOrderItemId) {
            const row = dt.row(`#soi-${salesOrderItemId}`);
            if (!row || !row.data() || !row.child.isShown()) {
                pollingRows.delete(salesOrderItemId);
                return;
            }

            refreshUploadJobs(salesOrderItemId, row);
        });
    }, POLL_INTERVAL_MS);

    // ============================
    // Dropzone template
    // ============================
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

    // Dropzone initialization for Add modal
    window.dropzoneAdd = null;
    if (document.getElementById('dropzone-add-order')) {
        Dropzone.autoDiscover = false;
        dropzoneAdd = new Dropzone('#dropzone-add-order', {
            previewTemplate: previewTemplate,
            url: '/SalesOrderItems/Upload',
            maxFilesize: 1000, // MB
            acceptedFiles: '.csv',
            autoProcessQueue: false,
            addRemoveLinks: true,
            maxFiles: 1,
            //accept: function (file, done) {
            //    const name = (file.name || '').toLowerCase();
            //    if (name.endsWith('.csv')) {
            //        done();
            //        return;
            //    }

            //    done('Sadece .csv dosyaları kabul edilir.');
            //    this.removeFile(file);
            //    Toast?.fire({ icon: 'error', title: 'Sadece .csv dosyaları kabul edilir.' });
            //},
            dictMaxFilesExceeded: "En fazla bir dosya yükleyebilirsiniz.",
            dictRemoveFile: 'Dosyayı Sil',
            dictFileTooBig: `Dosya boyutu belirlenen aralığın (100 MB) dışındadır.`,
            dictInvalidFileType: 'Lütfen .csv formatında dosya yükleyiniz.',
            parallelUploads: 1
        });
    }

    dropzoneAdd.on('error', function (file, message) {
        //const name = (file?.name || '').toLowerCase();
        //const isCsv = name.endsWith('.csv');
        //const maxBytes = (this.options?.maxFilesize || 10) * 1024 * 1024;
        //const isTooBig = file?.size && file.size > maxBytes;

        Toast?.fire({ icon: 'error', title: message/*'Lütfen .csv formatında dosya yükleyiniz.'*/ });

        //if (!isCsv) {
        //    Toast?.fire({ icon: 'error', title: 'Lütfen .csv formatında dosya yükleyiniz.' });
        //}
        //else if (isTooBig) {//(maks ${(this.options?.maxFilesize || 10)}MB)
        //    Toast?.fire({ icon: 'error', title: `Dosya boyutu belirlenen aralığın (${(this.options?.maxFilesize)}MB) dışındadır.` });
        //}
        //else {
        //    Toast?.fire({ icon: 'error', title: 'Dosya yükleme hatası.' });
        //}

        this.removeFile(file);
    });

    // Dropzone initialization for Edit modal
    window.dropzoneEdit = null;
    if (document.getElementById('dropzone-edit-order')) {
        Dropzone.autoDiscover = false;
        dropzoneEdit = new Dropzone('#dropzone-edit-order', {
            previewTemplate: previewTemplate,
            url: '/SalesOrderItems/Upload',
            maxFilesize: 200, // MB
            acceptedFiles: '.csv',
            autoProcessQueue: false,
            addRemoveLinks: true,
            maxFiles: 1,
            //accept: function (file, done) {
            //    const name = (file.name || '').toLowerCase();
            //    if (name.endsWith('.csv')) {
            //        done();
            //        return;
            //    }

            //    done('Sadece .csv dosyaları kabul edilir.');
            //    this.removeFile(file);
            //    Toast?.fire({ icon: 'error', title: 'Sadece .csv dosyaları kabul edilir.' });
            //},
            dictRemoveFile: 'Dosyayı Sil',
            dictFileTooBig: 'Dosya çok büyük (maks 10MB)',
            dictInvalidFileType: 'Geçersiz dosya türü',
            parallelUploads: 1
        });
    }
});
