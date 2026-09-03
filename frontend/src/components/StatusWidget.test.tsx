import { render, screen, waitFor } from '@testing-library/react';
import { StatusWidget } from '@/components/StatusWidget';

describe('StatusWidget', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    global.fetch = originalFetch;
    jest.restoreAllMocks();
  });

  it('does not show the error state while the request is still in flight', () => {
    global.fetch = jest.fn(() => new Promise(() => {})) as unknown as typeof fetch;

    render(<StatusWidget />);

    expect(screen.queryByText('Får ikke kontakt med API-et.')).not.toBeInTheDocument();
  });

  it('fetches through the same-origin proxy, not the backend directly', async () => {
    const fetchSpy = jest.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ status: 'ok', database: 'up' }),
    });
    global.fetch = fetchSpy as unknown as typeof fetch;

    render(<StatusWidget />);

    await waitFor(() => expect(screen.getByText('Tilkoblet')).toBeInTheDocument());

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/health',
      expect.objectContaining({ cache: 'no-store', signal: expect.any(AbortSignal) }),
    );
  });

  it('does not show the error state when the request is aborted (Strict Mode remount)', async () => {
    global.fetch = jest.fn(
      () => Promise.reject(new DOMException('aborted', 'AbortError')),
    ) as unknown as typeof fetch;

    render(<StatusWidget />);

    // Give the rejected promise a tick to settle, then confirm the error
    // state never appeared - an aborted request is not a failed one.
    await new Promise((resolve) => setTimeout(resolve, 0));
    expect(screen.queryByText('Får ikke kontakt med API-et.')).not.toBeInTheDocument();
  });

  it('shows the error state when the proxy call fails', async () => {
    global.fetch = jest.fn().mockRejectedValue(new Error('network error')) as unknown as typeof fetch;

    render(<StatusWidget />);

    await waitFor(() =>
      expect(screen.getByText('Får ikke kontakt med API-et.')).toBeInTheDocument(),
    );
  });

  it('shows the error state when the proxy itself returns an error status', async () => {
    global.fetch = jest.fn().mockResolvedValue({ ok: false }) as unknown as typeof fetch;

    render(<StatusWidget />);

    await waitFor(() =>
      expect(screen.getByText('Får ikke kontakt med API-et.')).toBeInTheDocument(),
    );
  });
});
