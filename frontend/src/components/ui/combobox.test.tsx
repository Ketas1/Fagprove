import { fireEvent, render, screen } from '@testing-library/react';
import { Combobox, type ComboboxItem } from './combobox';

describe('Combobox', () => {
  const items: ComboboxItem[] = [
    { id: 'a', label: 'Kari Nordmann', group: 'Foresatte' },
    { id: 'b', label: 'Ola Nordmann', group: 'Foresatte' },
    { id: 'c', label: 'Ski (SN-1)', group: 'Utstyr' },
  ];

  it('shows the placeholder when nothing is selected', () => {
    render(<Combobox items={items} value={null} onChange={jest.fn()} placeholder="Velg foresatt…" />);

    expect(screen.getByText('Velg foresatt…')).toBeInTheDocument();
  });

  it('shows the selected item label instead of the placeholder', () => {
    render(<Combobox items={items} value="b" onChange={jest.fn()} placeholder="Velg…" />);

    expect(screen.getByText('Ola Nordmann')).toBeInTheDocument();
    expect(screen.queryByText('Velg…')).not.toBeInTheDocument();
  });

  it('filters the list by typed text and calls onChange when an item is picked', () => {
    const onChange = jest.fn();
    render(<Combobox items={items} value={null} onChange={onChange} />);

    fireEvent.click(screen.getByRole('combobox'));
    fireEvent.change(screen.getByPlaceholderText('Søk…'), { target: { value: 'ola' } });

    expect(screen.getByText('Ola Nordmann')).toBeInTheDocument();
    expect(screen.queryByText('Kari Nordmann')).not.toBeInTheDocument();

    fireEvent.click(screen.getByText('Ola Nordmann'));

    expect(onChange).toHaveBeenCalledWith('b');
  });

  it('groups items under a heading per group', () => {
    render(<Combobox items={items} value={null} onChange={jest.fn()} />);

    fireEvent.click(screen.getByRole('combobox'));

    expect(screen.getByText('Foresatte')).toBeInTheDocument();
    expect(screen.getByText('Utstyr')).toBeInTheDocument();
  });
});
