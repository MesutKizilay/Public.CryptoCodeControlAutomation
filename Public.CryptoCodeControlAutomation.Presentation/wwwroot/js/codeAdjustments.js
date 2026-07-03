$(function () {
    const salesSelect = $("#adjust-salesorder");
    const plannedSelect = $("#adjust-plannedorder");
    const summaryButton = $("#adjust-summary-btn");
    const resetProductionButton = $("#reset-production-btn");
    const dailyProducedTableBody = $("#adjust-daily-produced-table tbody");

    const statusOperation = $("#adjust-status-operation");
    const statusQuantity = $("#adjust-status-quantity");
    const statusShiftDate = $("#adjust-status-shift-date");
    const statusShiftDateWrapper = $("#adjust-status-shift-date-wrapper");
    const statusShiftDateLabel = $('label[for="adjust-status-shift-date"]');
    const statusReason = $("#adjust-status-reason");
    const statusButton = $("#adjust-status-btn");

    const shiftFromDate = $("#adjust-shift-from-date");
    const shiftToDate = $("#adjust-shift-to-date");
    const shiftQuantity = $("#adjust-shift-quantity");
    const shiftReason = $("#adjust-shift-reason");
    const shiftButton = $("#adjust-shift-btn");

    let plannedItems = [];

    const statusNames = {
        1: "Allocated",
        2: "ProducedOk"
    };

    const initSelect2 = () => {
        salesSelect.select2({ placeholder: "Satış Siparişi", width: "100%", allowClear: true });
        plannedSelect.select2({ placeholder: "Planlı Sipariş", width: "100%", allowClear: true });
    };

    const initDatePickers = () => {
        if (typeof flatpickr !== "function") return;

        flatpickr(".code-adjustment-date", {
            dateFormat: "d-m-Y",
            monthSelectorType: "static"
        });
    };

    const formatDateForRequest = (date) => {
        if (!(date instanceof Date) || Number.isNaN(date.getTime())) return null;

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    };

    const getDateForRequest = ($input) => {
        const picker = $input.get(0)?._flatpickr;
        const selectedDate = picker?.selectedDates?.[0];
        if (selectedDate) {
            return formatDateForRequest(selectedDate);
        }

        const value = $input.val()?.toString().trim();
        if (!value) return null;

        const displayDateMatch = value.match(/^(\d{2})-(\d{2})-(\d{4})$/);
        if (displayDateMatch) {
            return `${displayDateMatch[3]}-${displayDateMatch[2]}-${displayDateMatch[1]}`;
        }

        return value;
    };

    const clearSelect = ($select) => {
        $select.empty();
        $select.append("<option></option>");
        $select.trigger("change");
    };

    const isValidId = (value) => Number.isFinite(value) && value > 0;

    const getSelection = () => {
        const salesOrderItemId = Number(salesSelect.val());
        const plannedOrderId = Number(plannedSelect.val());

        return {
            salesOrderItemId: isValidId(salesOrderItemId) ? salesOrderItemId : null,
            plannedOrderId: isValidId(plannedOrderId) ? plannedOrderId : null
        };
    };

    const ensureSelection = () => {
        const selection = getSelection();
        if (!selection.salesOrderItemId && !selection.plannedOrderId) {
            Toast?.fire({ icon: "warning", title: "Satış siparişi veya planlı sipariş seçiniz." });
            return null;
        }

        return selection;
    };

    const parseQuantity = ($input) => {
        const value = Number($input.val());
        return Number.isInteger(value) && value > 0 ? value : null;
    };

    const setSummary = (summary) => {
        $("#adjust-summary-available").text(summary?.available ?? 0);
        $("#adjust-summary-allocated").text(summary?.allocated ?? 0);
        $("#adjust-summary-produced").text(summary?.producedOk ?? 0);
        $("#adjust-summary-reject").text(summary?.reject ?? 0);
        $("#adjust-summary-scrap").text(summary?.scrap ?? 0);
        $("#adjust-summary-void").text(summary?.void ?? 0);
    };

    const renderDailyProducedSummary = (items) => {
        dailyProducedTableBody.empty();

        if (!Array.isArray(items) || items.length === 0) {
            dailyProducedTableBody.html('<tr class="text-muted"><td colspan="2">Üretim kaydı bulunamadı.</td></tr>');
            return;
        }

        items.forEach((item) => {
            const shiftDate = (item?.shiftDate ?? "").toString().slice(0, 10);
            const displayDate = shiftDate ? shiftDate.split("-").reverse().join(".") : "";
            const producedCount = Number(item?.producedCount ?? 0);

            dailyProducedTableBody.append(`
                <tr data-shift-date="${shiftDate}" title="üretim düzeltme alanına aktar">
                    <td>${displayDate}</td>
                    <td class="text-end fw-semibold">${producedCount.toLocaleString("tr-TR")}</td>
                </tr>`);
        });
    };

    const loadSalesOrders = () => {
        $.ajax({
            url: "/SalesOrderItems/GetList",
            type: "GET",
            success: function (items) {
                clearSelect(salesSelect);
                (items ?? []).forEach((item) => {
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
                fillPlannedOrders(Number(salesSelect.val()));
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    const loadSummary = (selection) => {
        $.ajax({
            url: "/Codes/GetCodeStatusSummary",
            type: "GET",
            data: selection,
            success: function (summary) {
                setSummary(summary);
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    const loadDailyProducedSummary = (selection) => {
        dailyProducedTableBody.html('<tr class="text-muted"><td colspan="2">Yükleniyor...</td></tr>');

        $.ajax({
            url: "/Codes/GetDailyProducedCodeSummary",
            type: "GET",
            data: selection,
            success: function (items) {
                renderDailyProducedSummary(items);
            },
            error: function (xhr) {
                renderDailyProducedSummary([]);
                parseErrorResponse?.(xhr);
            }
        });
    };

    const loadAdjustmentData = () => {
        const selection = ensureSelection();
        if (!selection) return;

        loadSummary(selection);
        loadDailyProducedSummary(selection);
    };

    const showLoading = (title) => {
        Swal?.fire({
            title: title,
            text: "Lütfen bekleyiniz...",
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => Swal.showLoading()
        });
    };

    const confirmDangerousOperation = (html) => {
        if (!Swal?.fire) {
            return Promise.resolve(window.confirm($(html).text() || "Devam etmek istiyor musunuz?"));
        }

        return Swal.fire({
            icon: "warning",
            title: "İşlem Onayı",
            html: html,
            showCancelButton: true,
            confirmButtonText: "Manipüle Et",
            cancelButtonText: "Vazgeç",
            confirmButtonColor: "#ea5455"
        }).then((result) => result.isConfirmed);
    };

    const sendStatusAdjustment = (payload) => {
        showLoading("Kod durumu güncelleniyor...");

        $.ajax({
            url: "/Codes/AdjustCodeStatus",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal?.close();
                Toast?.fire({ icon: "success", title: res?.message || "Status güncellendi." });
                statusQuantity.val("");
                statusReason.val("");
                loadAdjustmentData();
            },
            error: function (xhr) {
                Swal?.close();
                parseErrorResponse?.(xhr);
            }
        });
    };

    const sendShiftDateAdjustment = (payload) => {
        showLoading("üretim tarihi güncelleniyor...");

        $.ajax({
            url: "/Codes/AdjustCodeShiftDate",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal?.close();
                Toast?.fire({ icon: "success", title: res?.message || "üretim tarihi güncellendi." });
                shiftQuantity.val("");
                shiftReason.val("");
                loadAdjustmentData();
            },
            error: function (xhr) {
                Swal?.close();
                parseErrorResponse?.(xhr);
            }
        });
    };

    const requestResetPassword = () => {
        if (!Swal?.fire) {
            return Promise.resolve(window.prompt("Uretimi sifirlamak icin sifre giriniz."));
        }

        return Swal.fire({
            icon: "warning",
            title: "\u00dcretimi S\u0131f\u0131rla",
            html: '<div class="text-start">Se\u00e7ili kay\u0131tlar\u0131n kod durumu, planl\u0131 sipari\u015f, istasyon, paketleme, tasnif ve \u00fcretim tarihleri s\u0131f\u0131rlanacak.</div>',
            input: "password",
            inputPlaceholder: "\u015eifre",
            inputAttributes: {
                autocomplete: "off"
            },
            showCancelButton: true,
            confirmButtonText: "\u00dcretimi S\u0131f\u0131rla",
            cancelButtonText: "Vazge\u00e7",
            confirmButtonColor: "#ea5455",
            inputValidator: (value) => {
                if (!value) {
                    return "\u015eifre zorunludur.";
                }

                return null;
            }
        }).then((result) => result.isConfirmed ? result.value : null);
    };

    const sendResetProduction = (payload) => {
        showLoading("\u00dcretim s\u0131f\u0131rlan\u0131yor...");

        $.ajax({
            url: "/Codes/ResetProduction",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal?.close();
                Toast?.fire({ icon: "success", title: res?.message || "\u00dcretim s\u0131f\u0131rland\u0131." });
                loadAdjustmentData();
            },
            error: function (xhr) {
                Swal?.close();
                parseErrorResponse?.(xhr);
            }
        });
    };

    const updateStatusOperationView = () => {
        const operationParts = (statusOperation.val() || "").split("-").map(Number);
        const fromStatus = operationParts[0];
        const toStatus = operationParts[1];
        const isProducedToAllocated = fromStatus === 2 && toStatus === 1;

        statusShiftDateWrapper.removeClass("d-none");
        statusShiftDate.prop("disabled", false);
        statusShiftDateLabel.text(isProducedToAllocated ? "D\u00fc\u015f\u00fclecek \u00dcretim Tarihi" : "Hedef \u00dcretim Tarihi");
    };

    summaryButton.on("click", function (event) {
        event.preventDefault();
        loadAdjustmentData();
    });

    resetProductionButton.on("click", function (event) {
        event.preventDefault();

        const selection = ensureSelection();
        if (!selection) return;

        requestResetPassword().then((password) => {
            if (!password) return;

            sendResetProduction({
                salesOrderItemId: selection.salesOrderItemId,
                plannedOrderId: selection.plannedOrderId,
                password: password
            });
        });
    });

    dailyProducedTableBody.on("click", "tr[data-shift-date]", function () {
        const shiftDate = $(this).attr("data-shift-date");
        if (!shiftDate) return;

        const input = shiftFromDate.get(0);
        if (input?._flatpickr) {
            input._flatpickr.setDate(shiftDate, true, "Y-m-d");
        }
        else {
            shiftFromDate.val(shiftDate);
        }

        const statusInput = statusShiftDate.get(0);
        if (statusInput?._flatpickr) {
            statusInput._flatpickr.setDate(shiftDate, true, "Y-m-d");
        }
        else {
            statusShiftDate.val(shiftDate);
        }
    });

    salesSelect.on("change", function () {
        fillPlannedOrders(Number($(this).val()));
    });

    statusOperation.on("change", updateStatusOperationView);

    statusButton.on("click", function (event) {
        event.preventDefault();

        const selection = ensureSelection();
        if (!selection) return;

        const quantity = parseQuantity(statusQuantity);
        if (!quantity) {
            Toast?.fire({ icon: "warning", title: "Geçerli bir adet giriniz." });
            return;
        }

        const operationParts = (statusOperation.val() || "").split("-").map(Number);
        const fromStatus = operationParts[0];
        const toStatus = operationParts[1];
        const shiftDate = getDateForRequest(statusShiftDate);
        const shiftDateDisplay = statusShiftDate.val()?.toString().trim();
        const shiftDateWarning = toStatus === 2 ? "Hedef \u00fcretim tarihini se\u00e7iniz." : "D\u00fc\u015f\u00fclecek \u00fcretim tarihini se\u00e7iniz.";
        const reason = statusReason.val()?.toString().trim();

        if (!shiftDate) {
            Toast?.fire({ icon: "warning", title: shiftDateWarning });
            return;
        }

        if (!reason) {
            Toast?.fire({ icon: "warning", title: "Açıklama giriniz." });
            return;
        }

        const html = `
            <div class="text-start">
                <p class="mb-2"><strong>${quantity}</strong> adet kod <strong>${statusNames[fromStatus]}</strong> durumundan <strong>${statusNames[toStatus]}</strong> durumuna alınacak.</p>
                ${shiftDateDisplay ? `<p class="mb-2">Üretim Tarihi: <strong>${shiftDateDisplay}</strong></p>` : ""}
                <p class="mb-0">Bu işlem üretim raporlarını ve müşteriyle paylaşılan günlük adetleri etkiler. İşlem kayıt altına alınacaktır.</p>
            </div>`;

        confirmDangerousOperation(html).then((confirmed) => {
            if (!confirmed) return;

            sendStatusAdjustment({
                salesOrderItemId: selection.salesOrderItemId,
                plannedOrderId: selection.plannedOrderId,
                fromStatus: fromStatus,
                toStatus: toStatus,
                quantity: quantity,
                shiftDate: shiftDate,
                reason: reason
            });
        });
    });

    shiftButton.on("click", function (event) {
        event.preventDefault();

        const selection = ensureSelection();
        if (!selection) return;

        const fromDate = getDateForRequest(shiftFromDate);
        const toDate = getDateForRequest(shiftToDate);
        const fromDateDisplay = shiftFromDate.val()?.toString().trim();
        const toDateDisplay = shiftToDate.val()?.toString().trim();
        const quantity = parseQuantity(shiftQuantity);
        const reason = shiftReason.val()?.toString().trim();

        if (!fromDate || !toDate) {
            Toast?.fire({ icon: "warning", title: "Eski ve yeni üretim tarihlerini seçiniz." });
            return;
        }

        if (fromDate === toDate) {
            Toast?.fire({ icon: "warning", title: "Eski ve yeni üretim tarihleri farklı olmalıdır." });
            return;
        }

        if (!quantity) {
            Toast?.fire({ icon: "warning", title: "Geçerli bir adet giriniz." });
            return;
        }

        if (!reason) {
            Toast?.fire({ icon: "warning", title: "Açıklama giriniz." });
            return;
        }

        const html = `
            <div class="text-start">
                <p class="mb-2"><strong>${quantity}</strong> adet üretilmiş kodun üretim tarihi <strong>${fromDateDisplay}</strong> tarihinden <strong>${toDateDisplay}</strong> tarihine taşınacak.</p>
                <p class="mb-0">Bu işlem günlük üretim raporlarını etkiler. İşlem kayıt altına alınacaktır.</p>
            </div>`;

        confirmDangerousOperation(html).then((confirmed) => {
            if (!confirmed) return;

            sendShiftDateAdjustment({
                salesOrderItemId: selection.salesOrderItemId,
                plannedOrderId: selection.plannedOrderId,
                fromShiftDate: fromDate,
                toShiftDate: toDate,
                quantity: quantity,
                reason: reason
            });
        });
    });

    initSelect2();
    initDatePickers();
    updateStatusOperationView();
    loadSalesOrders();
    clearSelect(plannedSelect);
    loadPlannedOrders();
});
