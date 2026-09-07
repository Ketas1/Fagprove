// Adds matchers such as toBeInTheDocument to expect().
import '@testing-library/jest-dom';

// jsdom does not implement ResizeObserver, which @base-ui/react's floating
// components (Popover, Select, Menu, and anything built on them - Combobox,
// DropdownMenu, DatePicker) use for anchor positioning. A no-op stand-in is
// enough: tests only need the component to render and respond to
// interaction, not real layout measurement.
class ResizeObserverStub {
  observe() {}
  unobserve() {}
  disconnect() {}
}

// This setup file also runs for suites using `@jest-environment node` (see
// src/app/api/proxy-route.test.ts), where neither global exists at all -
// guard both instead of assuming jsdom.
if (typeof global.ResizeObserver === 'undefined') {
  global.ResizeObserver = ResizeObserverStub;
}

// jsdom also does not implement scrollIntoView, which cmdk (the Command
// primitive, used by the Combobox) calls when highlighting an item.
if (typeof Element !== 'undefined') {
  Element.prototype.scrollIntoView = () => {};
}
