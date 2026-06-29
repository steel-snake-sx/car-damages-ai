type InputProps = {
  label: string
  value: string | number
  onChange: (value: string) => void
  type?: 'text' | 'email' | 'password' | 'tel' | 'number'
  placeholder?: string
  required?: boolean
  min?: number
  max?: number
  error?: string
}

export function Input({
  label,
  value,
  onChange,
  type = 'text',
  placeholder,
  required,
  min,
  max,
  error,
}: InputProps) {
  return (
    <div className="input-group">
      <label>{label}</label>
      <input
        type={type}
        value={value}
        placeholder={placeholder}
        required={required}
        min={min}
        max={max}
        onChange={(event) => onChange(event.target.value)}
      />
      {error ? <p className="field-error">{error}</p> : null}
    </div>
  )
}
