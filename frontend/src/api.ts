const base = import.meta.env.VITE_API_URL ?? ''
export const auth = { get token() { return localStorage.getItem('workstock.token') }, set token(value: string | null) { value ? localStorage.setItem('workstock.token', value) : localStorage.removeItem('workstock.token') } }

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (auth.token) headers.set('Authorization', `Bearer ${auth.token}`)
  if (init.body && !(init.body instanceof FormData)) headers.set('Content-Type', 'application/json')
  const response = await fetch(`${base}${path}`, { ...init, headers })
  if (response.status === 401) { auth.token = null; window.dispatchEvent(new Event('workstock:logout')) }
  if (!response.ok) { const body = await response.json().catch(() => null); throw new Error(body?.error ?? body?.title ?? 'Something went wrong.') }
  return response.status === 204 ? undefined as T : response.json()
}

export async function apiBlobUrl(path: string): Promise<string> {
  const headers = new Headers()
  if (auth.token) headers.set('Authorization', `Bearer ${auth.token}`)
  const response = await fetch(`${base}${path}`, { headers })
  if (response.status === 401) { auth.token = null; window.dispatchEvent(new Event('workstock:logout')) }
  if (!response.ok) throw new Error('Unable to load file.')
  return URL.createObjectURL(await response.blob())
}
