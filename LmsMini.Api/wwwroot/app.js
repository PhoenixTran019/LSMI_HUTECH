// Nếu FE chạy cùng origin với API thì để rỗng là được
const apiBaseUrl = ""; // ví dụ: "" hoặc "https://localhost:5001"

let authToken = null;
let currentClassroomId = null;
let currentAssignmentId = null;

// ---------- Helper UI ----------

function showAlert(message, type = "success", timeout = 3000) {
    const container = document.getElementById("alert-container");
    const wrapper = document.createElement("div");
    wrapper.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    container.appendChild(wrapper);
    if (timeout) {
        setTimeout(() => {
            wrapper.querySelector(".alert")?.classList.remove("show");
            wrapper.querySelector(".alert")?.classList.add("hide");
            wrapper.remove();
        }, timeout);
    }
}

function showView(viewId) {
    const profile = document.getElementById("profile-view");
    const classrooms = document.getElementById("classrooms-view");

    profile.classList.add("d-none");
    classrooms.classList.add("d-none");

    document.getElementById(viewId).classList.remove("d-none");

    // set active tab
    document.getElementById("nav-profile").classList.remove("active");
    document.getElementById("nav-classrooms").classList.remove("active");
    if (viewId === "profile-view") {
        document.getElementById("nav-profile").classList.add("active");
    } else if (viewId === "classrooms-view") {
        document.getElementById("nav-classrooms").classList.add("active");
    }
}

// ---------- HTTP helper ----------

function authorizedFetch(url, options = {}) {
    const headers = options.headers || {};
    headers["Content-Type"] = headers["Content-Type"] || "application/json";

    if (authToken) {
        headers["Authorization"] = "Bearer " + authToken;
    }
    return fetch(apiBaseUrl + url, { ...options, headers });
}

// special helper for multipart (không tự set Content-Type)
function authorizedFetchMultipart(url, options = {}) {
    const headers = options.headers || {};
    if (authToken) {
        headers["Authorization"] = "Bearer " + authToken;
    }
    return fetch(apiBaseUrl + url, { ...options, headers });
}

// ---------- LOGIN / LOGOUT ----------

async function handleLogin(event) {
    event.preventDefault();
    const username = document.getElementById("login-username").value.trim();
    const password = document.getElementById("login-password").value;

    if (!username || !password) {
        showAlert("Vui lòng nhập Username và Password.", "warning");
        return;
    }

    try {
        const res = await fetch(apiBaseUrl + "/api/account/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        if (!res.ok) {
            const txt = await res.text();
            throw new Error(txt || "Đăng nhập thất bại.");
        }

        const data = await res.json();
        // giả định LoginResponseDto có property Token, Role, Username
        authToken = data.token || data.Token;
        if (!authToken) throw new Error("Không nhận được token từ server.");

        localStorage.setItem("lms_token", authToken);
        localStorage.setItem("lms_username", data.username || data.Username || username);

        document.getElementById("current-username").textContent =
            data.username || data.Username || username;

        document.getElementById("login-section").classList.add("d-none");
        document.getElementById("app-section").classList.remove("d-none");

        await loadProfile();
        await loadClassrooms();
        showAlert("Đăng nhập thành công.", "success");
    } catch (err) {
        console.error(err);
        showAlert("Đăng nhập thất bại: " + err.message, "danger", 5000);
    }
}

function handleLogout() {
    authToken = null;
    localStorage.removeItem("lms_token");
    localStorage.removeItem("lms_username");

    document.getElementById("app-section").classList.add("d-none");
    document.getElementById("login-section").classList.remove("d-none");
}

// ---------- PROFILE ----------

async function loadProfile() {
    try {
        const res = await authorizedFetch("/api/student/me", {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if (!res.ok) {
            throw new Error("Không lấy được hồ sơ.");
        }

        const p = await res.json();
        const info = document.getElementById("profile-info");
        info.innerHTML = `
            <dt class="col-sm-3">Student ID</dt><dd class="col-sm-9">${p.studentId || p.StudentId}</dd>
            <dt class="col-sm-3">Họ tên</dt><dd class="col-sm-9">${(p.lastName || p.LastName || "")} ${(p.firstName || p.FirstName || "")}</dd>
            <dt class="col-sm-3">Giới tính</dt><dd class="col-sm-9">${p.gender || p.Gender || ""}</dd>
            <dt class="col-sm-3">Lớp</dt><dd class="col-sm-9">${p.className || p.ClassName || ""}</dd>
            <dt class="col-sm-3">Khoa</dt><dd class="col-sm-9">${p.departName || p.DepartName || ""}</dd>
            <dt class="col-sm-3">Ngành</dt><dd class="col-sm-9">${p.majorName || p.MajorName || ""}</dd>
            <dt class="col-sm-3">Điện thoại</dt><dd class="col-sm-9">${p.phoneNum || p.PhoneNum || ""}</dd>
            <dt class="col-sm-3">Email</dt><dd class="col-sm-9">${p.mail || p.Mail || ""}</dd>
            <dt class="col-sm-3">Địa chỉ</dt><dd class="col-sm-9">${p.address || p.Address || ""}</dd>
        `;

        // fill vào form update
        document.getElementById("upd-phone").value = p.phoneNum || p.PhoneNum || "";
        document.getElementById("upd-email").value = p.mail || p.Mail || "";
        document.getElementById("upd-address").value = p.address || p.Address || "";
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi load hồ sơ: " + err.message, "danger");
    }
}

async function handleProfileUpdate(event) {
    event.preventDefault();
    const phone = document.getElementById("upd-phone").value.trim();
    const mail = document.getElementById("upd-email").value.trim();
    const addr = document.getElementById("upd-address").value.trim();

    const body = {
        phoneNum: phone || null,
        mail: mail || null,
        address: addr || null
    };

    try {
        const res = await authorizedFetch("/api/student/me", {
            method: "PUT",
            body: JSON.stringify(body)
        });
        if (!res.ok) {
            const txt = await res.text();
            throw new Error(txt || "Cập nhật thất bại.");
        }
        showAlert("Cập nhật hồ sơ thành công.", "success");
        await loadProfile();
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi cập nhật: " + err.message, "danger");
    }
}

// ---------- CLASSROOMS ----------

async function loadClassrooms() {
    try {
        const res = await authorizedFetch("/api/student/my-classrooms", {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) throw new Error("Không lấy được danh sách lớp.");

        const list = await res.json();
        const container = document.getElementById("classroom-list");
        container.innerHTML = "";

        if (!list || list.length === 0) {
            container.innerHTML = `<div class="text-muted small">Chưa tham gia lớp nào.</div>`;
            return;
        }

        list.forEach(c => {
            const id = c.classroomId || c.ClassroomId;
            const name = c.classroomName || c.ClassroomName || "(No name)";
            const subject = c.subjectName || c.SubjectName || "";
            const code = c.subjectCode || c.SubjectCode || "";

            const a = document.createElement("button");
            a.className = "list-group-item list-group-item-action d-flex justify-content-between align-items-center";
            a.innerHTML = `
                <div>
                    <div class="fw-semibold">${name}</div>
                    <div class="small text-muted">${subject} (${code})</div>
                </div>
                <span class="badge bg-secondary">Chi tiết</span>
            `;
            a.addEventListener("click", () => openClassroom(id, name));
            container.appendChild(a);
        });
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi load lớp: " + err.message, "danger");
    }
}

async function handleJoinClassroom(event) {
    event.preventDefault();
    const invite = document.getElementById("join-invite-code").value.trim();
    if (!invite) {
        showAlert("Vui lòng nhập InviteCode.", "warning");
        return;
    }

    try {
        const res = await authorizedFetch("/api/student/classroom/join-by-invite", {
            method: "POST",
            body: JSON.stringify({ inviteCode: invite })
        });
        if (!res.ok) {
            const txt = await res.text();
            throw new Error(txt || "Không tham gia lớp được.");
        }
        showAlert("Tham gia lớp thành công.", "success");
        document.getElementById("join-invite-code").value = "";
        await loadClassrooms();
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi tham gia lớp: " + err.message, "danger");
    }
}

async function openClassroom(classroomId, classroomName) {
    currentClassroomId = classroomId;
    document.getElementById("detail-classroom-name").textContent = classroomName || classroomId;
    document.getElementById("classroom-detail").classList.remove("d-none");

    await Promise.all([
        loadLessons(classroomId),
        loadAssignments(classroomId)
    ]);
}

// ---------- LESSONS ----------

async function loadLessons(classroomId) {
    try {
        const res = await authorizedFetch(`/api/student/classrooms/${encodeURIComponent(classroomId)}/lessons`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) throw new Error("Không lấy được lesson.");

        const list = await res.json();
        const ul = document.getElementById("lesson-list");
        ul.innerHTML = "";

        if (!list || list.length === 0) {
            ul.innerHTML = `<li class="list-group-item small text-muted">Chưa có bài giảng.</li>`;
            return;
        }

        list.forEach(l => {
            const li = document.createElement("li");
            li.className = "list-group-item list-group-item-action";
            li.textContent = l.title || l.Title || "(Lesson)";
            const id = l.lessonId || l.LessonId;
            li.addEventListener("click", () => openLesson(id));
            ul.appendChild(li);
        });
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi load lesson: " + err.message, "danger");
    }
}

async function openLesson(lessonId) {
    try {
        const res = await authorizedFetch(`/api/student/lessons/${encodeURIComponent(lessonId)}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) throw new Error("Không lấy được chi tiết bài giảng.");
        const lesson = await res.json();

        document.getElementById("lesson-title").textContent =
            lesson.title || lesson.Title || "(Lesson)";
        document.getElementById("lesson-content").textContent =
            lesson.content || lesson.Content || "";

        const filesDiv = document.getElementById("lesson-files");
        filesDiv.innerHTML = "";
        const files = lesson.files || lesson.Files || [];
        if (files.length > 0) {
            const list = document.createElement("ul");
            list.className = "list-group list-group-flush";
            files.forEach(f => {
                const li = document.createElement("li");
                li.className = "list-group-item small";
                li.textContent = f.fileName || f.FileName || "(file)";
                // nếu sau này có endpoint download file thì thêm link ở đây
                list.appendChild(li);
            });
            filesDiv.appendChild(list);
        }

        document.getElementById("lesson-detail").classList.remove("d-none");
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi xem bài giảng: " + err.message, "danger");
    }
}

// ---------- ASSIGNMENTS ----------

async function loadAssignments(classroomId) {
    try {
        const res = await authorizedFetch(`/api/student/classrooms/${encodeURIComponent(classroomId)}/assignments`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) throw new Error("Không lấy được assignment.");
        const list = await res.json();
        const ul = document.getElementById("assignment-list");
        ul.innerHTML = "";

        if (!list || list.length === 0) {
            ul.innerHTML = `<li class="list-group-item small text-muted">Chưa có bài tập.</li>`;
            return;
        }

        list.forEach(a => {
            const li = document.createElement("li");
            li.className = "list-group-item list-group-item-action small";
            const title = a.title || a.Title || "(Assignment)";
            const deadline = a.deadline || a.Deadline;
            const isSubmitted = a.isSubmitted || a.IsSubmitted;

            li.innerHTML = `
                <div class="d-flex justify-content-between">
                    <div>
                        <div class="fw-semibold">${title}</div>
                        <div class="text-muted">Deadline: ${deadline || ""}</div>
                    </div>
                    <div>
                        ${isSubmitted ? '<span class="badge bg-success">Đã nộp</span>' : '<span class="badge bg-secondary">Chưa nộp</span>'}
                    </div>
                </div>
            `;

            const id = a.assignmentId || a.AssignmentId;
            li.addEventListener("click", () => openAssignment(id));
            ul.appendChild(li);
        });
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi load assignment: " + err.message, "danger");
    }
}

async function openAssignment(assignId) {
    currentAssignmentId = assignId;
    try {
        const res = await authorizedFetch(`/api/student/assignments/${encodeURIComponent(assignId)}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) throw new Error("Không lấy được chi tiết bài tập.");

        const a = await res.json();

        document.getElementById("assign-title").textContent = a.title || a.Title || "(Assignment)";
        document.getElementById("assign-description").textContent = a.description || a.Description || "";
        document.getElementById("assign-deadline").textContent = a.deadline || a.Deadline || "";
        document.getElementById("assign-status").textContent = a.deadlineStatus || a.DeadlineStatus || "";

        const grade = a.grade ?? a.Grade;
        const feedback = a.feedback || a.FeedBack;
        document.getElementById("assign-grade").textContent = grade != null ? grade : "Chưa chấm";
        document.getElementById("assign-feedback").textContent = feedback || "—";

        const filesDiv = document.getElementById("assign-files");
        filesDiv.innerHTML = "";
        const files = a.files || a.Files || [];
        if (files.length > 0) {
            const list = document.createElement("ul");
            list.className = "list-group list-group-flush small";
            files.forEach(f => {
                const li = document.createElement("li");
                li.className = "list-group-item";
                li.textContent = f.fileName || f.FileName || "(file)";
                list.appendChild(li);
            });
            filesDiv.appendChild(list);
        }

        // load submission info
        await loadMySubmission(assignId);

        document.getElementById("assignment-detail").classList.remove("d-none");
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi xem bài tập: " + err.message, "danger");
    }
}

async function handleAssignmentSubmit(event) {
    event.preventDefault();
    if (!currentAssignmentId) {
        showAlert("Chưa chọn bài tập.", "warning");
        return;
    }

    const input = document.getElementById("assign-files-input");
    if (!input.files || input.files.length === 0) {
        showAlert("Vui lòng chọn ít nhất 1 file.", "warning");
        return;
    }

    const formData = new FormData();
    // tên field phải trùng với tham số List<IFormFile> files
    for (let i = 0; i < input.files.length; i++) {
        formData.append("files", input.files[i]);
    }

    try {
        const res = await authorizedFetchMultipart(`/api/student/assignments/${encodeURIComponent(currentAssignmentId)}/submit`, {
            method: "POST",
            body: formData
        });
        if (!res.ok) {
            const txt = await res.text();
            throw new Error(txt || "Nộp bài thất bại.");
        }
        showAlert("Nộp bài thành công.", "success");
        input.value = "";
        await loadMySubmission(currentAssignmentId);
        await loadAssignments(currentClassroomId);
    } catch (err) {
        console.error(err);
        showAlert("Lỗi khi nộp bài: " + err.message, "danger");
    }
}

async function loadMySubmission(assignId) {
    const div = document.getElementById("submission-info");
    div.textContent = "";
    try {
        const res = await authorizedFetch(`/api/student/assignments/${encodeURIComponent(assignId)}/submission`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) {
            // nếu 404 = chưa nộp, coi như không sao
            if (res.status === 404) {
                div.textContent = "Bạn chưa nộp bài.";
                return;
            }
            throw new Error("Không lấy được submission.");
        }
        const s = await res.json();
        const time = s.submitAt || s.SubmitAt || "";
        const grade = s.grade ?? s.Grade;
        div.textContent = `Đã nộp lúc ${time} – Điểm: ${grade ?? "Chưa chấm"}`;
    } catch (err) {
        console.error(err);
        div.textContent = "Không lấy được thông tin bài nộp.";
    }
}

// ---------- INIT ----------

document.addEventListener("DOMContentLoaded", () => {
    // gắn event
    document.getElementById("login-form").addEventListener("submit", handleLogin);
    document.getElementById("btn-logout").addEventListener("click", handleLogout);
    document.getElementById("nav-profile").addEventListener("click", () => showView("profile-view"));
    document.getElementById("nav-classrooms").addEventListener("click", () => showView("classrooms-view"));
    document.getElementById("profile-update-form").addEventListener("submit", handleProfileUpdate);
    document.getElementById("join-classroom-form").addEventListener("submit", handleJoinClassroom);
    document.getElementById("assign-submit-form").addEventListener("submit", handleAssignmentSubmit);

    // nếu đã có token (đã login trước đó) thì auto login lại
    const savedToken = localStorage.getItem("lms_token");
    const savedUser = localStorage.getItem("lms_username");
    if (savedToken) {
        authToken = savedToken;
        document.getElementById("current-username").textContent = savedUser || "";
        document.getElementById("login-section").classList.add("d-none");
        document.getElementById("app-section").classList.remove("d-none");

        loadProfile();
        loadClassrooms();
    }
});
