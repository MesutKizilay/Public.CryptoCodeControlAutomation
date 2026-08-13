$(function () {
    const operationLabels = {
        StatusChange: "Durum Düzeltme",
        ShiftDateChange: "Üretim Tarihi",
        ProductionReset: "\u00dcretim S\u0131f\u0131rlama"
    };

    const operationBadges = {
        StatusChange: "badge bg-label-danger",
        ShiftDateChange: "badge bg-label-warning",
        ProductionReset: "badge bg-label-danger"
    };

    const statusLabels = {
        0: "Available",
        1: "Allocated",
        2: "ProducedOk",
        3: "Reject",
        4: "Scrap",
        5: "Void"
    };

    const fieldMap = {
        codeAdjustmentLogId: "CodeAdjustmentLogId",
        operationType: "OperationType",
        salesOrderNo: "SalesOrderNo",
        salesItemNo: "SalesItemNo",
        plannedOrderNo: "PlannedOrderNo",
        fromStatus: "FromStatus",
        toStatus: "ToStatus",
        fromShiftDate: "FromShiftDate",
        toShiftDate: "ToShiftDate",
        quantity: "Quantity",
        reason: "Reason",
        createdBy: "CreatedBy",
        createdAt: "CreatedAt"
    };

    const formatDateTime = (value) => {
        if (!value) return "";
        return new Date(value).toLocaleString("tr-TR");
    };

    const formatDate = (value) => {
        if (!value) return "";
        return new Date(value).toLocaleDateString("tr-TR");
    };

    const formatStatus = (value) => statusLabels[value] ?? "";

    const buildDynamicQuery = (dtReq) => {
        const searchValue = dtReq.search?.value?.trim() || "";
        const sort = (dtReq.order || [])
            .map(o => {
                const data = dtReq.columns[o.column]?.data;
                const field = fieldMap[data];
                return field ? { field: field, direction: o.dir } : null;
            })
            .filter(Boolean);

        const dynamicQuery = { sort: sort, filter: null };

        if (searchValue) {
            dynamicQuery.filter = {
                field: "OperationType",
                operator: "contains",
                value: searchValue,
                logic: "or",
                filters: [
                    { field: "SalesOrderNo", operator: "contains", value: searchValue },
                    { field: "SalesItemNo", operator: "contains", value: searchValue },
                    { field: "PlannedOrderNo", operator: "contains", value: searchValue },
                    { field: "CreatedBy", operator: "contains", value: searchValue },
                    { field: "Reason", operator: "contains", value: searchValue }
                ]
            };
        }

        return dynamicQuery;
    };

    $("#code-adjustment-log-table").DataTable({
        processing: true,
        serverSide: true,
        autoWidth: false,
        scrollX: true,
        scrollCollapse: true,
        ajax: function (dtReq, callback) {
            const pageIndex = Math.trunc(dtReq.start / dtReq.length);
            const pageSize = dtReq.length;

            $.ajax({
                url: `/CodeAdjustmentLogs/GetList?Index=${pageIndex}&Size=${pageSize}`,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(buildDynamicQuery(dtReq)),
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
            { data: "codeAdjustmentLogId", title: "Id", width: "80px" },
            {
                data: "createdAt",
                title: "İşlem Tarihi",
                render: function (value) {
                    return formatDateTime(value);
                }
            },
            { data: "createdBy", title: "Kullanıcı" },
            {
                data: "operationType",
                title: "İşlem Tipi",
                render: function (value) {
                    const label = operationLabels[value] ?? value ?? "";
                    const badgeClass = operationBadges[value] ?? "badge bg-label-secondary";
                    return `<span class="${badgeClass}">${label}</span>`;
                }
            },
            { data: "salesOrderNo", title: "Satış Sipariş No" },
            { data: "salesItemNo", title: "Kalem Numarası" },
            { data: "plannedOrderNo", title: "Planlı Sipariş No" },
            {
                data: "fromStatus",
                title: "Durum Değişimi",
                render: function (_value, _type, row) {
                    const fromStatus = formatStatus(row.fromStatus);
                    const toStatus = formatStatus(row.toStatus);
                    if (!fromStatus && !toStatus) return "";
                    return `${fromStatus || "-"} -> ${toStatus || "-"}`;
                }
            },
            {
                data: "fromShiftDate",
                title: "Üretim Tarihi",
                render: function (_value, _type, row) {
                    const fromDate = formatDate(row.fromShiftDate);
                    const toDate = formatDate(row.toShiftDate);
                    if (!fromDate && !toDate) return "";
                    return `${fromDate || "-"} -> ${toDate || "-"}`;
                }
            },
            {
                data: "quantity",
                title: "Adet",
                className: "text-end",
                render: function (value) {
                    return Number(value ?? 0).toLocaleString("tr-TR");
                }
            },
            {
                data: "reason",
                title: "Açıklama",
                orderable: false,
                render: function (value) {
                    const text = value ?? "";
                    return `<span class="text-wrap">${text}</span>`;
                }
            }
        ],
        order: [[1, "desc"]],
        lengthMenu: [10, 25, 50, 100],
        language: { url: "../../assets/vendor/libs/datatables-bs5/i18n/tr.json" }
    });
});
