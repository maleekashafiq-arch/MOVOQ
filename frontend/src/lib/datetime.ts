function pad(value: number) {
  return String(value).padStart(2, '0')
}

/** Value for `<input type="datetime-local">` from an ISO timestamp. */
export function toLocalInput(iso: string) {
  const date = new Date(iso)
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

export function fromLocalInput(value: string) {
  return new Date(value).toISOString()
}

export function daysFromNow(days: number) {
  return toLocalInput(new Date(Date.now() + days * 86_400_000).toISOString())
}
