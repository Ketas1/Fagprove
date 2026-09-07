import { fireEvent, render, screen } from '@testing-library/react';
import { RowActionsMenu } from './row-actions-menu';

describe('RowActionsMenu', () => {
  it('links "Åpne" to the given href', () => {
    render(<RowActionsMenu openHref="/dashboard/loans/123" />);

    fireEvent.click(screen.getByRole('button', { name: 'Handlinger' }));

    expect(screen.getByRole('menuitem', { name: /Åpne/ })).toHaveAttribute('href', '/dashboard/loans/123');
  });

  it('does not show "Rediger" when no onEdit is given', () => {
    render(<RowActionsMenu openHref="/dashboard/loans/123" />);

    fireEvent.click(screen.getByRole('button', { name: 'Handlinger' }));

    expect(screen.queryByText('Rediger')).not.toBeInTheDocument();
  });

  it('calls onEdit when "Rediger" is chosen', () => {
    const onEdit = jest.fn();
    render(<RowActionsMenu openHref="/dashboard/loans/123" onEdit={onEdit} />);

    fireEvent.click(screen.getByRole('button', { name: 'Handlinger' }));
    fireEvent.click(screen.getByText('Rediger'));

    expect(onEdit).toHaveBeenCalledTimes(1);
  });
});
