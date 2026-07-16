import { useCallback, useEffect, useState } from 'react'

interface CalculatorProps {
  open: boolean
  onClose: () => void
}

type Op = '+' | '-' | '×' | '÷'

const compute = (a: number, b: number, op: Op): number => {
  switch (op) {
    case '+': return a + b
    case '-': return a - b
    case '×': return a * b
    case '÷': return b === 0 ? NaN : a / b
  }
}

const format = (n: number): string => {
  if (!isFinite(n)) return 'Error'
  // Trim floating point noise, keep it readable.
  const rounded = Math.round((n + Number.EPSILON) * 1e10) / 1e10
  return String(rounded)
}

export default function Calculator({ open, onClose }: CalculatorProps) {
  const [display, setDisplay] = useState('0')
  const [previous, setPrevious] = useState<number | null>(null)
  const [operator, setOperator] = useState<Op | null>(null)
  const [waiting, setWaiting] = useState(false)

  const clearAll = useCallback(() => {
    setDisplay('0')
    setPrevious(null)
    setOperator(null)
    setWaiting(false)
  }, [])

  const inputDigit = useCallback((d: string) => {
    setDisplay((cur) => {
      if (waiting) { setWaiting(false); return d }
      if (cur === '0') return d
      if (cur === 'Error') return d
      if (cur.replace('-', '').replace('.', '').length >= 15) return cur
      return cur + d
    })
  }, [waiting])

  const inputDot = useCallback(() => {
    setDisplay((cur) => {
      if (waiting) { setWaiting(false); return '0.' }
      if (cur === 'Error') return '0.'
      return cur.includes('.') ? cur : cur + '.'
    })
  }, [waiting])

  const backspace = useCallback(() => {
    setDisplay((cur) => {
      if (waiting || cur === 'Error') return cur
      if (cur.length <= 1 || (cur.length === 2 && cur.startsWith('-'))) return '0'
      return cur.slice(0, -1)
    })
  }, [waiting])

  const percent = useCallback(() => {
    setDisplay((cur) => format((parseFloat(cur) || 0) / 100))
  }, [])

  const toggleSign = useCallback(() => {
    setDisplay((cur) => (cur === '0' || cur === 'Error' ? cur : format(-(parseFloat(cur) || 0))))
  }, [])

  const applyOperator = useCallback((nextOp: Op) => {
    const value = parseFloat(display) || 0
    if (previous === null) {
      setPrevious(value)
    } else if (operator && !waiting) {
      const result = compute(previous, value, operator)
      setPrevious(result)
      setDisplay(format(result))
    }
    setOperator(nextOp)
    setWaiting(true)
  }, [display, previous, operator, waiting])

  const equals = useCallback(() => {
    if (operator === null || previous === null) return
    const value = parseFloat(display) || 0
    const result = compute(previous, value, operator)
    setDisplay(format(result))
    setPrevious(null)
    setOperator(null)
    setWaiting(true)
  }, [operator, previous, display])

  // Keyboard support while the calculator is open.
  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => {
      const k = e.key
      if (k >= '0' && k <= '9') inputDigit(k)
      else if (k === '.') inputDot()
      else if (k === '+') applyOperator('+')
      else if (k === '-') applyOperator('-')
      else if (k === '*') applyOperator('×')
      else if (k === '/') { e.preventDefault(); applyOperator('÷') }
      else if (k === '%') percent()
      else if (k === 'Enter' || k === '=') { e.preventDefault(); equals() }
      else if (k === 'Backspace') backspace()
      else if (k === 'Escape') onClose()
      else if (k === 'c' || k === 'C') clearAll()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open, inputDigit, inputDot, applyOperator, percent, equals, backspace, clearAll, onClose])

  // Reset when reopened.
  useEffect(() => { if (open) clearAll() }, [open, clearAll])

  if (!open) return null

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <div className="card modal calc-modal fade-up" onMouseDown={(e) => e.stopPropagation()}>
        <div className="calc-head">
          <h3 className="section-title" style={{ margin: 0 }}>Calculator</h3>
          <button className="btn ghost icon" title="Close" onClick={onClose}>✕</button>
        </div>

        <div className="calc-display" aria-live="polite">
          <div className="calc-expr">{previous !== null && operator ? `${format(previous)} ${operator}` : '\u00A0'}</div>
          <div className="calc-value">{display}</div>
        </div>

        <div className="calc-keys">
          <button className="calc-btn fn" onClick={clearAll}>AC</button>
          <button className="calc-btn fn" onClick={toggleSign}>±</button>
          <button className="calc-btn fn" onClick={percent}>%</button>
          <button className="calc-btn op" onClick={() => applyOperator('÷')}>÷</button>

          <button className="calc-btn" onClick={() => inputDigit('7')}>7</button>
          <button className="calc-btn" onClick={() => inputDigit('8')}>8</button>
          <button className="calc-btn" onClick={() => inputDigit('9')}>9</button>
          <button className="calc-btn op" onClick={() => applyOperator('×')}>×</button>

          <button className="calc-btn" onClick={() => inputDigit('4')}>4</button>
          <button className="calc-btn" onClick={() => inputDigit('5')}>5</button>
          <button className="calc-btn" onClick={() => inputDigit('6')}>6</button>
          <button className="calc-btn op" onClick={() => applyOperator('-')}>−</button>

          <button className="calc-btn" onClick={() => inputDigit('1')}>1</button>
          <button className="calc-btn" onClick={() => inputDigit('2')}>2</button>
          <button className="calc-btn" onClick={() => inputDigit('3')}>3</button>
          <button className="calc-btn op" onClick={() => applyOperator('+')}>+</button>

          <button className="calc-btn" onClick={() => inputDigit('0')}>0</button>
          <button className="calc-btn" onClick={inputDot}>.</button>
          <button className="calc-btn fn" onClick={backspace} title="Backspace">⌫</button>
          <button className="calc-btn eq" onClick={equals}>=</button>
        </div>
      </div>
    </div>
  )
}
