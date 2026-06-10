(function () {
    "use strict";

    const portInput = document.getElementById("moxa-listen-port");
    const startButton = document.getElementById("moxa-start-btn");
    const stopButton = document.getElementById("moxa-stop-btn");
    const listenerStatus = document.getElementById("moxa-listener-status");
    const streamStatus = document.getElementById("moxa-stream-status");
    const codeInput = document.getElementById("moxa-code-input");
    const codeMeta = document.getElementById("moxa-code-meta");
    const testValueInput = document.getElementById("moxa-test-value");
    const testSendButton = document.getElementById("moxa-test-send-btn");
    const tableBody = document.getElementById("moxa-message-table-body");
    const messageCount = document.getElementById("moxa-message-count");

    if (!portInput || !startButton || !stopButton || !codeInput || !tableBody) return;

    let receivedCount = 0;

    const setBadge = (element, text, className) => {
        if (!element) return;
        element.textContent = text;
        element.className = `badge ${className}`;
    };

    const readErrorMessage = async (response) => {
        try {
            const body = await response.json();
            return body?.message || "İşlem başarısız oldu.";
        }
        catch {
            return "İşlem başarısız oldu.";
        }
    };

    const postJson = async (url, body) => {
        const response = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body || {})
        });

        if (!response.ok)
            throw new Error(await readErrorMessage(response));

        if (response.status === 204)
            return null;

        const contentType = response.headers.get("content-type") || "";
        return contentType.includes("application/json") ? response.json() : null;
    };

    const refreshStatus = async () => {
        try {
            const response = await fetch("/Test2/Status", { cache: "no-store" });
            const status = await response.json();

            if (status.isRunning) {
                setBadge(listenerStatus, `${status.port} portu dinleniyor`, "bg-success");
                portInput.value = status.port;
            }
            else {
                setBadge(listenerStatus, "Durduruldu", "bg-secondary");
            }
        }
        catch {
            setBadge(listenerStatus, "Durum alınamadı", "bg-danger");
        }
    };

    const appendMessage = (message) => {
        if (tableBody.firstElementChild?.classList.contains("text-muted"))
            tableBody.innerHTML = "";

        const row = document.createElement("tr");
        const valueCell = document.createElement("td");
        const sourceCell = document.createElement("td");
        const dateCell = document.createElement("td");

        valueCell.textContent = message.value || "";
        sourceCell.textContent = message.source || "";
        dateCell.textContent = message.receivedAt
            ? new Date(message.receivedAt).toLocaleString("tr-TR")
            : new Date().toLocaleString("tr-TR");

        row.append(valueCell, sourceCell, dateCell);
        tableBody.prepend(row);

        receivedCount++;
        if (messageCount) messageCount.textContent = receivedCount.toString();
    };

    const eventSource = new EventSource("/Test2/Stream");

    eventSource.onopen = () => {
        setBadge(streamStatus, "Bağlı", "bg-success");
    };

    eventSource.onerror = () => {
        setBadge(streamStatus, "Yeniden bağlanıyor", "bg-warning");
    };

    eventSource.addEventListener("code", (event) => {
        const message = JSON.parse(event.data);

        codeInput.value = message.value || "";
        codeInput.dispatchEvent(new Event("input", { bubbles: true }));

        if (codeMeta) {
            const receivedAt = message.receivedAt
                ? new Date(message.receivedAt).toLocaleString("tr-TR")
                : new Date().toLocaleString("tr-TR");
            codeMeta.textContent = `Kaynak: ${message.source || "-"} | Zaman: ${receivedAt}`;
        }

        appendMessage(message);
    });

    startButton.addEventListener("click", async () => {
        const port = Number(portInput.value);

        if (!Number.isInteger(port) || port < 1 || port > 65535) {
            Toast?.fire({ icon: "warning", title: "Geçerli bir port giriniz." });
            return;
        }

        try {
            const result = await postJson("/Test2/Start", { port });
            Toast?.fire({ icon: "success", title: result?.message || "Dinleyici başlatıldı." });
            await refreshStatus();
        }
        catch (error) {
            Toast?.fire({ icon: "error", title: error.message });
        }
    });

    stopButton.addEventListener("click", async () => {
        try {
            const result = await postJson("/Test2/Stop");
            Toast?.fire({ icon: "success", title: result?.message || "Dinleyici durduruldu." });
            await refreshStatus();
        }
        catch (error) {
            Toast?.fire({ icon: "error", title: error.message });
        }
    });

    testSendButton?.addEventListener("click", async () => {
        const value = testValueInput?.value.trim() || "";

        if (!value) {
            Toast?.fire({ icon: "warning", title: "Test verisi giriniz." });
            return;
        }

        try {
            await postJson("/Test2/PublishTestMessage", { value });
        }
        catch (error) {
            Toast?.fire({ icon: "error", title: error.message });
        }
    });

    window.addEventListener("beforeunload", () => eventSource.close());
    refreshStatus();
})();
