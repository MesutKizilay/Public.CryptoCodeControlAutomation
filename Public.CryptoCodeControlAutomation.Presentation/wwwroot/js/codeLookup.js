(function () {
    "use strict";

    const form = document.getElementById("code-lookup-form");
    const input = document.getElementById("code-lookup-input");
    const submitButton = document.getElementById("code-lookup-submit");
    const emptyState = document.getElementById("code-lookup-empty");
    const notFoundState = document.getElementById("code-lookup-not-found");
    const result = document.getElementById("code-lookup-result");
    const statusPanel = document.getElementById("code-lookup-status");
    let codeLookupScannerSource = null;

    if (!form || !input || !submitButton || !emptyState || !notFoundState || !result || !statusPanel) return;

    const statusMap = {
        0: { label: "Hazır", color: "secondary" },
        1: { label: "Tasnif Edilmiş", color: "info" },
        2: { label: "Üretilmiş", color: "success" },
        3: { label: "Iskarta", color: "warning" },
        4: { label: "Fire", color: "danger" },
        5: { label: "Boş", color: "dark" }
    };

    const plannedOrderStatusMap = {
        0: "Aktif",
        1: "Tamamlandı",
        2: "İptal Edildi"
    };

    const fields = {
        code: "lookup-code",
        salesOrderNo: "lookup-sales-order-no",
        salesItemNo: "lookup-sales-item-no",
        salesMaterialNo: "lookup-sales-material-no",
        gtin: "lookup-gtin",
        plannedOrderNo: "lookup-planned-order-no",
        lineCode: "lookup-line-code"
    };

    const normalizeCode = (value) => {
        return (value || "").replace(/\|9(1|2|3)/g, "\u001D9$1").trim();
    };

    const displayValue = (value) => {
        return value === null || value === undefined || value === "" ? "-" : value.toString();
    };

    const setText = (id, value) => {
        const element = document.getElementById(id);
        if (element) element.textContent = displayValue(value);
    };

    const formatDate = (value) => {
        if (!value) return "-";
        const date = new Date(value);
        return Number.isNaN(date.getTime()) ? "-" : date.toLocaleString("tr-TR");
    };

    const showLoading = (loading) => {
        submitButton.disabled = loading;
        submitButton.innerHTML = loading
            ? '<span class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>Sorgulanıyor'
            : '<i class="bx bx-search me-1"></i>Sorgula';
    };

    const showNotFound = () => {
        emptyState.classList.add("d-none");
        result.classList.add("d-none");
        notFoundState.classList.remove("d-none");
    };

    const renderResult = (data) => {
        emptyState.classList.add("d-none");
        notFoundState.classList.add("d-none");
        result.classList.remove("d-none");

        Object.entries(fields).forEach(([property, id]) => setText(id, data[property]));

        const status = statusMap[data.status] || { label: displayValue(data.status), color: "primary" };
        result.className = `card code-lookup-card code-lookup-tone-${status.color}`;
        setText("lookup-status-label", status.label);

        const packagingText = data.packagingLevel === null || data.packagingLevel === undefined
            ? "-"
            : `P${data.packagingLevel}`;
        setText("lookup-packaging-level", packagingText);
        setText("lookup-packaging-badge", packagingText);

        const packagingBadge = document.getElementById("lookup-packaging-badge");
        if (packagingBadge) packagingBadge.className = "code-lookup-package-value";

        setText(
            "lookup-planned-order-status",
            data.plannedOrderStatus === null || data.plannedOrderStatus === undefined
                ? "-"
                : plannedOrderStatusMap[data.plannedOrderStatus] || data.plannedOrderStatus);
        setText("lookup-allocated-at", formatDate(data.allocatedAt));
        setText("lookup-produced-at", formatDate(data.producedAt));
    };

    const lookupCode = (rawCode) => {
        const code = normalizeCode(rawCode);
        if (!code) {
            Toast?.fire({ icon: "warning", title: "Lütfen bir kod okutun." });
            input.focus();
            return;
        }

        showLoading(true);

        $.ajax({
            url: "/Codes/GetCodeLookup",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ code: code }),
            success: function (response) {
                if (!response) {
                    showNotFound();
                    return;
                }

                renderResult(response);
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            },
            complete: function () {
                showLoading(false);
                input.select();
                input.focus();
            }
        });
    };

    form.addEventListener("submit", (event) => {
        event.preventDefault();
        lookupCode(input.value);
    });

    const getResponseMessage = async (response) => {
        try {
            const body = await response.json();
            return body?.message || "Endustriyel el terminali baglantisi baslatilamadi.";
        }
        catch {
            return "Endustriyel el terminali baglantisi baslatilamadi.";
        }
    };

    const startCodeLookupScanner = async () => {
        try {
            const response = await fetch("/Codes/StartCodeLookupScanner", {
                method: "POST"
            });

            if (!response.ok) {
                throw new Error(await getResponseMessage(response));
            }

            codeLookupScannerSource = new EventSource("/Codes/CodeLookupScannerStream");

            codeLookupScannerSource.addEventListener("code", (event) => {
                const message = JSON.parse(event.data);
                const value = message?.value ?? "";

                input.value = value;
                lookupCode(value);
            });

            codeLookupScannerSource.onerror = () => {
                console.warn("Code lookup scanner baglantisi yeniden kurulmaya calisiliyor.");
            };
        }
        catch (error) {
            Toast?.fire({
                icon: "error",
                title: error?.message || "Endustriyel el terminali baglantisi baslatilamadi."
            });
        }
    };

    input.focus();
    startCodeLookupScanner();

    window.addEventListener("beforeunload", () => {
        codeLookupScannerSource?.close();
    });
})();
