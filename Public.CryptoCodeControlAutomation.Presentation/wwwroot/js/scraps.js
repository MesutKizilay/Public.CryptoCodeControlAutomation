(function () {
    "use strict";

    const tbInput = document.getElementById("tb-no-input");
    const plannedOrderInput = document.getElementById("planned-order-no");
    const plannedOrderMessage = document.getElementById("planned-order-message");
    const rawScanDisplay = document.getElementById("raw-scan-display");

    const codeInput = document.getElementById("scraps-code-input");
    const addButton = document.getElementById("scraps-add-btn");
    const fireButton = document.getElementById("scraps-fire-btn");
    const tableBody = document.querySelector("#scraps-table tbody");
    const countBadge = document.getElementById("scraps-code-count");
    const existingCodes = new Set();
    const allowedCodes = new Map();
    let allowedCodesLoaded = false;
    let scrapsScannerSource = null;

    if (!codeInput || !addButton || !tableBody) return;
    if (tbInput) {
        tbInput.focus();
    }

    const setMessage = (text, isError) => {
        if (!plannedOrderMessage) return;
        plannedOrderMessage.textContent = text || "";
        plannedOrderMessage.classList.remove("text-danger", "text-success");
        if (text) plannedOrderMessage.classList.add(isError ? "text-danger" : "text-success");
    };

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

    const addRow = (value, codeIdOverride) => {
        const code = normalizeCode(value).trim();
        //const code = value.trim();
        if (!code) return;

        if (existingCodes.has(code)) {
            Toast?.fire({ icon: "warning", title: "Bu kod zaten eklendi." });
            return;
        }

        if (!allowedCodesLoaded) {
            Toast?.fire({ icon: "warning", title: "Planlı sipariş kodları yüklenmedi." });
            return;
        }

        const codeId = codeIdOverride ?? allowedCodes.get(code);
        if (!codeId) {
            Toast?.fire({ icon: "error", title: "Bu kod planlı siparişte bulunamadı." });
            return;
        }

        clearEmptyRow();

        const row = document.createElement("tr");
        row.dataset.codeId = codeId;
        row.innerHTML = `
            <td>${code}</td>
            <td>${new Date().toLocaleString("tr-TR")}</td>
            <td><button type="button" class="btn btn-sm btn-outline-danger scraps-remove-btn">Sil</button></td>
        `;
        tableBody.prepend(row);
        existingCodes.add(code);
        updateCount();
    };

    const addCodesFromPlannedOrder = (items) => {

        tableBody.innerHTML = "";
        existingCodes.clear();
        ensureEmptyRow();
        updateCount();

        if (!Array.isArray(items)) return;
        items.forEach((item) => {
            const value = (item?.codeValue ?? item?.CodeValue ?? "").toString().trim();
            const idValue = item?.codeId ?? item?.CodeId;
            const codeId = Number(idValue);
            if (value && Number.isFinite(codeId)) {
                addRow(value, codeId);
            }
        });
    };

    const canAddCode = () => {
        const planned = plannedOrderInput ? plannedOrderInput.value.trim() : "";
        if (!planned) {
            Toast?.fire({ icon: "warning", title: "Önce planlı sipariş numarasını alınız." });
            return false;
        }
        return true;
    };

    const processScannedCode = (rawCode) => {
        if (!canAddCode()) return false;

        const code = normalizeCode(rawCode).trim();
        if (!code) return false;

        if (rawScanDisplay) {
            rawScanDisplay.textContent = code;
        }

        addRow(code);
        return true;
    };

    const handleAdd = () => {
        if (!processScannedCode(codeInput.value)) return;

        codeInput.value = "";
        codeInput.focus();
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

    const startScrapsScanner = async () => {
        try {
            const response = await fetch("/Codes/StartScrapsScanner", {
                method: "POST"
            });

            if (!response.ok) {
                throw new Error(await getResponseMessage(response));
            }

            scrapsScannerSource = new EventSource("/Codes/ScrapsScannerStream");

            scrapsScannerSource.addEventListener("code", (event) => {
                const message = JSON.parse(event.data);
                const value = message?.value ?? "";

                codeInput.value = value;
                processScannedCode(value);
                codeInput.value = "";
                codeInput.focus();
            });

            scrapsScannerSource.onerror = () => {
                console.warn("Scraps scanner bağlantısı yeniden kurulmaya çalışılıyor.");
            };
        }
        catch (error) {
            Toast?.fire({
                icon: "error",
                title: error?.message || "Endüstriyel el terminali bağlantısı başlatılamadı."
            });
        }
    };

    if (tbInput) {
        tbInput.addEventListener("input", () => {
            if (plannedOrderInput) plannedOrderInput.value = "";
            setMessage("", false);
            //allowedCodes.clear();
            // allowedCodesLoaded = false;
            // tableBody.innerHTML = "";
            // existingCodes.clear();
            // ensureEmptyRow();
        });

        tbInput.addEventListener("keydown", (event) => {
            if (event.key !== "Enter") return;
            event.preventDefault();

            const tbNo = tbInput.value.trim();
            if (!tbNo) {
                if (plannedOrderInput) plannedOrderInput.value = "";
                setMessage("TB no giriniz.", true);
                allowedCodes.clear();
                allowedCodesLoaded = false;
                return;
            }

            setMessage("Sorgulanıyor...", false);

            $.ajax({
                url: "/Codes/GetPlannedOrderByPalletNumber",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ tbNo: tbNo }),
                success: function (res) {
                    if (res && res.success) {
                        const plannedOrderNo = res.plannedOrderNo || "";
                        if (plannedOrderInput) plannedOrderInput.value = plannedOrderNo;
                        //allowedCodes.clear();
                        //allowedCodesLoaded = false;

                        if (plannedOrderNo) {
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
                                    allowedCodesLoaded = false;
                                    tableBody.innerHTML = "";
                                    existingCodes.clear();
                                    ensureEmptyRow();
                                    updateCount();
                                    if (Array.isArray(items)) {
                                        items.forEach((item) => {
                                            const value = (item?.codeValue ?? "").toString().trim();
                                            const idValue = item?.codeId;
                                            const codeId = Number(idValue);
                                            if (value && Number.isFinite(codeId)) {
                                                allowedCodes.set(value, codeId);
                                            }
                                        });
                                    }
                                    allowedCodesLoaded = true;
                                    codeInput.focus();
                                    Toast?.fire({ icon: "success", title: "Okutulacak kodlar terminale yüklendi." });
                                    if (shouldCloseSwal) Swal.close();
                                    // İstemiyorsan bu satırı yorum satırı yap.
                                    //addCodesFromPlannedOrder(items);
                                },
                                error: function (xhr) {
                                    //Toast?.fire({ icon: "error", title: "Kod listesi alınamadı." });
                                    parseErrorResponse?.(xhr);
                                }
                            });
                        }
                        //Toast?.fire({ icon: "success", title: res.message || "Planlı sipariş bulundu." });
                        //Toast?.fire({ icon: "success", title: "Kodlar yüklendi." });
                        setMessage("", false);
                    }
                    else {
                        if (plannedOrderInput) plannedOrderInput.value = "";
                        allowedCodes.clear();
                        allowedCodesLoaded = false;
                        Toast?.fire({ icon: "error", title: res?.message || "Planlı sipariş bulunamadı." });
                    }
                },
                error: function (xhr) {
                    if (plannedOrderInput) plannedOrderInput.value = "";
                    setMessage("Servis hatası oluştu.", true);
                    allowedCodes.clear();
                    allowedCodesLoaded = false;
                    parseErrorResponse?.(xhr);
                }
            });
        });
    }

    if (plannedOrderInput) {
        plannedOrderInput.addEventListener("input", () => {
            //allowedCodes.clear();
            // allowedCodesLoaded = false;
            // tableBody.innerHTML = "";
            // existingCodes.clear();
            // ensureEmptyRow();
        });

        plannedOrderInput.addEventListener("keydown", (event) => {
            if (event.key !== "Enter") return;
            event.preventDefault();

            const plannedOrderNo = normalizePlannedOrderNo(plannedOrderInput.value);
            if (!plannedOrderNo) {
                Toast?.fire({ icon: "warning", title: "Planlı sipariş no giriniz." });
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
                    allowedCodesLoaded = false;
                    tableBody.innerHTML = "";
                    existingCodes.clear();
                    ensureEmptyRow();
                    updateCount();
                    if (Array.isArray(items)) {
                        items.forEach((item) => {
                            const value = (item?.codeValue ?? "").toString().trim();
                            const idValue = item?.codeId;
                            const codeId = Number(idValue);
                            if (value && Number.isFinite(codeId)) {
                                allowedCodes.set(value, codeId);
                            }
                        });
                    }
                    allowedCodesLoaded = true;
                    codeInput.focus();
                    Toast?.fire({ icon: "success", title: "Okutulacak kodlar terminale yüklendi." });
                    //if (shouldCloseSwal) Swal.close();
                    // İstemiyorsan bu satırı yorum satırı yap.
                    //addCodesFromPlannedOrder(items);
                },
                error: function (xhr) {
                    parseErrorResponse?.(xhr);
                }
            });
        });
    }

    addButton.addEventListener("click", (event) => {
        event.preventDefault();
        handleAdd();
    });

    codeInput.addEventListener("keydown", (event) => {
        if (event.key !== "Enter") return;
        event.preventDefault();
        handleAdd();
    });

    if (fireButton) {
        fireButton.addEventListener("click", (event) => {
            event.preventDefault();

            //Eski yöntem (tabloda görünen satırlardan ID toplama):
            const ids = Array.from(tableBody.querySelectorAll("tr"))
                .filter((row) => !row.classList.contains("text-muted"))
                .map((row) => Number(row.dataset.codeId))
                .filter((id) => Number.isFinite(id));

            //const ids = Array.from(allowedCodes.values())
            //    .map((id) => Number(id))
            //    .filter((id) => Number.isFinite(id));

            if (ids.length === 0) {
                Toast?.fire({ icon: "warning", title: "Fireye gönderilecek kod bulunamadı." });
                return;
            }

            $.ajax({
                url: "/Codes/ScrapCodes",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ codeIds: ids }),
                success: function (res) {
                    tableBody.innerHTML = "";
                    existingCodes.clear();
                    ensureEmptyRow();
                    updateCount();

                    const count = res?.updatedCount;
                    Toast?.fire({ icon: "success", title: `Fireye alınan kod sayısı: ${count}` });
                },
                error: function (xhr) {
                    parseErrorResponse?.(xhr);
                }
            });
        });
    }

    tableBody.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) return;
        if (!target.classList.contains("scraps-remove-btn")) return;

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
    startScrapsScanner();

    window.addEventListener("beforeunload", () => {
        scrapsScannerSource?.close();
    });
})();
