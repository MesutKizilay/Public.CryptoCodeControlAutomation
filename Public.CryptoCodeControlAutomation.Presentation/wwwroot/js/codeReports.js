$(function () {
    const codeInput = $("#code-report-code");
    const salesSelect = $("#code-report-salesorder");
    const plannedSelect = $("#code-report-plannedorder");
    const statusSelect = $("#code-report-status");
    const filterButton = $("#code-report-filter-btn");
    const exportButton = $("#code-report-export-btn");
    const onlyCodesCheckbox = $("#code-report-only-codes");
    const shiftDateInput = document.querySelector("#flatpickr-range");

    let plannedItems = [];
    let selectedShiftDates = [];

    const statusLabels = {
        0: "Avaılable",
        1: "Allocated",
        2: "Produced",
        3: "Reject",
        4: "Scrap",
        5: "Voıd"
    };

    const statusBadges = {
        0: "badge bg-info",
        1: "badge bg-primary",
        2: "badge bg-success",
        3: "badge bg-warning",
        4: "badge bg-danger",
        5: "badge bg-dark"
    };

    const initSelect2 = () => {
        salesSelect.select2({ placeholder: "Satış Siparişi", width: "100%", allowClear: true });
        plannedSelect.select2({ placeholder: "Planlı Sipariş", width: "100%", allowClear: true });
        statusSelect.select2({ placeholder: "Kod Durumu", width: "100%", allowClear: true });
    };

    const initShiftDatePicker = () => {
        if (!shiftDateInput || typeof flatpickr !== "function") return;

        flatpickr(shiftDateInput, {
            mode: "range",
            dateFormat: "d-m-Y",
            //dateFormat: "Y-m-d",
            monthSelectorType: "static",
            conjunction: " to ",
            onChange: function (selectedDates) {
                selectedShiftDates = selectedDates.slice(0, 2);
            },
            onClose: function (selectedDates, _dateStr, instance) {
                selectedShiftDates = selectedDates.slice(0, 2);

                if (selectedDates.length === 1) {
                    const formattedDate = instance.formatDate(selectedDates[0], instance.config.dateFormat);

                    window.setTimeout(function () {
                        const displayInput = instance.altInput ?? instance.input;
                        displayInput.value = formattedDate;
                    }, 0);
                }
            }
        });
    };

    const formatDateForRequest = (date) => {
        if (!(date instanceof Date) || Number.isNaN(date.getTime())) return null;

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
        //return `${day}-${month}-${year}`;
    };

    const getShiftDateFilter = () => {
        if (selectedShiftDates.length === 0) {
            return { shiftDateStart: null, shiftDateEnd: null };
        }

        const startDate = selectedShiftDates[0];
        const endDate = selectedShiftDates[1] ?? selectedShiftDates[0];

        return {
            shiftDateStart: formatDateForRequest(startDate),
            shiftDateEnd: formatDateForRequest(endDate)
        };
    };

    const getCodeValueFilter = () => (codeInput.val() ?? "").toString().trim();

    const clearSelect = ($select) => {
        $select.empty();
        $select.append("<option></option>");
        $select.trigger("change");
    };

    const loadSalesOrders = () => {
        $.ajax({
            url: "/SalesOrderItems/GetList",
            type: "GET",
            success: function (items) {
                clearSelect(salesSelect);
                const data = items ?? [];
                data.forEach((item) => {
                    const id = item?.salesOrderItemId;
                    const text = `${item?.salesOrderNo ?? ""} / ${item?.salesItemNo ?? ""}`.trim();
                    if (id) {
                        salesSelect.append(`<option value="${id}">${text}</option>`);
                    }
                });
                salesSelect.trigger("change");
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    const isValidId = (value) => Number.isFinite(value) && value > 0;

    const fillPlannedOrders = (salesOrderItemId) => {
        clearSelect(plannedSelect);
        const filteredItems = isValidId(salesOrderItemId)
            ? (plannedItems ?? []).filter((item) => Number(item?.salesOrderItemId) === salesOrderItemId)
            : (plannedItems ?? []);

        filteredItems.forEach((item) => {
            const id = item?.plannedOrderId;
            const text = item?.plannedOrderNo ?? "";
            if (id) {
                plannedSelect.append(
                    `<option value="${id}" data-sales-order-item-id="${item?.salesOrderItemId ?? ""}">${text}</option>`
                );
            }
        });
        plannedSelect.trigger("change");
    };

    const loadPlannedOrders = () => {
        $.ajax({
            url: "/Dashboard/GetPlannedOrdersBySalesOrderItemId",
            type: "GET",
            success: function (items) {
                plannedItems = items ?? [];
                const selectedSalesOrderItemId = Number(salesSelect.val());
                fillPlannedOrders(selectedSalesOrderItemId);
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    initSelect2();
    initShiftDatePicker();
    loadSalesOrders();
    clearSelect(plannedSelect);
    loadPlannedOrders();

    salesSelect.on("change", function () {
        const salesOrderItemId = Number($(this).val());
        fillPlannedOrders(salesOrderItemId);
    });

    const dt = $("#code-report-table").DataTable({
        processing: true,
        serverSide: true,
        searching: false,
        deferLoading: 0,
        autoWidth: false,
        scrollX: true,
        scrollCollapse: true,
        ajax: function (dtReq, callback) {
            const pageIndex = Math.trunc(dtReq.start / dtReq.length);
            const pageSize = dtReq.length;

            const salesOrderItemId = Number(salesSelect.val());
            const plannedOrderId = Number(plannedSelect.val());
            const statusValue = statusSelect.val();
            const shiftDateFilter = getShiftDateFilter();
            const codeValue = getCodeValueFilter();

            const payload = {
                codeValue: codeValue || null,
                salesOrderItemId: Number.isFinite(salesOrderItemId) ? salesOrderItemId : null,
                plannedOrderId: Number.isFinite(plannedOrderId) ? plannedOrderId : null,
                status: statusValue !== "" && statusValue !== null ? Number(statusValue) : null,
                shiftDateStart: shiftDateFilter.shiftDateStart,
                shiftDateEnd: shiftDateFilter.shiftDateEnd
            };

            $.ajax({
                url: `/Codes/GetCodeReportList?Index=${pageIndex}&Size=${pageSize}`,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    callback({
                        draw: dtReq.draw,
                        recordsTotal: res.noOfItem ?? 0,
                        recordsFiltered: res.noOfItem ?? 0,
                        data: res.items ?? []
                    });
                },
                error: function () {
                    callback({ draw: dtReq.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                }
            });
        },
        columns: [
            {
                data: 'codeValue',
                title: 'Kod',
                render: function (data) {
                    return $('<div>').text(data ?? '').html();
                }
            },
            {
                data: "status",
                title: "Durum",
                render: function (d) {
                    const label = statusLabels[d] ?? d ?? "";
                    const badgeClass = statusBadges[d] ?? "badge bg-light text-dark";
                    return `<span class="${badgeClass}">${label}</span>`;
                }
            },
            { data: "salesOrderNo", title: "Satış Sipariş No" },
            { data: "salesItemNo", title: "Kalem Numarası" },
            { data: "plannedOrderNo", title: "Planlı Sipariş No" },
            {
                data: 'producedAt',
                title: 'Üretim Tarihi',
                render: function (d) {
                    if (!d) return '';
                    const dt = new Date(d);
                    return dt.toLocaleDateString('tr-TR');
                }
            },
            {
                data: 'expirationDate',
                title: 'SKT',
                render: function (d) {
                    if (!d) return '';
                    const dt = new Date(d);
                    return dt.toLocaleDateString('tr-TR');
                }
            },
        ],
        order: [[0, "desc"]],
        lengthMenu: [25, 50, 100],
        language: { url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" }
    });

    filterButton.on("click", function (event) {
        event.preventDefault();
        dt.ajax.reload();
    });

    const sanitizeFilePart = (value, fallback) => {
        const cleaned = (value ?? "").toString().trim();
        if (!cleaned) return fallback;
        return cleaned.replace(/[\\/:*?"<>|]/g, "-");
    };

    const buildExportFileName = () => {
        const salesText = salesSelect.find("option:selected").text().trim();
        const plannedText = plannedSelect.find("option:selected").text().trim();

        let salesOrderNo = "all";
        let salesItemNo = "all";

        if (salesText) {
            const parts = salesText.split("/").map((p) => p.trim());
            salesOrderNo = sanitizeFilePart(parts[0], "all");
            salesItemNo = sanitizeFilePart(parts[1], "all");
        }

        const plannedOrderNo = sanitizeFilePart(plannedText, "all");
        return `${salesOrderNo}-${salesItemNo}_${plannedOrderNo}.csv`;
    };

    const downloadBlob = (blob, filename) => {
        const link = document.createElement("a");
        const url = window.URL.createObjectURL(blob);
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    };

    exportButton.on("click", function (event) {
        event.preventDefault();

        const salesOrderItemId = Number(salesSelect.val());
        const plannedOrderId = Number(plannedSelect.val());
        const statusValue = statusSelect.val();
        const onlyCodes = onlyCodesCheckbox.is(":checked");
        const shiftDateFilter = getShiftDateFilter();
        const codeValue = getCodeValueFilter();

        const hasCodeValue = codeValue.length > 0;
        const hasSalesOrder = Number.isFinite(salesOrderItemId) && salesOrderItemId > 0;
        const hasPlannedOrder = Number.isFinite(plannedOrderId) && plannedOrderId > 0;
        const hasStatus = statusValue !== "" && statusValue !== null;
        const hasShiftDate = shiftDateFilter.shiftDateStart !== null;

        if (!hasCodeValue && !hasSalesOrder && !hasPlannedOrder && !hasStatus && !hasShiftDate) {
            Toast?.fire({
                icon: "warning",
                title: "Lütfen en az bir filtre seçin."
            });
            return;
        }

        const payload = {
            codeValue: hasCodeValue ? codeValue : null,
            salesOrderItemId: hasSalesOrder ? salesOrderItemId : null,
            plannedOrderId: hasPlannedOrder ? plannedOrderId : null,
            status: hasStatus ? Number(statusValue) : null,
            onlyCodes: onlyCodes,
            shiftDateStart: shiftDateFilter.shiftDateStart,
            shiftDateEnd: shiftDateFilter.shiftDateEnd
        };

        const defaultFileName = buildExportFileName();
        //const defaultFileName = "mcyık"

        const startDownload = (inputValue) => {
            let fileName = (inputValue ?? "").toString().trim();
            if (!fileName) {
                fileName = defaultFileName;
            }

            const hasExtension = /\.[^.\s]+$/.test(fileName);
            if (!hasExtension) {
                fileName += ".csv";
            }

            fileName = fileName.replace(/[\\/:*?"<>|]/g, "-");

            Swal?.fire({
                title: "Dosya hazırlanıyor...",
                text: "Lütfen bekleyin",
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            $.ajax({
                url: "/Codes/ExportCodeReport",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                xhrFields: { responseType: "blob" },
                success: function (data) {
                    const blob = data instanceof Blob ? data : new Blob([data], { type: "text/csv;charset=utf-8" });
                    downloadBlob(blob, fileName);
                },
                error: function (xhr) {
                    parseErrorResponse?.(xhr);
                },
                complete: function () {
                    Swal?.close();
                }
            });
        };

        if (!Swal?.fire) {
            startDownload(defaultFileName);
            return;
        }

        Swal.fire({
            title: "Dosya adı",
            text: "Lütfen indirilecek dosyanın adını giriniz.",
            input: "text",
            inputValue: defaultFileName,
            showCancelButton: true,
            confirmButtonText: "İndir",
            cancelButtonText: "Vazgeç",
            inputValidator: (value) => {
                if (!value || !value.trim()) {
                    return "Dosya adı zorunludur.";
                }
                return null;
            }
        }).then((result) => {
            if (!result.isConfirmed) {
                return;
            }
            startDownload(result.value);
        });
    });
});
