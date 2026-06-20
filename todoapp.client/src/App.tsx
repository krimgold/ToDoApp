import { useEffect, useState } from 'react';
import './App.css';

interface Todo {
    id: string;
    name: string;
    status: string;
    priority: number;
}

function App() {
    const [todos, setTodos] = useState<Todo[]>();
    const [newTitle, setNewTitle] = useState('');
    const [newStatus, setNewStatus] = useState('NotStarted');
    const [newPriority, setNewPriority] = useState(1);
    const [createError, setCreateError] = useState<string | null>(null);

    // Start with no token so app always shows login on cold start
    // Token will be set after explicit login. We still persist it to localStorage
    // to support page reloads, but we don't auto-authenticate from storage to
    // avoid briefly showing protected UI before token validation.
    const [token, setToken] = useState<string | null>(null);
    const [editingId, setEditingId] = useState<string | null>(null);
    const [edits, setEdits] = useState<Record<string, { status: string; priority: number }>>({});
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loginError, setLoginError] = useState<string | null>(null);

    const API_BASE = 'https://localhost:7196';

    function api(path: string) {
        return `${API_BASE}${path.startsWith('/') ? path : `/${path}`}`;
    }

    useEffect(() => {
        if (token) populateTodos();
    }, [token]);

    const contents = !token
        ? <p><em className="small">Please log in to view tasks.</em></p>
        : (todos === undefined
            ? <p><em>Loading tasks…</em></p>
            : <ul className="task-list">
                {todos.map(todo =>
                    <li key={todo.id} className="task-item">
                        <div className="task-info">
                            <div style={{fontWeight:600}}>{todo.name}</div>
                            <div className="task-meta">Priority: {todo.priority} • Status: {todo.status}</div>
                        </div>
                        <div className="task-actions">
                            {editingId === todo.id
                                ? <div className="edit-controls">
                                    <select value={edits[todo.id]?.status ?? todo.status}
                                        onChange={e => setEdits({...edits, [todo.id]: { ...(edits[todo.id] ?? { status: todo.status, priority: todo.priority }), status: e.target.value, priority: edits[todo.id]?.priority ?? todo.priority }})}>
                                        <option>NotStarted</option>
                                        <option>InProgress</option>
                                        <option>Completed</option>
                                    </select>
                                    <input type="number" value={edits[todo.id]?.priority ?? todo.priority}
                                        onChange={e => setEdits({...edits, [todo.id]: { ...(edits[todo.id] ?? { status: todo.status, priority: todo.priority }), priority: Number(e.target.value) }})}
                                        className="input" style={{width:64}} />
                                    <button onClick={() => updateTodo(todo.id)} className="button" >Save</button>
                                    <button onClick={() => { setEditingId(null); }} className="button secondary">Cancel</button>
                                </div>
                                : <>
                                    <button onClick={() => { setEditingId(todo.id); setEdits({...edits, [todo.id]: { status: todo.status, priority: todo.priority }}); }} className="button">Edit</button>
                                    <button
                                        onClick={() => deleteTodo(todo.id)}
                                        className="button delete"
                                        disabled={todo.status !== 'Completed'}
                                        title={todo.status !== 'Completed' ? 'Only completed tasks can be deleted' : 'Delete'}
                                    >Delete</button>
                                </>
                            }
                        </div>
                    </li>
                )}
            </ul>
        );

    return (
        <div className="app-card">
            <div className="app-header">
                <div>
                    <h1 style={{margin:0}}>To-do list</h1>
                </div>
            </div>

            {/* If logged in, show the create form above the task list */}
            {token && (
                <form onSubmit={addTodo} className="create-form">
                    <input className={`input ${createError ? 'error' : ''}`} value={newTitle} onChange={e => { setNewTitle(e.target.value); if (createError) setCreateError(null); }} placeholder="New to-do title" />
                    <select className="input" value={newStatus} onChange={e => setNewStatus(e.target.value)}>
                        <option>NotStarted</option>
                        <option>InProgress</option>
                        <option>Completed</option>
                    </select>
                    <input className="input" type="number" value={newPriority} onChange={e => setNewPriority(Number(e.target.value))} style={{width:80}} min={0} />
                    <button type="submit" className="button" disabled={!newTitle.trim()}>Add</button>
                </form>
            )}
            {createError && <div className="error-text">{createError}</div>}

            {/* Always render the task contents (which will prompt to log in if not authenticated) */}
            {contents}

            <div style={{marginTop:16}}>
                {!token
                    ? <>
                        <form onSubmit={handleLogin} className="login-form">
                            <input className="input" value={username} onChange={e => setUsername(e.target.value)} placeholder="Username" />
                            <input className="input" type="password" value={password} onChange={e => setPassword(e.target.value)} placeholder="Password" />
                            <button type="submit" className="button">Login</button>
                        </form>
                        {loginError && <div style={{color:'red', marginTop:8}}>{loginError}</div>}
                      </>
                    : <div className="small">Logged in as <strong>{username}</strong> <button onClick={logout} className="button secondary" style={{marginLeft:8}}>Logout</button></div>
                }
            </div>
        </div>
    );

    function authHeaders() {
        const headers: Record<string,string> = { 'Content-Type': 'application/json' };
        // token state may not be updated synchronously right after login; fall back to localStorage
        const stored = localStorage.getItem('jwt_token');
        const t = token ?? stored;
        if (t) headers['Authorization'] = `Bearer ${t}`;
        return headers;
    }

    async function populateTodos() {
        try {
            const response = await fetch(api('/api/tasks'), { headers: authHeaders() });
            if (response.ok) {
                const data = await response.json();
                setTodos(data);
            } else {
                console.error('Failed to load todos', response.status);
                if (response.status === 401) {
                    // token invalid/expired
                    logout();
                }
            }
        } catch (err) {
            console.error('Error fetching todos', err);
        }
    }

    async function addTodo(e: React.FormEvent) {
        e.preventDefault();
        if (!newTitle) return;
        try {
            const response = await fetch(api('/api/tasks'), {
                method: 'POST',
                headers: authHeaders(),
                body: JSON.stringify({ Name: newTitle, Status: newStatus, Priority: newPriority })
            });
            if (response.ok) {
                setNewTitle('');
                setNewStatus('NotStarted');
                setNewPriority(1);
                populateTodos();
            } else {
                console.error('Failed to add todo', response.status);
                if (response.status === 401) logout();
            }
        } catch (err) {
            console.error('Error adding todo', err);
        }
    }

    async function deleteTodo(id: string) {
        // guard locally: don't call delete if task not completed
        const current = todos?.find(t => t.id === id);
        if (!current) return;
        if (current.status !== 'Completed') {
            // user-friendly feedback
            alert('Task cannot be deleted unless its status is Completed.');
            return;
        }

        try {
            const response = await fetch(api(`/api/tasks/${id}`), { method: 'DELETE', headers: authHeaders() });
            if (response.ok) {
                populateTodos();
            } else {
                console.error('Failed to delete', response.status);
                if (response.status === 401) logout();
                else if (response.status === 400 || response.status === 403) {
                    // show server-side validation message if available
                    try {
                        const text = await response.text();
                        alert(text || 'Task cannot be deleted');
                    } catch {
                        alert('Task cannot be deleted');
                    }
                }
            }
        } catch (err) {
            console.error('Error deleting todo', err);
            alert('Error deleting task');
        }
    }

    async function updateTodo(id: string) {
        const edit = edits[id];
        if (!edit) return;
        // find the current todo to preserve Name
        const current = todos?.find(t => t.id === id);
        if (!current) return;

        try {
            const response = await fetch(api(`/api/tasks/${id}`), {
                method: 'PUT',
                headers: authHeaders(),
                body: JSON.stringify({ Id: id, Name: current.name, Status: edit.status, Priority: edit.priority })
            });
            if (response.ok) {
                setEditingId(null);
                // refresh list
                populateTodos();
            } else {
                console.error('Failed to update', response.status);
                if (response.status === 401) logout();
            }
        } catch (err) {
            console.error('Error updating todo', err);
        }
    }

    async function handleLogin(e: React.FormEvent) {
        e.preventDefault();
        setLoginError(null);
        try {
            const response = await fetch(api('/api/login'), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Username: username, Password: password })
            });
            if (!response.ok) {
                setLoginError('Login failed');
                return;
            }
            // try to parse JSON { token: '...' } or plain text token
            let tokenValue: string | null = null;
            const contentType = response.headers.get('content-type') || '';
            if (contentType.includes('application/json')) {
                const data = await response.json();
                // server returns { JwtToken: "...", ExpiresIn: n }
                tokenValue = data?.JwtToken ?? data?.jwtToken ?? data?.token ?? (typeof data === 'string' ? data : null);
            } else {
                tokenValue = await response.text();
            }
            if (tokenValue) {
                setToken(tokenValue);
                localStorage.setItem('jwt_token', tokenValue);
                setPassword('');
                setLoginError(null);
                // load tasks immediately after successful login
                populateTodos();
            } else {
                setLoginError('No token returned');
            }
        } catch (err) {
            console.error('Login error', err);
            setLoginError('Login error');
        }
    }

    function logout() {
        setToken(null);
        localStorage.removeItem('jwt_token');
        setTodos(undefined);
    }
}

export default App;
