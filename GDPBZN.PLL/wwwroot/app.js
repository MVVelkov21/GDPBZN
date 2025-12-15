/* ============================
   GDPBZN • Operative Center UI
   Robust client for backend DTO mismatches
   ============================ */

let token = null;
let user = null;
let hub = null;

let incidents = [];
let selectedIncidentId = null;
let joinedIncidentId = null;

const $ = (id) => document.getElementById(id);

/* ---------- helpers ---------- */
function escapeHtml(s) {
    return String(s ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function fmtUtc(iso) {
    try { return new Date(iso).toLocaleString(); } catch { return String(iso ?? ""); }
}

function avatarFromName(name) {
    if (!name) return "?";
    const parts = name.trim().split(/\s+/);
    const a = parts[0]?.[0] ?? "?";
    const b = parts[1]?.[0] ?? "";
    return (a + b).toUpperCase();
}

function setConn(state, text) {
    $("connText").textContent = text;
    const dot = $("connDot");
    dot.style.background =
        state === "ok" ? "var(--ok)" :
            state === "warn" ? "var(--warn)" :
                state === "danger" ? "var(--danger)" :
                    "#666";
}

function setHint(id, msg, kind = "muted") {
    const el = $(id);
    el.textContent = msg || "";
    el.style.color =
        kind === "danger" ? "var(--danger)" :
            kind === "ok" ? "var(--ok)" :
                kind === "warn" ? "var(--warn)" :
                    "var(--muted)";
}

function enableAuthedUI(enabled) {
    $("btnLogout").disabled = !enabled;
    $("btnRefresh").disabled = !enabled;
    $("btnNewIncident").disabled = !enabled;
    $("search").disabled = !enabled;
    $("statusFilter").disabled = !enabled;
    $("btnJoin").disabled = !enabled;
    $("btnLeave").disabled = !enabled;

    $("chatText").disabled = !enabled || !selectedIncidentId;
    $("btnSendChat").disabled = !enabled || !selectedIncidentId;

    $("btnNewTask").disabled = !enabled || !selectedIncidentId;
}

function showModal(id, show) {
    $(id).classList.toggle("show", !!show);
}

/* ---------- enum mappings (int) ---------- */
/**
 * NOTE: adjust numbers if your server enum differs.
 * Common default is 0..N in declaration order.
 */
const INCIDENT_TYPE = {
    Fire: 0,
    Rescue: 1,
    Hazmat: 2,
    TrafficAccident: 3,
    Flood: 4,
    Other: 5
};

const TASK_TYPE = {
    Operational: 0,
    Logistic: 1,
    Administrative: 2,
    Other: 3
};

/* ---------- API core ---------- */
async function apiRaw(path, options = {}) {
    const headers = options.headers || {};
    headers["Content-Type"] = "application/json";
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const res = await fetch(path, { ...options, headers });
    const text = await res.text();
    let body = null;
    try { body = text ? JSON.parse(text) : null; } catch { body = text; }

    if (!res.ok) {
        const err = new Error("HTTP " + res.status);
        err.status = res.status;
        err.body = body;
        err.rawText = text;
        throw err;
    }
    return body;
}

function errorText(e) {
    const b = e?.body;
    if (!b) return e?.message || "Unknown error";

    if (typeof b === "string") return b;

    // ASP.NET validation problem details pattern:
    // { title, status, errors: { field: [msg...] } }
    if (b.errors && typeof b.errors === "object") {
        const lines = [];
        for (const [k, arr] of Object.entries(b.errors)) {
            const msg = Array.isArray(arr) ? arr.join(" | ") : String(arr);
            lines.push(`${k}: ${msg}`);
        }
        return `${b.title || "Validation error"}\n` + lines.join("\n");
    }

    return JSON.stringify(b, null, 2);
}

function looksLikeReqWrapperNeeded(e) {
    const t = (e?.rawText || "").toLowerCase();
    return t.includes("the req field is required");
}

function looksLikeEnumConversionIssue(e) {
    const t = (e?.rawText || "").toLowerCase();
    return t.includes("could not be converted") && (t.includes("$.type") || t.includes("$.status"));
}

/**
 * Robust POST:
 * - tries body as-is
 * - if server expects { req: ... }, retries with wrapper
 */
async function postRobust(path, payload) {
    try {
        return await apiRaw(path, { method: "POST", body: JSON.stringify(payload) });
    } catch (e1) {
        if (e1.status === 400 && (looksLikeReqWrapperNeeded(e1))) {
            // retry with { req: payload }
            return await apiRaw(path, { method: "POST", body: JSON.stringify({ req: payload }) });
        }
        // some backends do opposite: they expect direct body but we sent wrapper elsewhere
        throw e1;
    }
}

/**
 * Robust POST that can also try enum as string if int fails (rare)
 */
async function postRobustEnum(path, payload, enumFieldsAsStringFallback = false) {
    try {
        return await postRobust(path, payload);
    } catch (e1) {
        // If enum conversion error and fallback enabled, convert enum ints to strings if possible
        if (e1.status === 400 && looksLikeEnumConversionIssue(e1) && enumFieldsAsStringFallback) {
            const clone = JSON.parse(JSON.stringify(payload));
            // try to map ints back to names for known fields
            if (typeof clone.type === "number") {
                const name = Object.keys(INCIDENT_TYPE).find(k => INCIDENT_TYPE[k] === clone.type);
                if (name) clone.type = name;
            }
            if (typeof clone.taskType === "number") {
                const name = Object.keys(TASK_TYPE).find(k => TASK_TYPE[k] === clone.taskType);
                if (name) clone.taskType = name;
            }
            return await postRobust(path, clone);
        }
        throw e1;
    }
}

/* ---------- UI rendering ---------- */
function badgeForStatus(status) {
    const s = String(status);
    const cls =
        (s === "Resolved") ? "ok" :
            (s === "InProgress" || s === "OnScene") ? "warn" :
                (s === "Cancelled") ? "danger" :
                    "";
    return `<span class="badge ${cls}">${escapeHtml(s)}</span>`;
}

function renderIncidents() {
    const q = $("search").value.trim().toLowerCase();
    const status = $("statusFilter").value;

    const list = (incidents || []).filter(i => {
        const hay = `${i.id} ${i.addressText ?? ""}`.toLowerCase();
        const okQ = !q || hay.includes(q);
        const okS = !status || String(i.status) === status;
        return okQ && okS;
    });

    const root = $("incidentList");
    root.innerHTML = "";

    if (list.length === 0) {
        root.innerHTML = `<div class="empty">Няма данни. Създай нов инцидент.</div>`;
        return;
    }

    for (const i of list) {
        const isActive = i.id === selectedIncidentId;
        const el = document.createElement("div");
        el.className = `item ${isActive ? "active" : ""}`;
        el.onclick = () => selectIncident(i.id);

        el.innerHTML = `
      <div class="item-top">
        <div class="item-title">#${i.id} • ${escapeHtml(i.type)}</div>
        ${badgeForStatus(i.status)}
      </div>
      <div class="item-sub">${escapeHtml(i.addressText ?? "")}</div>
      <div class="item-meta">created: ${escapeHtml(fmtUtc(i.createdAtUtc))}</div>
    `;
        root.appendChild(el);
    }
}

function clearDetails() {
    $("detailsSubtitle").textContent = "Избери инцидент от списъка";
    $("kv").innerHTML = "";
    $("units").innerHTML = "";
    $("hazards").innerHTML = "";
    $("resources").innerHTML = "";
    $("annotations").innerHTML = "";
    $("gpsFeed").innerHTML = "";
    $("sosFeed").innerHTML = "";
    $("chatLog").innerHTML = "";
    $("taskList").innerHTML = "";
    selectedIncidentId = null;
    enableAuthedUI(!!token);
}

function kvRow(key, val) {
    return `
    <div class="k">
      <div class="key">${escapeHtml(key)}</div>
      <div class="val">${escapeHtml(val ?? "—")}</div>
    </div>
  `;
}

function renderChat(msgs) {
    const log = $("chatLog");
    log.innerHTML = msgs.length ? "" : `<div class="empty">Няма съобщения.</div>`;
    for (const m of msgs) {
        const el = document.createElement("div");
        el.className = "msg";
        el.innerHTML = `
      <div class="msg-head">
        <div class="msg-from">${escapeHtml(m.senderName || "System")}</div>
        <div class="msg-time">${escapeHtml(fmtUtc(m.sentAtUtc))}</div>
      </div>
      <div class="msg-body">${escapeHtml(m.text)}</div>
    `;
        log.appendChild(el);
    }
    log.scrollTop = log.scrollHeight;
}

function renderTasks(tasks) {
    const list = $("taskList");
    list.innerHTML = tasks.length ? "" : `<div class="empty">Няма задачи.</div>`;
    for (const t of tasks) {
        const el = document.createElement("div");
        el.className = "mini";
        el.innerHTML = `<div><strong>${escapeHtml(t.title)}</strong> • ${escapeHtml(t.type)} • ${escapeHtml(t.status)}</div>
                    <div class="muted">${escapeHtml(t.details ?? "")}</div>`;
        list.appendChild(el);
    }
}

function renderDetails(d) {
    $("detailsSubtitle").textContent = `#${d.id} • ${d.type} • ${d.status}`;

    $("kv").innerHTML =
        kvRow("CreatedAtUtc", d.createdAtUtc) +
        kvRow("SourceChannel", d.sourceChannel) +
        kvRow("Address", d.addressText) +
        kvRow("Coords", `${d.lat ?? "—"}, ${d.lng ?? "—"}`) +
        kvRow("Description", d.description ?? "—");

    // Units
    const units = $("units");
    units.innerHTML = "";
    if (!d.units?.length) units.innerHTML = `<div class="empty">Няма екипи.</div>`;
    else {
        for (const u of d.units) {
            const members = (u.members ?? [])
                .map(m => `${m.fullName} ${m.acknowledged ? "✅" : "⏳"}`)
                .join(", ");

            const chip = document.createElement("div");
            chip.className = "chip";
            chip.innerHTML = `<strong>${escapeHtml(u.callSign)}</strong> <span class="muted">(${u.vehicleId})</span><div class="muted">${escapeHtml(members || "—")}</div>`;
            units.appendChild(chip);
        }
    }

    // Hazards
    const hz = $("hazards");
    hz.innerHTML = "";
    if (!d.hazards?.length) hz.innerHTML = `<div class="empty">Няма опасни вещества.</div>`;
    else {
        for (const h of d.hazards) {
            const chip = document.createElement("div");
            chip.className = "chip";
            chip.innerHTML = `<strong>UN ${escapeHtml(h.unNumber)}</strong> <span class="muted">${escapeHtml(h.name)}</span>`;
            hz.appendChild(chip);
        }
    }

    // Resources
    const res = $("resources");
    res.innerHTML = "";
    if (!d.resources?.length) res.innerHTML = `<div class="empty">Няма ресурсни заявки.</div>`;
    else {
        for (const r of d.resources) {
            const el = document.createElement("div");
            el.className = "mini";
            el.innerHTML = `<div><strong>${escapeHtml(r.resourceName)}</strong> x${escapeHtml(r.quantity)} • ${escapeHtml(r.status)}</div>
                      <div class="muted">${escapeHtml(r.notes ?? "")}</div>`;
            res.appendChild(el);
        }
    }

    // Annotations
    const ann = $("annotations");
    ann.innerHTML = "";
    if (!d.annotations?.length) ann.innerHTML = `<div class="empty">Няма анотации.</div>`;
    else {
        for (const a of d.annotations) {
            const el = document.createElement("div");
            el.className = "mini";
            el.innerHTML = `<div><strong>${escapeHtml(a.kind)}</strong> • ${escapeHtml(fmtUtc(a.createdAtUtc))}</div>
                      <div class="muted">${escapeHtml(a.text ?? "")}</div>`;
            ann.appendChild(el);
        }
    }

    renderTasks(d.tasks || []);
    renderChat(d.chat || []);

    $("chatText").disabled = !token || !selectedIncidentId;
    $("btnSendChat").disabled = !token || !selectedIncidentId;
    $("btnNewTask").disabled = !token || !selectedIncidentId;
}

/* ---------- load data ---------- */
async function loadIncidents() {
    incidents = await apiRaw("/api/incidents");
    incidents = incidents || [];
    renderIncidents();
}

async function selectIncident(id) {
    selectedIncidentId = id;
    renderIncidents();
    const d = await apiRaw(`/api/incidents/${id}`);
    renderDetails(d);
    enableAuthedUI(!!token);
}

/* ---------- SignalR ---------- */
async function startHub() {
    if (!token) return;
    if (!window.signalR) return;
    if (hub) return;

    hub = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/incident", { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

    hub.onreconnecting(() => setConn("warn", "Reconnecting..."));
    hub.onreconnected(() => setConn("ok", "Online"));
    hub.onclose(() => { setConn("danger", "Offline"); hub = null; });

    // minimal events
    hub.on("incident.created", async () => { try { await loadIncidents(); } catch {} });
    hub.on("task.created", async (p) => { if (p?.incidentId === selectedIncidentId) { try { await selectIncident(selectedIncidentId); } catch {} } });
    hub.on("unit.ack", async (p) => { if (p?.incidentId === selectedIncidentId) { try { await selectIncident(selectedIncidentId); } catch {} } });
    hub.on("chat.new", async (p) => {
        if (p?.incidentId === selectedIncidentId) {
            // append quickly
            const log = $("chatLog");
            if (log.querySelector(".empty")) log.innerHTML = "";
            const el = document.createElement("div");
            el.className = "msg";
            el.innerHTML = `
        <div class="msg-head">
          <div class="msg-from">${escapeHtml(p.senderName || "System")}</div>
          <div class="msg-time">${escapeHtml(fmtUtc(p.sentAtUtc))}</div>
        </div>
        <div class="msg-body">${escapeHtml(p.text)}</div>
      `;
            log.appendChild(el);
            log.scrollTop = log.scrollHeight;
        }
    });

    try {
        await hub.start();
        setConn("ok", "Online");
    } catch {
        setConn("danger", "Offline");
    }
}

/* ---------- LOGIN (robust) ---------- */
async function login() {
    setHint("loginStatus", "Logging in...");
    try {
        const res = await apiRaw("/api/auth/login", {
            method: "POST",
            body: JSON.stringify({
                username: $("username").value.trim(),
                password: $("password").value
            })
        });

        // token field variants
        token = res?.token ?? res?.accessToken ?? res?.jwt ?? res?.bearerToken ?? res?.access_token ?? null;
        if (!token) throw new Error("Login response missing token (token/accessToken/jwt).");

        const employeeId = res.employeeId ?? res.id ?? res.userId ?? null;
        const fullName = res.fullName ?? res.name ?? res.displayName ?? "User";
        const role = res.role ?? res.userRole ?? "—";

        user = { id: employeeId, name: fullName, role };

        $("userName").textContent = user.name;
        $("userRole").textContent = user.role;
        $("userAvatar").textContent = avatarFromName(user.name);

        enableAuthedUI(true);
        setHint("loginStatus", `OK. Logged as ${user.name} (id=${user.id ?? "?"}).`, "ok");

        await startHub();
        await loadIncidents();
    } catch (e) {
        console.error(e);
        token = null;
        user = null;
        enableAuthedUI(false);
        setConn("danger", "Offline");
        setHint("loginStatus", "Login failed: " + errorText(e), "danger");

        $("userName").textContent = "Not signed in";
        $("userRole").textContent = "—";
        $("userAvatar").textContent = "?";
    }
}

/* ---------- Create Incident (robust) ---------- */
async function createIncident() {
    setHint("newIncidentStatus", "Creating...");
    try {
        const lat = $("incLat").value.trim();
        const lng = $("incLng").value.trim();

        const typeName = $("incType").value;
        const payload = {
            // send int by default (most DTOs use enum)
            type: INCIDENT_TYPE[typeName] ?? 0,
            addressText: $("incAddress").value.trim(),
            lat: lat ? Number(lat) : null,
            lng: lng ? Number(lng) : null,
            description: $("incDesc").value.trim() || null,
            sourceChannel: $("incSource").value.trim() || "112",
            fireStationId: Number($("incStationId").value.trim() || "1"),
            shiftId: $("incShiftId").value.trim() ? Number($("incShiftId").value.trim()) : null,
            vehicleIds: null
        };

        // try robust create; if enum conversion errors and server expects string, enable fallback
        const res = await postRobustEnum("/api/incidents", payload, true);

        const id = res?.incidentId ?? res?.id ?? res;
        setHint("newIncidentStatus", `Created IncidentId=${id ?? "?"}`, "ok");
        showModal("modalIncident", false);

        await loadIncidents();
    } catch (e) {
        console.error(e);
        setHint("newIncidentStatus", errorText(e), "danger");
    }
}

/* ---------- Create Task (robust) ---------- */
async function createTask() {
    if (!selectedIncidentId) return;
    setHint("newTaskStatus", "Creating...");
    try {
        const typeName = $("taskType").value;

        const payload = {
            incidentId: selectedIncidentId,
            title: $("taskTitle").value.trim(),
            details: $("taskDetails").value.trim() || null,
            type: TASK_TYPE[typeName] ?? 0,
            assignVehicleIds: null,
            assignEmployeeIds: null
        };

        const res = await postRobustEnum("/api/incidents/tasks", payload, true);
        const id = res?.taskId ?? res?.id ?? res;

        setHint("newTaskStatus", `Created TaskId=${id ?? "?"}`, "ok");
        showModal("modalTask", false);

        await selectIncident(selectedIncidentId);
    } catch (e) {
        console.error(e);
        setHint("newTaskStatus", errorText(e), "danger");
    }
}

/* ---------- Send Chat (robust) ---------- */
async function sendChat() {
    if (!selectedIncidentId) return;
    const text = $("chatText").value.trim();
    if (!text) return;

    try {
        const payload = {
            incidentId: selectedIncidentId,
            senderEmployeeId: user?.id ?? 1,
            text,
            attachmentUrl: null
        };

        await postRobust("/api/incidents/chat", payload);
        $("chatText").value = "";
    } catch (e) {
        alert(errorText(e));
    }
}

/* ============================
   Wiring
   ============================ */
document.querySelectorAll(".tab").forEach(t => {
    t.addEventListener("click", () => {
        const tabName = t.dataset.tab;
        document.querySelectorAll(".tab").forEach(x => x.classList.toggle("active", x.dataset.tab === tabName));
        document.querySelectorAll(".pane").forEach(p => p.classList.toggle("active", p.id === `pane-${tabName}`));
    });
});

$("btnLogin").addEventListener("click", login);

$("btnLogout").addEventListener("click", async () => {
    token = null;
    user = null;
    incidents = [];
    selectedIncidentId = null;
    joinedIncidentId = null;

    if (hub) { try { await hub.stop(); } catch {} hub = null; }
    setConn("danger", "Offline");

    $("userName").textContent = "Not signed in";
    $("userRole").textContent = "—";
    $("userAvatar").textContent = "?";

    $("incidentList").innerHTML = `<div class="empty">Влез и създай инцидент.</div>`;
    clearDetails();
    enableAuthedUI(false);
    setHint("loginStatus", "Logged out.", "muted");
});

$("btnRefresh").addEventListener("click", async () => {
    try { await loadIncidents(); } catch (e) { alert(errorText(e)); }
});

$("search").addEventListener("input", renderIncidents);
$("statusFilter").addEventListener("change", renderIncidents);

$("btnNewIncident").addEventListener("click", () => {
    $("newIncidentStatus").textContent = "";
    showModal("modalIncident", true);
});
$("btnCloseIncident").addEventListener("click", () => showModal("modalIncident", false));
$("modalIncident").addEventListener("click", (e) => {
    if (e.target.id === "modalIncident") showModal("modalIncident", false);
});
$("btnCreateIncident").addEventListener("click", createIncident);

$("btnNewTask").addEventListener("click", () => {
    $("newTaskStatus").textContent = "";
    $("taskTitle").value = "";
    $("taskDetails").value = "";
    $("taskType").value = "Operational";
    showModal("modalTask", true);
});
$("btnCloseTask").addEventListener("click", () => showModal("modalTask", false));
$("modalTask").addEventListener("click", (e) => {
    if (e.target.id === "modalTask") showModal("modalTask", false);
});
$("btnCreateTask").addEventListener("click", createTask);

$("btnSendChat").addEventListener("click", sendChat);

// initial
enableAuthedUI(false);
setConn("danger", "Offline");
clearDetails();
