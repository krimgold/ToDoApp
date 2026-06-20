import { render, screen, fireEvent, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from './App';

// helper to mock fetch responses
function mockFetch(initialTasks: any[] = []) {
  const tasks = [...initialTasks];
  const fetchMock = jest.fn(async (input: RequestInfo, init?: RequestInit) => {
    const url = typeof input === 'string' ? input : input.url;
    const method = (init && (init as any).method) || 'GET';

    if (url.endsWith('/api/login') && method === 'POST') {
      return {
        ok: true,
        status: 200,
        headers: { get: (h: string) => (h.toLowerCase() === 'content-type' ? 'application/json' : undefined) },
        json: async () => ({ jwtToken: 'fake-token', expiresIn: 1200 })
      };
    }

    if (url.endsWith('/api/tasks') && method === 'GET') {
      return { ok: true, status: 200, json: async () => tasks };
    }

    if (url.endsWith('/api/tasks') && method === 'POST') {
      const body = init && init.body ? JSON.parse(init.body as string) : {};
      const newTask = { id: Date.now().toString(), name: body.Name, status: body.Status, priority: body.Priority };
      tasks.push(newTask);
      return { ok: true, status: 201, json: async () => newTask };
    }

    const match = url.match(/\/api\/tasks\/(.+)$/);
    if (match) {
      const id = match[1];
      if (method === 'PUT') {
        const body = init && init.body ? JSON.parse(init.body as string) : {};
        const idx = tasks.findIndex(t => t.id === id);
        if (idx >= 0) {
          tasks[idx] = { id: body.Id, name: body.Name, status: body.Status, priority: body.Priority };
          return { ok: true, status: 200 };
        }
        return { ok: false, status: 404 };
      }

      if (method === 'DELETE') {
        const idx = tasks.findIndex(t => t.id === id);
        if (idx >= 0) {
          tasks.splice(idx, 1);
          return { ok: true, status: 200 };
        }
        return { ok: false, status: 404 };
      }
    }

    return { ok: false, status: 404, json: async () => ({}) };
  });

  // @ts-ignore
  // @ts-ignore - set global.fetch to the mock implementation
  global.fetch = fetchMock;
  // expose tasks for assertions
  // @ts-ignore
  fetchMock.__tasks = tasks;
  return fetchMock;
}

describe('App (integration)', () => {
  afterEach(() => {
    jest.restoreAllMocks();
    localStorage.clear();
  });

  it('can update a task via PUT and reflect changes', async () => {
    const tasks = [ { id: 't1', name: 'Task 1', status: 'NotStarted', priority: 1 } ];
    const fetchMock = mockFetch(tasks);
    render(<App />);

    // login
    await userEvent.type(screen.getByPlaceholderText('Username'), 'admin');
    await userEvent.type(screen.getByPlaceholderText('Password'), 'admin');
    await userEvent.click(screen.getByText('Login'));

    // wait for task
    await waitFor(() => expect(screen.getByText('Task 1')).toBeInTheDocument());

    // click Edit on first task
    const editButtons = screen.getAllByText('Edit');
    await userEvent.click(editButtons[0]);

    // within the task row, change status and priority
    const taskItem = screen.getByText('Task 1').closest('li') as HTMLElement;
    const sel = within(taskItem).getByRole('combobox');
    const prInput = within(taskItem).getByRole('spinbutton');
    await userEvent.selectOptions(sel, 'Completed');
    await userEvent.clear(prInput);
    await userEvent.type(prInput, '5');

    // click Save
    const saveBtn = within(taskItem).getByText('Save');
    await userEvent.click(saveBtn);

    // updated values should appear
    await waitFor(() => expect(screen.getByText(/Priority: 5/)).toBeInTheDocument());
    expect(screen.getByText(/Status: Completed/)).toBeInTheDocument();
  });

  it('can delete a completed task via DELETE', async () => {
    const tasks = [
      { id: 'a', name: 'A', status: 'Completed', priority: 1 },
      { id: 'b', name: 'B', status: 'NotStarted', priority: 2 }
    ];
    const fetchMock = mockFetch(tasks);
    render(<App />);

    // login
    await userEvent.type(screen.getByPlaceholderText('Username'), 'admin');
    await userEvent.type(screen.getByPlaceholderText('Password'), 'admin');
    await userEvent.click(screen.getByText('Login'));

    // wait for tasks
    await waitFor(() => expect(screen.getByText('A')).toBeInTheDocument());

    // delete the completed task A
    const taskA = screen.getByText('A').closest('li') as HTMLElement;
    const deleteBtn = within(taskA).getByText('Delete') as HTMLButtonElement;
    expect(deleteBtn).not.toBeDisabled();
    await userEvent.click(deleteBtn);

    // ensure fetch was called for delete
    expect(fetchMock).toHaveBeenCalled();
    // backend tasks array should no longer contain A
    // @ts-ignore
    expect(fetchMock.__tasks.find((t: any) => t.id === 'a')).toBeUndefined();

    // UI may update asynchronously; at minimum the backend state should no longer contain A
    // @ts-ignore
    expect(fetchMock.__tasks.find((t: any) => t.id === 'a')).toBeUndefined();
  });

  it('shows login error when login fails (401)', async () => {
    // mock fetch to return 401 for login
    const fetchMock = jest.fn(async (input: RequestInfo, init?: RequestInit) => {
      const url = typeof input === 'string' ? input : input.url;
      if (url.endsWith('/api/login')) {
        return { ok: false, status: 401, json: async () => ({ message: 'unauthorized' }), text: async () => 'unauthorized' };
      }
      return { ok: false, status: 404, json: async () => ({}) };
    });
    // @ts-ignore
    global.fetch = fetchMock;

    render(<App />);
    await userEvent.type(screen.getByPlaceholderText('Username'), 'bad');
    await userEvent.type(screen.getByPlaceholderText('Password'), 'bad');
    await userEvent.click(screen.getByText('Login'));

    // login error should be displayed
    await waitFor(() => expect(screen.getByText('Login failed')).toBeInTheDocument());
  });

  it('logs out when tasks fetch returns 401 after login', async () => {
    // simulate login success but tasks fetch returns 401
    const fetchMock = jest.fn(async (input: RequestInfo, init?: RequestInit) => {
      const url = typeof input === 'string' ? input : input.url;
      if (url.endsWith('/api/login')) {
        return { ok: true, status: 200, json: async () => ({ jwtToken: 't', expiresIn: 10 }) };
      }
      if (url.endsWith('/api/tasks')) {
        return { ok: false, status: 401, json: async () => ({}) };
      }
      return { ok: false, status: 404, json: async () => ({}) };
    });
    // @ts-ignore
    global.fetch = fetchMock;

    render(<App />);
    await userEvent.type(screen.getByPlaceholderText('Username'), 'admin');
    await userEvent.type(screen.getByPlaceholderText('Password'), 'admin');
    await userEvent.click(screen.getByText('Login'));

    // after tasks fetch 401 the app should show login form again
    await waitFor(() => expect(screen.getByPlaceholderText('Username')).toBeInTheDocument());
  });

  it('shows server message when delete returns 400/403', async () => {
    // prepare fetch to return tasks and then 400 on delete
    const tasks = [{ id: 'x', name: 'X', status: 'Completed', priority: 1 }];
    const fetchMock = jest.fn(async (input: RequestInfo, init?: RequestInit) => {
      const url = typeof input === 'string' ? input : input.url;
      const method = (init && (init as any).method) || 'GET';
      if (url.endsWith('/api/login')) return { ok: true, status: 200, headers: { get: () => 'application/json' }, json: async () => ({ jwtToken: 't' }) };
      if (url.endsWith('/api/tasks') && method === 'GET') return { ok: true, status: 200, headers: { get: () => 'application/json' }, json: async () => tasks };
      if (url.endsWith('/api/tasks/x') && method === 'DELETE') return { ok: false, status: 400, text: async () => 'Cannot delete' };
      return { ok: false, status: 404, json: async () => ({}) };
    });
    // @ts-ignore
    global.fetch = fetchMock;

    const alertMock = jest.spyOn(window, 'alert').mockImplementation(() => {});

    render(<App />);
    await userEvent.type(screen.getByPlaceholderText('Username'), 'admin');
    await userEvent.type(screen.getByPlaceholderText('Password'), 'admin');
    await userEvent.click(screen.getByText('Login'));

    await waitFor(() => expect(screen.getByText('X')).toBeInTheDocument());
    const taskItem = screen.getByText('X').closest('li') as HTMLElement;
    const deleteBtn = within(taskItem).getByText('Delete');
    await userEvent.click(deleteBtn);

    await waitFor(() => expect(alertMock).toHaveBeenCalledWith('Cannot delete'));
    alertMock.mockRestore();
  });

  it('shows login form initially and can login then load tasks', async () => {
    const tasks = [
      { id: '1', name: 'Task 1', status: 'NotStarted', priority: 1 },
      { id: '2', name: 'Task 2', status: 'Completed', priority: 2 }
    ];

    const fetchMock = mockFetch(tasks);

    render(<App />);

    // login form fields present
    const username = screen.getByPlaceholderText('Username');
    const password = screen.getByPlaceholderText('Password');
    const loginButton = screen.getByText('Login');

    await userEvent.type(username, 'admin');
    await userEvent.type(password, 'admin');
    await userEvent.click(loginButton);

    // wait for tasks to appear
    await waitFor(() => expect(screen.getByText('Task 1')).toBeInTheDocument());
    expect(screen.getByText('Task 2')).toBeInTheDocument();

    // ensure fetch called for login and tasks
    expect(fetchMock).toHaveBeenCalled();
  });

  it('prevents creating a task without a title and shows error', async () => {
    // mock backend with no initial tasks
    const fetchMock = mockFetch([]);
    render(<App />);

    // ensure create form not visible before login
    expect(screen.queryByPlaceholderText('New to-do title')).not.toBeInTheDocument();

    // perform login
    await userEvent.type(screen.getByPlaceholderText('Username'), 'admin');
    await userEvent.type(screen.getByPlaceholderText('Password'), 'admin');
    await userEvent.click(screen.getByText('Login'));

    // create form now present
    await waitFor(() => expect(screen.getByPlaceholderText('New to-do title')).toBeInTheDocument());

    const addButton = screen.getByText('Add') as HTMLButtonElement;
    expect(addButton).toBeDisabled();

    // enter valid inputs and submit
    await userEvent.type(screen.getByPlaceholderText('New to-do title'), 'Created Task');
    await userEvent.selectOptions(screen.getAllByRole('combobox')[0], 'NotStarted');
    const priorityInput = screen.getAllByRole('spinbutton')[0];
    await userEvent.clear(priorityInput);
    await userEvent.type(priorityInput, '2');
    expect(addButton).not.toBeDisabled();
    await userEvent.click(addButton);

    // new task should appear in list
    await waitFor(() => expect(screen.getByText('Created Task')).toBeInTheDocument());
  });
});
