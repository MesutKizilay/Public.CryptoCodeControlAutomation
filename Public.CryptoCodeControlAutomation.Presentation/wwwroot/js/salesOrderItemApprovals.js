$(function () {
    const approvalLabels = {
        0: "Onay bekliyor",
        1: "Üretim onayı",
        2: "Sevkiyat onayı"
    };

    const approvalBadge = (status) => {
        const value = status ?? 0;
        switch (value) {
            case 1:
                return '<span class="badge bg-label-primary">Üretim Onayı</span>';
            case 2:
                return '<span class="badge bg-label-success">Sevkiyat Onayı</span>';
            default:
                return '<span class="badge bg-label-warning">Onay Bekliyor</span>';
        }
    };

    const salesOrderStatusBadge = (status) => {
        switch (status) {
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
    };

    const approvalInfo = (username, approvedAt) => {
        if (!username && !approvedAt) {
            return '<span class="text-muted small">-</span>';
        }

        const dateText = approvedAt ? new Date(approvedAt).toLocaleString('tr-TR') : '';
        return `
            <div class="d-flex flex-column">
                <span>${username || '-'}</span>
                <small class="text-muted">${dateText}</small>
            </div>`;
    };

    const actionButton = (row) => {
        const current = row.approvalStatus ?? 0;
        if (current === 0) {
            return `
                <button type="button" class="btn btn-sm btn-primary approval-action js-approval"
                        data-id="${row.salesOrderItemId}" data-status="1">
                    Üretim Onayı
                </button>`;
        }

        if (current === 1) {
            return `
                <button type="button" class="btn btn-sm btn-success approval-action js-approval"
                        data-id="${row.salesOrderItemId}" data-status="2">
                    Sevkiyat Onayı
                </button>`;
        }

        return '<span class="text-muted small">Tamamlandı</span>';
    };

    const dt = $("#salesOrderItemApprovalTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: function (dtReq, callback) {
            const pageIndex = Math.trunc(dtReq.start / dtReq.length);
            const pageSize = dtReq.length;
            const searchValue = dtReq.search?.value?.trim() || "";

            const sort = (dtReq.order || []).map(o => ({
                field: dtReq.columns[o.column].data,
                direction: o.dir
            }));

            const dynamicQuery = { sort: sort, filter: null };

            if (searchValue) {
                dynamicQuery.filter = {
                    field: "SalesOrderNo",
                    operator: "contains",
                    value: searchValue,
                    logic: "or",
                    filters: [
                        { field: "MaterialNo", operator: "contains", value: searchValue },
                        { field: "SalesItemNo", operator: "contains", value: searchValue }
                    ]
                };
            }

            $.ajax({
                url: "/SalesOrderItems/GetList?Index=" + pageIndex + "&Size=" + pageSize + "&withDeleted=false",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(dynamicQuery),
                success: function (res) {
                    const items = res.items ?? [];
                    callback({
                        draw: dtReq.draw,
                        recordsTotal: res.noOfItem ?? 0,
                        recordsFiltered: res.noOfItem ?? 0,
                        data: items
                    });
                },
                error: function () {
                    callback({ draw: dtReq.draw, recordsTotal: 0, recordsFiltered: 0, data: [] });
                }
            });
        },
        columns: [
            { data: "salesOrderItemId", title: "Id", width: "70px" },
            { data: "salesOrderNo", title: "Satış Sipariş No" },
            { data: "salesItemNo", title: "Kalem Numarası" },
            { data: "materialNo", title: "Mamül No" },
            { data: "gtin", title: "GTIN" },
            { data: "sapPlannedUnitQty", title: "Birim Miktarı" },
            { data: "sapCaseQty", title: "Koli Miktarı" },
            {
                data: "status",
                title: "Durum",
                render: function (d) {
                    return salesOrderStatusBadge(d);
                }
            },
            {
                data: "approvalStatus",
                title: "Onay Durumu",
                render: function (d) {
                    return approvalBadge(d);
                }
            },
            {
                data: null,
                title: "Üretim Onayı",
                orderable: false,
                render: function (row) {
                    return approvalInfo(row.productionApprovedByUsername, row.productionApprovedAt);
                }
            },
            {
                data: null,
                title: "Sevkiyat Onayı",
                orderable: false,
                render: function (row) {
                    return approvalInfo(row.shipmentApprovedByUsername, row.shipmentApprovedAt);
                }
            },
            {
                data: null,
                title: "İşlem",
                orderable: false,
                searchable: false,
                render: function (row) {
                    return actionButton(row);
                }
            }
        ],
        dom:
            "<'row mx-2'<'col-md-2'l>" +
            "<'col-md-10 dt-action-buttons d-flex align-items-center justify-content-md-end justify-content-center flex-wrap gap-2 mb-3 mb-md-0'f>>" +
            "<'row'<'col-12 table-responsive'tr>>" +
            "<'row mx-2 mt-2'<'col-sm-12 col-md-6'i><'col-sm-12 col-md-6'p>>",
        order: [[0, "desc"]],
        lengthMenu: [10, 25, 50, 100],
        language: { url: "../../assets/vendor/libs/datatables-bs5/i18n/tr.json" }
    });

    dt.on("init", function () {
        const $filter = $("#salesOrderItemApprovalTable_filter");
        $filter.find("label").contents().filter(function () { return this.nodeType === 3; }).remove();
        $filter.find("input").attr("placeholder", "S. Order No, M. No...").addClass("form-control me-2");
    });

    $("#salesOrderItemApprovalTable").on("click", ".js-approval", function () {
        const salesOrderItemId = Number(this.dataset.id);
        const approvalStatus = Number(this.dataset.status);
        const label = approvalLabels[approvalStatus] ?? "Onay";

        Swal.fire({
            title: `${label} verilsin mi?`,
            icon: "question",
            showCancelButton: true,
            confirmButtonText: "Onayla",
            cancelButtonText: "Vazgeç",
            showLoaderOnConfirm: true,
            allowOutsideClick: () => !Swal.isLoading(),
            preConfirm: async () => {
                try {
                    await $.ajax({
                        url: "/SalesOrderItems/UpdateApprovalStatus",
                        type: "POST",
                        contentType: "application/json",
                        data: JSON.stringify({
                            salesOrderItemId: salesOrderItemId,
                            approvalStatus: approvalStatus
                        })
                    });
                    return true;
                }
                catch (xhr) {
                    parseErrorResponse?.(xhr);
                    return false;
                }
            }
        }).then((result) => {
            if (!result.isConfirmed) return;
            dt.ajax.reload(null, false);
            Toast?.fire({ icon: "success", title: "Onay durumu güncellendi." });
        });
    });
});
