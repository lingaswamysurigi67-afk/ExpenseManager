import { useEffect, useState } from 'react'

// Delay propagating a rapidly-changing value (e.g. a search box) so we don't
// fire a server request on every keystroke.
export function useDebouncedValue<T>(value: T, delay = 350): T {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const t = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(t)
  }, [value, delay])
  return debounced
}
