import { resolveBackendUrl } from '@/lib/api';

describe('resolveBackendUrl', () => {
  const original = process.env.API_BASE_URL;

  afterEach(() => {
    process.env.API_BASE_URL = original;
  });

  it('joins the path segments under /api on the backend', () => {
    process.env.API_BASE_URL = 'http://localhost:5080';

    expect(resolveBackendUrl(['health'], '').toString()).toBe('http://localhost:5080/api/health');
  });

  it('preserves multi-segment paths', () => {
    process.env.API_BASE_URL = 'http://localhost:5080';

    expect(resolveBackendUrl(['loans', '123'], '').toString()).toBe(
      'http://localhost:5080/api/loans/123',
    );
  });

  it('forwards a query string', () => {
    process.env.API_BASE_URL = 'http://localhost:5080';

    expect(resolveBackendUrl(['loans'], '?status=overdue').toString()).toBe(
      'http://localhost:5080/api/loans?status=overdue',
    );
  });

  it('falls back to localhost when the variable is unset', () => {
    delete process.env.API_BASE_URL;

    expect(resolveBackendUrl(['health'], '').toString()).toBe('http://localhost:5080/api/health');
  });
});
