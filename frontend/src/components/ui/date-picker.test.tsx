import { fireEvent, render, screen } from '@testing-library/react';
import { DatePicker } from './date-picker';

describe('DatePicker', () => {
  it('shows the placeholder when no value is set', () => {
    render(<DatePicker value="" onChange={jest.fn()} placeholder="Velg fødselsdato" />);

    expect(screen.getByText('Velg fødselsdato')).toBeInTheDocument();
  });

  it('shows the formatted value when set, not the placeholder', () => {
    render(<DatePicker value="2018-08-12" onChange={jest.fn()} placeholder="Velg fødselsdato" />);

    const expected = new Date(2018, 7, 12).toLocaleDateString('nb-NO');
    expect(screen.getByText(expected)).toBeInTheDocument();
    expect(screen.queryByText('Velg fødselsdato')).not.toBeInTheDocument();
  });

  it('opens a calendar when the trigger is clicked', () => {
    render(<DatePicker value="2018-08-12" onChange={jest.fn()} />);

    fireEvent.click(screen.getByRole('button'));

    expect(screen.getByRole('grid')).toBeInTheDocument();
  });
});
