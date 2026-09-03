import { apiUrl } from '@/lib/api';

describe('apiUrl', () => {
  const original = process.env.NEXT_PUBLIC_API_BASE_URL;

  afterEach(() => {
    process.env.NEXT_PUBLIC_API_BASE_URL = original;
  });

  it('joins the base URL and the path', () => {
    process.env.NEXT_PUBLIC_API_BASE_URL = 'http://localhost:5080';

    expect(apiUrl('/api/health')).toBe('http://localhost:5080/api/health');
  });

  it('adds a missing leading slash', () => {
    process.env.NEXT_PUBLIC_API_BASE_URL = 'http://localhost:5080';

    expect(apiUrl('api/health')).toBe('http://localhost:5080/api/health');
  });

  it('does not produce a double slash when the base URL has a trailing one', () => {
    process.env.NEXT_PUBLIC_API_BASE_URL = 'http://localhost:5080/';

    expect(apiUrl('/api/health')).toBe('http://localhost:5080/api/health');
  });

  it('falls back to localhost when the variable is unset', () => {
    delete process.env.NEXT_PUBLIC_API_BASE_URL;

    expect(apiUrl('/api/health')).toBe('http://localhost:5080/api/health');
  });
});
