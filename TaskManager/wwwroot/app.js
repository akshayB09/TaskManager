let allTasks = [];
let currentFilter = 'pending';
let editingId = null;

// ── API calls ─────────────────────────────────────────────────────────────────

const api = {
  getTasks:     ()       => fetch('/api/tasks').then(r => r.json()),
  addTask:      data     => fetch('/api/tasks', json('POST', data)),
  editTask:     (id, d)  => fetch(`/api/tasks/${id}`, json('PUT', d)),
  completeTask: id       => fetch(`/api/tasks/${id}/complete`, { method: 'PATCH' }),
  deleteTask:   id       => fetch(`/api/tasks/${id}`, { method: 'DELETE' }),
};

function json(method, data) {
  return { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(data) };
}

// ── Render ────────────────────────────────────────────────────────────────────

async function loadAndRender() {
  allTasks = await api.getTasks();
  renderTasks();
}

function renderTasks() {
  const filtered = allTasks.filter(t =>
    currentFilter === 'all'     ? true :
    currentFilter === 'done'    ? t.isCompleted :
    /* pending */                 !t.isCompleted
  );

  const priorityOrder = { High: 0, Medium: 1, Low: 2 };
  filtered.sort((a, b) => {
    const pd = priorityOrder[a.priority] - priorityOrder[b.priority];
    if (pd !== 0) return pd;
    if (!a.dueDate && !b.dueDate) return 0;
    if (!a.dueDate) return 1;
    if (!b.dueDate) return -1;
    return new Date(a.dueDate) - new Date(b.dueDate);
  });

  const list  = document.getElementById('task-list');
  const empty = document.getElementById('empty-msg');

  if (filtered.length === 0) {
    list.innerHTML = '';
    empty.classList.remove('hidden');
    return;
  }

  empty.classList.add('hidden');
  list.innerHTML = filtered.map(taskCardHtml).join('');
}

function taskCardHtml(t) {
  const p   = t.priority.toLowerCase();
  const due = t.dueDate
    ? new Date(t.dueDate).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })
    : 'No due date';
  const badge = t.isCompleted
    ? `<span class="badge badge-done">Done</span>`
    : `<span class="badge badge-${p}">${t.priority}</span>`;
  const completeBtn = !t.isCompleted
    ? `<button class="btn btn-sm btn-complete" data-action="complete" data-id="${t.id}">&#10003; Done</button>`
    : '';

  return `
    <div class="task-card priority-${p} ${t.isCompleted ? 'done' : ''}">
      <div class="task-top">
        <span class="task-title">${esc(t.title)}</span>
        ${badge}
      </div>
      <div class="task-meta">${due}</div>
      ${t.description ? `<div class="task-desc">${esc(t.description)}</div>` : ''}
      <div class="task-actions">
        ${completeBtn}
        <button class="btn btn-sm btn-edit"   data-action="edit"   data-id="${t.id}">Edit</button>
        <button class="btn btn-sm btn-delete" data-action="delete" data-id="${t.id}">Delete</button>
      </div>
    </div>`;
}

const esc = s => s
  .replace(/&/g, '&amp;')
  .replace(/</g, '&lt;')
  .replace(/>/g, '&gt;');

// ── Event delegation for task buttons ─────────────────────────────────────────

document.getElementById('task-list').addEventListener('click', async e => {
  const btn = e.target.closest('[data-action]');
  if (!btn) return;
  const { action, id } = btn.dataset;

  if (action === 'complete') {
    await api.completeTask(id);
    await loadAndRender();
  } else if (action === 'delete') {
    if (!confirm('Delete this task?')) return;
    await api.deleteTask(id);
    await loadAndRender();
  } else if (action === 'edit') {
    openEditModal(id);
  }
});

// ── Modal ─────────────────────────────────────────────────────────────────────

function openAddModal() {
  editingId = null;
  document.getElementById('modal-title').textContent = 'New Task';
  document.getElementById('task-form').reset();
  document.getElementById('f-priority').value = 'Medium';
  showModal();
}

function openEditModal(id) {
  const t = allTasks.find(t => t.id === id);
  if (!t) return;
  editingId = id;
  document.getElementById('modal-title').textContent = 'Edit Task';
  document.getElementById('f-title').value    = t.title;
  document.getElementById('f-desc').value     = t.description ?? '';
  document.getElementById('f-due').value      = t.dueDate ? t.dueDate.split('T')[0] : '';
  document.getElementById('f-priority').value = t.priority;
  showModal();
}

const showModal  = () => { document.getElementById('modal').classList.remove('hidden'); document.getElementById('f-title').focus(); };
const closeModal = () =>   document.getElementById('modal').classList.add('hidden');

document.getElementById('open-add').addEventListener('click', openAddModal);
document.getElementById('close-modal').addEventListener('click', closeModal);
document.getElementById('modal').addEventListener('click', e => { if (e.target.id === 'modal') closeModal(); });

// ── Form submit ───────────────────────────────────────────────────────────────

document.getElementById('task-form').addEventListener('submit', async e => {
  e.preventDefault();
  const data = {
    title:       document.getElementById('f-title').value.trim(),
    description: document.getElementById('f-desc').value.trim() || null,
    dueDate:     document.getElementById('f-due').value || null,
    priority:    document.getElementById('f-priority').value,
  };

  if (editingId) {
    await api.editTask(editingId, data);
  } else {
    await api.addTask(data);
  }

  closeModal();
  await loadAndRender();
});

// ── Filter buttons ────────────────────────────────────────────────────────────

document.querySelectorAll('.filter-btn').forEach(btn => {
  btn.addEventListener('click', () => {
    document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    currentFilter = btn.dataset.filter;
    renderTasks();
  });
});

// ── Boot ──────────────────────────────────────────────────────────────────────

loadAndRender();
