import { render, screen } from '@testing-library/react';
import { HealthStatus } from '@/components/HealthStatus';

describe('HealthStatus', () => {
  it('reports the database as connected when it is up', () => {
    render(<HealthStatus health={{ status: 'ok', database: 'up' }} />);

    expect(screen.getByText('Kjører')).toBeInTheDocument();
    expect(screen.getByText('Tilkoblet')).toBeInTheDocument();
  });

  it('reports the database as disconnected when it is down', () => {
    render(<HealthStatus health={{ status: 'ok', database: 'down' }} />);

    expect(screen.getByText('Ikke tilkoblet')).toBeInTheDocument();
  });

  it('shows an error when the API could not be reached', () => {
    render(<HealthStatus health={null} />);

    expect(screen.getByText('Får ikke kontakt med API-et.')).toBeInTheDocument();
  });
});
