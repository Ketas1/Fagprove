import { fireEvent, render, screen } from '@testing-library/react';
import { SearchInput } from './search-input';

describe('SearchInput', () => {
  it('calls onChange as the user types', () => {
    const onChange = jest.fn();
    render(<SearchInput value="" onChange={onChange} placeholder="Søk…" />);

    fireEvent.change(screen.getByPlaceholderText('Søk…'), { target: { value: 'ski' } });

    expect(onChange).toHaveBeenCalledWith('ski');
  });

  it('does not show a clear button when empty', () => {
    render(<SearchInput value="" onChange={jest.fn()} />);

    expect(screen.queryByRole('button', { name: 'Tøm søk' })).not.toBeInTheDocument();
  });

  it('shows a clear button once there is a value, and clears it on click', () => {
    const onChange = jest.fn();
    render(<SearchInput value="ski" onChange={onChange} />);

    fireEvent.click(screen.getByRole('button', { name: 'Tøm søk' }));

    expect(onChange).toHaveBeenCalledWith('');
  });
});
