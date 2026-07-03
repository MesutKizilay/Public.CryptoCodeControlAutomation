$(function () {
    const legacyOrderTablesEnabled = false;

    const setText = (id, value) => {
        const el = document.getElementById(id);
        if (el) el.textContent = value ?? 0;
    };

    const plannedOrdersBody = document.querySelector("#dashboard-plannedorders-table tbody");
    const plannedSubtitle = document.getElementById("planned-orders-subtitle");
    const codeStatusSelection = document.getElementById("code-status-selection");
    const producedPeriodLabel = document.getElementById("produced-period-label");

    const producedPeriodLabels = {
        daily: "Günlük",
        weekly: "Haftalık",
        monthly: "Aylık",
        yearly: "Yıllık"
    };

    const formatSalesOrderItemStatus = (status) => {
        switch (status) {
            case 0:
                return '<span class="badge bg-success">Aktif</span>';
            case 1:
                return '<span class="badge bg-primary">Tamamlandı</span>';
            case 2:
                return '<span class="badge bg-danger">İptal edildi</span>';
            default:
                return '<span class="badge bg-light text-dark">Bilinmiyor</span>';
        }
    };

    const formatPlannedOrderStatus = (status) => {
        switch (status) {
            case 0:
                return '<span class="badge bg-success">Aktif</span>';
            case 1:
                return '<span class="badge bg-primary">Tamamlandı</span>';
            case 2:
                return '<span class="badge bg-danger">İptal edildi</span>';
            default:
                return '<span class="badge bg-light text-dark">Bilinmiyor</span>';
        }
    };

    const formatCodeUploadStatus = (isCodeUploaded) => {
        return isCodeUploaded
            ? '<span class="badge bg-success">Kod Yüklendi</span>'
            : '<span class="badge bg-danger">Kod Yüklenmedi</span>';
    };

    const formatQuantity = (value) => {
        if (value === null || value === undefined) return "-";
        return Number(value).toLocaleString("tr-TR");
    };

    const renderPlannedOrders = (items) => {
        if (!plannedOrdersBody) return;
        plannedOrdersBody.innerHTML = "";

        if (!Array.isArray(items) || items.length === 0) {
            plannedOrdersBody.innerHTML = `<tr class="text-muted"><td colspan="5">Planlı sipariş bulunamadı.</td></tr>`;
            return;
        }

        items.forEach((item) => {
            plannedOrdersBody.insertAdjacentHTML("beforeend", `
        <tr data-planned-order-id="${item?.plannedOrderId ?? ""}" data-planned-order-no="${item?.plannedOrderNo ?? ""}">
            <td>${item?.plannedOrderId ?? ""}</td>
            <td>${item?.plannedOrderNo ?? ""}</td>
            <td>${item?.lineCode ?? ""}</td>
            <td>${item?.totalUnitQty ?? 0}</td>
            <td>${formatPlannedOrderStatus(item?.status)}</td>
        </tr>    `);
        });
    };

    let selectedSalesOrderItemId = null;
    let selectedPlannedOrderId = null;
    let selectedSalesOrderLabel = "";
    let selectedPlannedOrderLabel = "";
    let codeStatusChart = null;
    let producedChart = null;
    let selectedProducedPeriod = "monthly";

    const buildOrUpdateChart = (summary) => {
        const series = [
            summary?.available ?? 0,
            summary?.allocated ?? 0,
            summary?.producedOk ?? 0,
            summary?.reject ?? 0,
            summary?.scrap ?? 0,
            summary?.void ?? 0
        ];

        if (!codeStatusChart) {
            const el = document.querySelector("#code-status-donut");
            if (!el || !window.ApexCharts) return;

            codeStatusChart = new ApexCharts(el, {
                chart: { type: "donut", height: 320 },
                labels: ["Available", "Allocated", "ProducedOk", "Reject Kurtarma", "Scrap", "Void"],
                series: series,
                legend: { position: "bottom" },
                dataLabels: { enabled: true },
                colors: ["#00cfe8", "#7367f0", "#28c76f", "#ff9f43", "#ea5455", "#4b4b4b"],
                plotOptions: { pie: { donut: { size: "70%" } } }
            });
            codeStatusChart.render();
        }
        else {
            codeStatusChart.updateSeries(series);
        }
    };

    const loadCodeSummary = () => {
        $.ajax({
            url: "/Dashboard/GetCodeStatusSummary",
            type: "GET",
            success: function (res) {
                setText("code-total", res?.total ?? 0);
                setText("code-produced", res?.producedOk ?? 0);
                setText("code-scrap", res?.scrap ?? 0);
                setText("code-reject", res?.reject ?? 0);
                setText("code-available", res?.available ?? 0);
                setText("code-allocated", res?.allocated ?? 0);
                setText("code-void", res?.void ?? 0);
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    const loadCodeDistribution = () => {
        $.ajax({
            url: "/Dashboard/GetCodeStatusSummary",
            type: "GET",
            data: {
                salesOrderItemId: selectedSalesOrderItemId
                //plannedOrderId: selectedPlannedOrderId
            },
            success: function (res) {
                buildOrUpdateChart(res);
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    const updateProducedPeriodLabel = () => {
        if (!producedPeriodLabel) return;
        producedPeriodLabel.textContent = producedPeriodLabels[selectedProducedPeriod] ?? "Aylık";
    };

    const updateCodeStatusSelection = () => {
        if (!codeStatusSelection) return;
        if (!selectedSalesOrderLabel && !selectedPlannedOrderLabel) {
            codeStatusSelection.textContent = "Tüm kodlar";
            return;
        }

        if (selectedSalesOrderLabel && selectedPlannedOrderLabel) {
            codeStatusSelection.textContent = `${selectedSalesOrderLabel} / ${selectedPlannedOrderLabel}`;
            return;
        }

        codeStatusSelection.textContent = selectedSalesOrderLabel || selectedPlannedOrderLabel;
    };


    const buildOrUpdateProducedChart = (items) => {
        const chartEl = document.getElementById("produced-bar-chart");
        if (!chartEl || !window.Chart) return;

        const labels = (items || []).map(item => item?.label ?? "");
        const data = (items || []).map(item => item?.count ?? 0);

        if (!producedChart) {
            if (chartEl.dataset?.height) {
                chartEl.height = chartEl.dataset.height;
            }

            const ctx = chartEl.getContext("2d");
            producedChart = new Chart(ctx, {
                type: "bar",
                data: {
                    labels: labels,
                    datasets: [
                        {
                            label: "Üretilen",
                            data: data,
                            backgroundColor: "#7367f0",
                            borderColor: "transparent",
                            maxBarThickness: 18,
                            borderRadius: { topLeft: 8, topRight: 8 }
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: {
                        x: { grid: { display: false } },
                        y: {
                            beginAtZero: true,
                            ticks: { precision: 0 }
                        }
                    }
                }
            });
        } else {
            producedChart.data.labels = labels;
            producedChart.data.datasets[0].data = data;
            producedChart.update();
        }
    };

    const loadProducedChart = () => {
        $.ajax({
            url: "/Dashboard/GetProducedCodeStatistics",
            type: "GET",
            data: {
                period: selectedProducedPeriod,
                salesOrderItemId: selectedSalesOrderItemId
                //plannedOrderId: selectedPlannedOrderId
            },
            success: function (items) {
                buildOrUpdateProducedChart(items);
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    };

    const loadPlannedOrders = (salesOrderItemId) => {
        if (!plannedOrdersBody) return;
        plannedOrdersBody.innerHTML = `<tr class="text-muted"><td colspan="5">Yükleniyor...</td></tr>`;

        $.ajax({
            url: "/Dashboard/GetPlannedOrdersBySalesOrderItemId",
            type: "GET",
            data: { salesOrderItemId: salesOrderItemId },
            success: function (items) {
                renderPlannedOrders(items);
            },
            error: function (xhr) {
                renderPlannedOrders([]);
                parseErrorResponse?.(xhr);
            }
        });
    };

    updateProducedPeriodLabel();
    updateCodeStatusSelection();
    loadCodeSummary();
    loadCodeDistribution();
    loadProducedChart();

    document.querySelectorAll("[data-produced-period]").forEach((item) => {
        item.addEventListener("click", () => {
            const period = item.getAttribute("data-produced-period");
            if (!period) return;
            selectedProducedPeriod = period;
            updateProducedPeriodLabel();
            loadProducedChart();
        });
    });

    const salesPlannedDt = $("#dashboard-sales-planned-table").DataTable({
        processing: true,
        serverSide: true,
        searching: true,
        ordering: false,
        autoWidth: false,
        scrollX: true,
        ajax: function (dtReq, callback) {
            const pageIndex = Math.trunc(dtReq.start / dtReq.length);
            const pageSize = dtReq.length;
            const searchValue = dtReq.search?.value?.trim() || "";

            $.ajax({
                url: `/Dashboard/GetSalesPlannedOrderSummary?Index=${pageIndex}&Size=${pageSize}`,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ search: searchValue }),
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
            { data: "salesOrderNo", title: "Satış Sipariş No" },
            { data: "salesItemNo", title: "Kalem Numarası" },
            {
                data: "sapCaseQty",
                title: "Koli Miktarı",
                className: "text-end",
                render: function (value) {
                    return formatQuantity(value);
                }
            },
            {
                data: "sapPlannedUnitQty",
                title: "Birim Miktarı",
                className: "text-end",
                render: function (value) {
                    return formatQuantity(value);
                }
            },
            {
                data: "isCodeUploaded",
                title: "Kod Yükleme Durumu",
                className: "text-center",
                render: function (value) {
                    return formatCodeUploadStatus(value);
                }
            },
            {
                data: "plannedOrderNo",
                title: "Planlı Sipariş No",
                render: function (value) {
                    return value || "-";
                }
            },
            {
                data: "plannedOrderUnitQty",
                title: "Planlı Sipariş Miktarı",
                className: "text-end",
                render: function (value) {
                    return formatQuantity(value);
                }
            }
        ],
        lengthMenu: [10, 25, 50],
        language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" }
    });

    $("#dashboard-sales-planned-table tbody").on("click", "tr", function () {
        const data = salesPlannedDt.row(this).data();
        if (!data) return;

        $("#dashboard-sales-planned-table tbody tr").removeClass("table-active");
        $(this).addClass("table-active");

        const salesOrderItemId = Number(data.salesOrderItemId);
        const plannedOrderId = Number(data.plannedOrderId);

        selectedSalesOrderItemId = Number.isFinite(salesOrderItemId) && salesOrderItemId > 0
            ? salesOrderItemId
            : null;
        selectedPlannedOrderId = Number.isFinite(plannedOrderId) && plannedOrderId > 0
            ? plannedOrderId
            : null;
        selectedSalesOrderLabel = `${data.salesOrderNo ?? ""} / ${data.salesItemNo ?? ""}`.trim();
        selectedPlannedOrderLabel = data.plannedOrderNo ?? "";

        updateCodeStatusSelection();
        loadCodeDistribution();
        loadProducedChart();
    });

    if (legacyOrderTablesEnabled) {
        const salesDt = $("#dashboard-salesorderitems-table").DataTable({
            processing: true,
            serverSide: true,
            searching: true,
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
                { data: "salesOrderItemId", title: "Id" },
                { data: "salesOrderNo", title: "Satış Sipariş No" },
                { data: "salesItemNo", title: "Kalem Numarası" },
                { data: "materialNo", title: "Mamül No" },
                { data: "sapPlannedUnitQty", title: "Birim Miktarı" },
                {
                    data: "status",
                    title: "Durum",
                    orderable: false,
                    render: function (d) {
                        return formatSalesOrderItemStatus(d);
                    }
                }
            ],
            order: [[0, "desc"]],
            lengthMenu: [5, 10, 25],
            language: { url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/tr.json" }
        });

        $("#dashboard-salesorderitems-table tbody").on("click", "tr", function () {
            const data = salesDt.row(this).data();
            if (!data) return;

            $("#dashboard-salesorderitems-table tbody tr").removeClass("table-active");
            $(this).addClass("table-active");

            selectedSalesOrderItemId = data.salesOrderItemId;
            selectedPlannedOrderId = null;
            selectedPlannedOrderLabel = "";
            selectedSalesOrderLabel = `${data.salesOrderNo ?? ""}`.trim();

            if (plannedSubtitle) {
                const label = `${data.salesOrderNo ?? ""} / ${data.salesItemNo ?? ""}`;
                plannedSubtitle.textContent = label || "Satış siparişi seçiniz";
            }

            loadPlannedOrders(data.salesOrderItemId);
            updateCodeStatusSelection();
            loadCodeDistribution();
            loadProducedChart();
        });

        $("#dashboard-plannedorders-table tbody").on("click", "tr", function () {
            const plannedOrderId = Number(this.dataset.plannedOrderId);
            if (!Number.isFinite(plannedOrderId)) return;

            $("#dashboard-plannedorders-table tbody tr").removeClass("table-active");
            $(this).addClass("table-active");

            selectedPlannedOrderId = plannedOrderId;
            selectedPlannedOrderLabel = this.dataset.plannedOrderNo ?? "";
            updateCodeStatusSelection();
            loadCodeDistribution();
            loadProducedChart();
        });
    }
});
