import { contactStatusInfo } from './contact-status';

describe('contactStatusInfo', () => {
  it('flags a loan nobody has contacted about', () => {
    expect(contactStatusInfo({ contactAttemptCount: 0, lastContactedAt: null })).toEqual({
      label: 'Ikke kontaktet',
      tone: 'warning',
    });
  });

  it('shows the attempt count and the date of the last one', () => {
    expect(
      contactStatusInfo({ contactAttemptCount: 2, lastContactedAt: '2026-09-05T10:30:00+02:00' }),
    ).toEqual({ label: '2 forsøk · 05.09', tone: 'success' });
  });

  it('uses the same wording for a single attempt - "forsøk" has no plural form', () => {
    expect(
      contactStatusInfo({ contactAttemptCount: 1, lastContactedAt: '2026-09-05T10:30:00+02:00' }),
    ).toEqual({ label: '1 forsøk · 05.09', tone: 'success' });
  });

  it('pads single-digit days and months', () => {
    expect(
      contactStatusInfo({ contactAttemptCount: 1, lastContactedAt: '2026-01-02T12:00:00+01:00' }),
    ).toEqual({ label: '1 forsøk · 02.01', tone: 'success' });
  });

  it('treats a null timestamp as never contacted even if a count slipped through', () => {
    expect(contactStatusInfo({ contactAttemptCount: 3, lastContactedAt: null })).toEqual({
      label: 'Ikke kontaktet',
      tone: 'warning',
    });
  });
});
