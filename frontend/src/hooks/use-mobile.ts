import * as React from "react"

const MOBILE_BREAKPOINT = 768

function subscribe(callback: () => void) {
  const mql = window.matchMedia(`(max-width: ${MOBILE_BREAKPOINT - 1}px)`)
  mql.addEventListener("change", callback)
  return () => mql.removeEventListener("change", callback)
}

function getSnapshot() {
  return window.innerWidth < MOBILE_BREAKPOINT
}

function getServerSnapshot() {
  return false
}

// useSyncExternalStore, not useState+useEffect: this is the API React
// recommends for subscribing to an external source like matchMedia, and it
// is SSR-safe without triggering a setState-in-effect lint violation.
export function useIsMobile() {
  return React.useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot)
}
