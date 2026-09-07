import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { StaffLinkPanel } from '@/components/StaffLinkPanel';

describe('StaffLinkPanel', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    global.fetch = originalFetch;
    jest.restoreAllMocks();
  });

  function mockFetchSequence(...responses: Array<{ ok: boolean; status?: number; json: () => Promise<unknown> }>) {
    const fetchSpy = jest.fn();
    responses.forEach((response) => fetchSpy.mockResolvedValueOnce(response));
    global.fetch = fetchSpy as unknown as typeof fetch;
    return fetchSpy;
  }

  it('shows a linked confirmation when /api/staff/me succeeds', async () => {
    mockFetchSequence({
      ok: true,
      json: () => Promise.resolve({ id: 'a', name: 'Kari Nordmann', auth0UserId: 'auth0|abc123' }),
    });

    render(<StaffLinkPanel />);

    await waitFor(() => expect(screen.getByText('Kari Nordmann')).toBeInTheDocument());
    expect(screen.getByText(/Du er koblet som/)).toBeInTheDocument();
  });

  it('shows the error state when /api/staff/me fails with something other than 404', async () => {
    mockFetchSequence({ ok: false, status: 500, json: () => Promise.resolve(null) });

    render(<StaffLinkPanel />);

    await waitFor(() =>
      expect(screen.getByText('Får ikke kontakt med API-et.')).toBeInTheDocument(),
    );
  });

  it('shows the error state when the bootstrap list cannot be loaded after a 404 from /me', async () => {
    mockFetchSequence(
      { ok: false, status: 404, json: () => Promise.resolve(null) },
      { ok: false, json: () => Promise.resolve(null) },
    );

    render(<StaffLinkPanel />);

    await waitFor(() =>
      expect(screen.getByText('Får ikke kontakt med API-et.')).toBeInTheDocument(),
    );
  });

  it('shows a message when there are no staff profiles yet', async () => {
    mockFetchSequence(
      { ok: false, status: 404, json: () => Promise.resolve(null) },
      { ok: true, json: () => Promise.resolve([]) },
    );

    render(<StaffLinkPanel />);

    await waitFor(() =>
      expect(screen.getByText('Ingen ansattprofiler ennå.')).toBeInTheDocument(),
    );
  });

  it('shows an unlinked profile with a link button, and a linked one without', async () => {
    mockFetchSequence(
      { ok: false, status: 404, json: () => Promise.resolve(null) },
      {
        ok: true,
        json: () =>
          Promise.resolve([
            { id: 'a', name: 'Kari Nordmann', auth0UserId: null },
            { id: 'b', name: 'Ola Nordmann', auth0UserId: 'auth0|abc123' },
          ]),
      },
    );

    render(<StaffLinkPanel />);

    await waitFor(() => expect(screen.getByText('Kari Nordmann')).toBeInTheDocument());
    expect(screen.getByRole('button', { name: 'Koble til min konto' })).toBeInTheDocument();
    expect(screen.getByText('Ola Nordmann')).toBeInTheDocument();
    expect(screen.getByText('Koblet')).toBeInTheDocument();
  });

  it('creates a profile and refreshes the list', async () => {
    const fetchSpy = mockFetchSequence(
      { ok: false, status: 404, json: () => Promise.resolve(null) },
      { ok: true, json: () => Promise.resolve([]) },
      { ok: true, json: () => Promise.resolve({ id: 'a', name: 'Kari Nordmann', auth0UserId: null }) },
      { ok: false, status: 404, json: () => Promise.resolve(null) },
      {
        ok: true,
        json: () => Promise.resolve([{ id: 'a', name: 'Kari Nordmann', auth0UserId: null }]),
      },
    );

    render(<StaffLinkPanel />);

    await waitFor(() => expect(screen.getByText('Ingen ansattprofiler ennå.')).toBeInTheDocument());

    fireEvent.change(screen.getByLabelText('Navn'), { target: { value: 'Kari Nordmann' } });
    fireEvent.click(screen.getByRole('button', { name: 'Opprett profil' }));

    await waitFor(() => expect(screen.getByText('Kari Nordmann')).toBeInTheDocument());

    expect(fetchSpy).toHaveBeenNthCalledWith(
      3,
      '/api/staff',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ name: 'Kari Nordmann' }),
      }),
    );
  });

  it('shows the Norwegian conflict message when the account is already linked elsewhere', async () => {
    mockFetchSequence(
      { ok: false, status: 404, json: () => Promise.resolve(null) },
      {
        ok: true,
        json: () =>
          Promise.resolve([{ id: 'a', name: 'Kari Nordmann', auth0UserId: null }]),
      },
      { ok: false, json: () => Promise.resolve({ reason: 'Auth0AccountAlreadyLinked' }) },
    );

    render(<StaffLinkPanel />);

    await waitFor(() => expect(screen.getByText('Kari Nordmann')).toBeInTheDocument());

    fireEvent.click(screen.getByRole('button', { name: 'Koble til min konto' }));

    await waitFor(() =>
      expect(
        screen.getByText('Kontoen din er allerede koblet til en annen profil.'),
      ).toBeInTheDocument(),
    );
  });
});
