(function () {
    "use strict";

    const plannedOrderInput = document.getElementById("recover-planned-order-no");
    const codeInput = document.getElementById("recover-code-input");
    const submitButton = document.getElementById("recover-submit-btn");
    const tableBody = document.querySelector("#recover-table tbody");
    const countBadge = document.getElementById("recover-code-count");

    const allowedCodes = new Map();
    const existingCodes = new Set();
    let recoverScannerSource = null;

    if (!plannedOrderInput || !codeInput || !submitButton || !tableBody) return;

    const normalizeCode = (value) => {
        return (value || "").replace(/\|9(1|2|3)/g, "\u001D9$1");
    };

    const normalizePlannedOrderNo = (value) => {
        const plannedOrderNo = (value || "").trim();
        return /^\d+$/.test(plannedOrderNo)
            ? plannedOrderNo.padStart(8, "0")
            : plannedOrderNo;
    };

    const ensureEmptyRow = () => {
        if (tableBody.children.length === 0) {
            const row = document.createElement("tr");
            row.className = "text-muted";
            row.innerHTML = "<td colspan=\"3\">Henüz kayıt yok.</td>";
            tableBody.appendChild(row);
        }
    };

    const clearEmptyRow = () => {
        if (tableBody.children.length === 1 && tableBody.firstElementChild?.classList.contains("text-muted")) {
            tableBody.innerHTML = "";
        }
    };

    const updateCount = () => {
        if (!countBadge) return;
        const count = tableBody.querySelectorAll("tr:not(.text-muted)").length;
        countBadge.textContent = count.toString();
    };

    const addRow = (code, codeId) => {
        const value = normalizeCode(code).trim();
        if (!value) return;

        if (allowedCodes.size == 0) {
            Toast?.fire({ icon: "warning", title: "Lütfen planlı siparişi okutarak kodları getiriniz." });
            return;
        }

        if (existingCodes.has(value)) {
            Toast?.fire({ icon: "warning", title: "Bu kod zaten eklendi." });
            return;
        }

        if (!codeId) {
            Toast?.fire({ icon: "error", title: "Bu kod planlı siparişte bulunamadı." });
            return;
        }

        clearEmptyRow();

        const row = document.createElement("tr");
        row.dataset.codeId = codeId;
        row.innerHTML = `
            <td>${$('<div>').text(code).html()}</td>
            <td>${new Date().toLocaleString("tr-TR")}</td>
            <td><button type="button" class="btn btn-sm btn-outline-danger recover-remove-btn">Sil</button></td>
        `;
        tableBody.prepend(row);
        existingCodes.add(value);
        updateCount();
    };

    const processScannedCode = (rawCode) => {
        const code = normalizeCode(rawCode).trim();
        if (!code) return;

        const codeId = allowedCodes.get(code);
        addRow(code, codeId);
    };

    const getResponseMessage = async (response) => {
        try {
            const body = await response.json();
            return body?.message || "Endüstriyel el terminali bağlantısı başlatılamadı.";
        }
        catch {
            return "Endüstriyel el terminali bağlantısı başlatılamadı.";
        }
    };

    const startRecoverScanner = async () => {
        try {
            const response = await fetch("/Codes/StartRecoverScanner", {
                method: "POST"
            });

            if (!response.ok) {
                throw new Error(await getResponseMessage(response));
            }

            recoverScannerSource = new EventSource("/Codes/RecoverScannerStream");

            recoverScannerSource.addEventListener("code", (event) => {
                const message = JSON.parse(event.data);
                const value = message?.value ?? "";

                codeInput.value = value;
                processScannedCode(value);
                codeInput.value = "";
                codeInput.focus();
            });

            recoverScannerSource.onerror = () => {
                console.warn("Recover scanner bağlantısı yeniden kurulmaya çalışılıyor.");
            };
        }
        catch (error) {
            Toast?.fire({
                icon: "error",
                title: error?.message || "Endüstriyel el terminali bağlantısı başlatılamadı."
            });
        }
    };

    plannedOrderInput.addEventListener("keydown", (event) => {
        if (event.key !== "Enter") return;
        event.preventDefault();

        const plannedOrderNo = normalizePlannedOrderNo(plannedOrderInput.value);
        if (!plannedOrderNo) {
            Toast?.fire({ icon: "warning", title: "Planlı sipariş numarasını giriniz." });
            return;
        }

        plannedOrderInput.value = plannedOrderNo;

        const shouldCloseSwal = !!window.Swal;
        if (shouldCloseSwal) {
            Swal.fire({
                title: "Planlı siparişe ait kodlar getiriliyor",
                text: "Lütfen bekleyiniz...",
                allowOutsideClick: false,
                allowEscapeKey: false,
                showConfirmButton: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
        }

        $.ajax({
            url: "/Codes/GetListCodesByPlannedOrderId",
            type: "GET",
            data: { plannedOrderNo: plannedOrderNo },
            success: function (items) {
                allowedCodes.clear();
                tableBody.innerHTML = "";
                existingCodes.clear();
                ensureEmptyRow();
                updateCount();
                if (Array.isArray(items)) {
                    items.forEach((item) => {
                        const value = normalizeCode((item?.codeValue ?? "").toString()).trim();
                        const idValue = item?.codeId;
                        const codeId = Number(idValue);
                        if (value && Number.isFinite(codeId)) {
                            allowedCodes.set(value, codeId);
                        }
                    });
                }
                codeInput.focus();
                Toast?.fire({ icon: "success", title: "Okutulacak kodlar terminale yüklendi." });
                //if (shouldCloseSwal) Swal.close();
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
                //if (shouldCloseSwal) Swal.close(); 
            }
        });
    });

    codeInput.addEventListener("keydown", (event) => {
        if (event.key !== "Enter") return;
        event.preventDefault();

        processScannedCode(codeInput.value);
        codeInput.value = "";
        codeInput.focus();
    });

    submitButton.addEventListener("click", (event) => {
        event.preventDefault();

        const ids = Array.from(tableBody.querySelectorAll("tr"))
            .filter((row) => !row.classList.contains("text-muted"))
            .map((row) => Number(row.dataset.codeId))
            .filter((id) => Number.isFinite(id));

        if (ids.length === 0) {
            Toast?.fire({ icon: "warning", title: "Üretime kazandırılacak kod bulunamadı." });
            return;
        }

        $.ajax({
            url: "/Codes/RecoverCodes",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ codeIds: ids }),
            success: function (res) {
                tableBody.innerHTML = "";
                existingCodes.clear();
                ensureEmptyRow();
                updateCount();

                const count = res?.updatedCount;
                Toast?.fire({ icon: "success", title: `Üretime kazandırılan kod sayısı: ${count}` });
            },
            error: function (xhr) {
                parseErrorResponse?.(xhr);
            }
        });
    });

    tableBody.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) return;
        if (!target.classList.contains("recover-remove-btn")) return;

        const row = target.closest("tr");
        if (row) {
            const codeCell = row.querySelector("td");
            const codeValue = codeCell ? codeCell.textContent?.trim() : "";
            if (codeValue) existingCodes.delete(codeValue);
            row.remove();
        }
        ensureEmptyRow();
        updateCount();
    });

    updateCount();
    startRecoverScanner();

    window.addEventListener("beforeunload", () => {
        recoverScannerSource?.close();
    });
})();
