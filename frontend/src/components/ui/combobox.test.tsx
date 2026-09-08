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

  it('offers a create-new row for unmatched text when onCreateNew is given', () => {
    const onCreateNew = jest.fn();
    render(<Combobox items={items} value={null} onChange={jest.fn()} onCreateNew={onCreateNew} />);

    fireEvent.click(screen.getByRole('combobox'));
    fireEvent.change(screen.getByPlaceholderText('Søk…'), { target: { value: 'Snowboard' } });

    const createRow = screen.getByText('Opprett «Snowboard»');
    expect(createRow).toBeInTheDocument();

    fireEvent.click(createRow);

    expect(onCreateNew).toHaveBeenCalledWith('Snowboard');
  });

  it('does not offer a create-new row when the typed text exactly matches an existing item', () => {
    render(<Combobox items={items} value={null} onChange={jest.fn()} onCreateNew={jest.fn()} />);

    fireEvent.click(screen.getByRole('combobox'));
    fireEvent.change(screen.getByPlaceholderText('Søk…'), { target: { value: 'Ola Nordmann' } });

    expect(screen.queryByText('Opprett «Ola Nordmann»')).not.toBeInTheDocument();
  });

  it('never offers a create-new row when onCreateNew is not given', () => {
    render(<Combobox items={items} value={null} onChange={jest.fn()} />);

    fireEvent.click(screen.getByRole('combobox'));
    fireEvent.change(screen.getByPlaceholderText('Søk…'), { target: { value: 'Snowboard' } });

    expect(screen.queryByText(/Opprett/)).not.toBeInTheDocument();
    expect(screen.getByText('Ingen treff.')).toBeInTheDocument();
  });
});
