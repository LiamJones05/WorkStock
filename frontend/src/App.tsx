import { FormEvent, useEffect, useMemo, useState } from 'react'
import { api, apiBlobUrl, auth } from './api'

type Page = 'today' | 'jobs' | 'customers' | 'team'
type Role = 'Owner' | 'Manager' | 'Employee'
type Me = {
  user: { id: string; displayName: string; role: Role }
  organisation: { id: string; name: string; activeEmployeeCount: number }
}
type UserAccount = { id: string; displayName: string; email: string; role: Role; isActive: boolean; createdAt: string }

const fmt = (value?: string) => value ? new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) : 'Not scheduled'
const isManager = (me: Me | null) => me?.user.role === 'Owner' || me?.user.role === 'Manager'

export default function App() {
  const [me, setMe] = useState<Me | null>(null)
  const [page, setPage] = useState<Page>('today')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(true)

  const loadSession = async () => {
    if (!auth.token) {
      setLoading(false)
      return
    }
    try {
      setMe(await api<Me>('/api/auth/me'))
    } catch {
      setMe(null)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadSession()
    const listener = () => setMe(null)
    window.addEventListener('workstock:logout', listener)
    return () => window.removeEventListener('workstock:logout', listener)
  }, [])

  if (loading) return <main className="centred"><div className="loader" />Loading Workstock...</main>
  if (!me) return <Auth onAuthenticated={() => { void loadSession(); setMessage('Welcome to Workstock.') }} />

  const signOut = async () => {
    try {
      await api('/api/auth/logout', { method: 'POST' })
    } finally {
      auth.token = null
      setMe(null)
    }
  }
  const pages: [Page, string][] = isManager(me) ? [['today', 'Today'], ['jobs', 'Jobs'], ['customers', 'Customers'], ['team', 'Team']] : [['today', 'Today'], ['jobs', 'Jobs']]

  return <div className="app-shell">
    <aside>
      <div className="brand"><img src="/workstock-mark.svg" alt="" /> <span>workstock</span></div>
      <nav>{pages.map(([key, label]) => <button className={page === key ? 'active' : ''} onClick={() => setPage(key)} key={key}>{label}</button>)}</nav>
      <div className="account">
        <strong>{me.user.displayName}</strong>
        <small>{me.organisation.name}</small>
        <small>{me.organisation.activeEmployeeCount} active employees</small>
        <button className="text" onClick={signOut}>Sign out</button>
      </div>
    </aside>
    <main className="content">
      {message && <div className="toast">{message}<button onClick={() => setMessage('')} aria-label="Dismiss">x</button></div>}
      {page === 'today' && <Today onOpenJob={() => setPage('jobs')} />}
      {page === 'jobs' && <Jobs me={me} onMessage={setMessage} />}
      {page === 'customers' && isManager(me) && <Customers onMessage={setMessage} />}
      {page === 'team' && isManager(me) && <Team me={me} onMessage={setMessage} onRefreshMe={loadSession} />}
    </main>
  </div>
}

function Auth({ onAuthenticated }: { onAuthenticated: () => void }) {
  const [register, setRegister] = useState(false)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setBusy(true)
    setError('')
    const data = Object.fromEntries(new FormData(event.currentTarget))
    try {
      const result: any = await api(register ? '/api/auth/register' : '/api/auth/login', {
        method: 'POST',
        body: JSON.stringify(register ? { organisationName: data.organisationName, displayName: data.displayName, email: data.email, password: data.password } : data)
      })
      auth.token = result.token
      onAuthenticated()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unable to sign in.')
    } finally {
      setBusy(false)
    }
  }

  return <main className="auth"><section>
    <div className="brand"><img src="/workstock-mark.svg" alt="" /> <span>workstock</span></div>
    <h1>{register ? 'Start organising your work.' : 'Welcome back.'}</h1>
    <p>Jobs, customers and site work, all in one place.</p>
    <form onSubmit={submit}>
      {register && <>
        <label>Business name<input name="organisationName" required minLength={2} placeholder="Greenfield Services" /></label>
        <label>Your name<input name="displayName" required minLength={2} autoComplete="name" /></label>
      </>}
      <label>Email<input name="email" required type="email" autoComplete="email" /></label>
      <label>Password<input name="password" required type="password" minLength={12} autoComplete={register ? 'new-password' : 'current-password'} /></label>
      {error && <p className="error">{error}</p>}
      <button className="primary" disabled={busy}>{busy ? 'Please wait...' : register ? 'Create free account' : 'Sign in'}</button>
    </form>
    <button className="text switch" onClick={() => { setRegister(!register); setError('') }}>{register ? 'Already have an account? Sign in' : 'New to Workstock? Create an account'}</button>
  </section></main>
}

function Today({ onOpenJob }: { onOpenJob: () => void }) {
  const [data, setData] = useState<any>()
  const [error, setError] = useState('')
  useEffect(() => { api<any>('/api/dashboard').then(setData).catch((e: unknown) => setError(e instanceof Error ? e.message : 'Unable to load dashboard.')) }, [])
  if (error) return <ErrorBox text={error} />
  if (!data) return <Loading />

  return <>
    <header><div><p className="eyebrow">YOUR WORKDAY</p><h1>{new Intl.DateTimeFormat(undefined, { weekday: 'long', day: 'numeric', month: 'long' }).format(new Date())}</h1><p className="muted">A clear view of what needs your attention.</p></div><button className="primary" onClick={onOpenJob}>View all jobs</button></header>
    <section className="metrics"><Metric value={data.summary.active} label="Active jobs" /><Metric value={data.summary.awaiting} label="Awaiting action" /><Metric value={data.summary.overdue} label="Overdue" danger /></section>
    <section className="panel"><div className="panel-title"><h2>Today's schedule</h2><span>{data.today.length} jobs</span></div>{data.today.length ? <div className="schedule">{data.today.map((job: any) => <article key={job.id}><time>{job.scheduledStart ? new Date(job.scheduledStart).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '-'}</time><div><strong>{job.title}</strong><p>{job.customer} - {job.site?.addressLine1 ?? 'No site selected'}</p></div><span className={`badge ${colour(job.status)}`}>{job.status}</span></article>)}</div> : <Empty title="Nothing scheduled today" body="Create a job and add a start time to see it here." />}</section>
    {data.overdue.length > 0 && <section className="panel warning"><div className="panel-title"><h2>Needs attention</h2></div>{data.overdue.map((job: any) => <div className="compact" key={job.id}><strong>{job.jobNumber} - {job.title}</strong><span>Due {fmt(job.dueDate)}</span></div>)}</section>}
  </>
}

function Jobs({ me, onMessage }: { me: Me; onMessage: (text: string) => void }) {
  const [jobs, setJobs] = useState<any[]>([])
  const [statuses, setStatuses] = useState<any[]>([])
  const [selected, setSelected] = useState<any>()
  const [creating, setCreating] = useState(false)
  const [query, setQuery] = useState('')
  const [error, setError] = useState('')

  const load = async () => {
    try {
      setJobs((await api<any>(`/api/jobs?view=active&q=${encodeURIComponent(query)}`)).items)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unable to load jobs.')
    }
  }
  useEffect(() => { void load() }, [query])
  useEffect(() => { void api<any[]>('/api/jobs/statuses').then(setStatuses) }, [])

  return <>
    <header><div><p className="eyebrow">OPERATIONS</p><h1>{isManager(me) ? 'Jobs' : 'My jobs'}</h1></div>{isManager(me) && <button className="primary" onClick={() => setCreating(true)}>New job</button>}</header>
    <div className="toolbar"><input aria-label="Search jobs" value={query} onChange={e => setQuery(e.target.value)} placeholder="Search job number, customer or title" /></div>
    {error && <ErrorBox text={error} />}
    <section className="panel table-panel"><div className="job-table heading"><span>Job</span><span>Customer & site</span><span>Schedule</span><span>Status</span></div>{jobs.map(job => <button className="job-table row" onClick={() => setSelected(job)} key={job.id}><span><strong>{job.jobNumber}</strong><small>{job.title}</small></span><span>{job.customer.name}<small>{job.site?.name ?? 'No site'}</small></span><span>{fmt(job.scheduledStart)}</span><span className={`badge ${colour(job.status.name)}`}>{job.status.name}</span></button>)}{!jobs.length && <Empty title="No active jobs" body={isManager(me) ? 'Create your first job to start your schedule.' : 'Assigned jobs will appear here.'} />}</section>
    {selected && <JobDetail id={selected.id} statuses={statuses} canDeleteDocuments={isManager(me)} onClose={() => setSelected(undefined)} onChanged={() => { void load(); onMessage('Job updated.') }} />}
    {creating && <JobForm statuses={statuses} onClose={() => setCreating(false)} onCreated={() => { setCreating(false); void load(); onMessage('Job created.') }} />}
  </>
}

function Customers({ onMessage }: { onMessage: (text: string) => void }) {
  const [data, setData] = useState<any[]>([])
  const [creating, setCreating] = useState(false)
  const [query, setQuery] = useState('')
  const load = async () => setData((await api<any>(`/api/customers?q=${encodeURIComponent(query)}`)).items)
  useEffect(() => { void load() }, [query])
  return <>
    <header><div><p className="eyebrow">CONTACTS</p><h1>Customers</h1></div><button className="primary" onClick={() => setCreating(true)}>New customer</button></header>
    <div className="toolbar"><input value={query} onChange={e => setQuery(e.target.value)} placeholder="Search name, company or email" /></div>
    <section className="panel customer-list">{data.map(customer => <article key={customer.id}><div className="avatar">{customer.name.slice(0, 1)}</div><div><strong>{customer.name}</strong><p>{customer.companyName ?? customer.email ?? 'No contact details'}</p></div><span>{customer.city ?? '-'}</span></article>)}{!data.length && <Empty title="No customers yet" body="Add a customer before creating a job." />}</section>
    {creating && <CustomerForm onClose={() => setCreating(false)} onCreated={() => { setCreating(false); void load(); onMessage('Customer created.') }} />}
  </>
}

function Team({ me, onMessage, onRefreshMe }: { me: Me; onMessage: (text: string) => void; onRefreshMe: () => Promise<void> }) {
  const [users, setUsers] = useState<UserAccount[]>([])
  const [count, setCount] = useState(me.organisation.activeEmployeeCount)
  const [creating, setCreating] = useState(false)
  const [editing, setEditing] = useState<UserAccount | null>(null)
  const [error, setError] = useState('')

  const load = async () => {
    try {
      const result = await api<{ activeEmployeeCount: number; users: UserAccount[] }>('/api/users')
      setUsers(result.users)
      setCount(result.activeEmployeeCount)
      await onRefreshMe()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unable to load employees.')
    }
  }
  useEffect(() => { void load() }, [])

  return <>
    <header><div><p className="eyebrow">ADMIN</p><h1>Team</h1><p className="muted">{count} active employees for future seat billing.</p></div><button className="primary" onClick={() => setCreating(true)}>Add employee</button></header>
    {error && <ErrorBox text={error} />}
    <section className="panel table-panel"><div className="team-table heading"><span>Name</span><span>Email</span><span>Role</span><span>Status</span></div>{users.map(user => <button className="team-table row" onClick={() => setEditing(user)} key={user.id}><span><strong>{user.displayName}</strong><small>Added {new Date(user.createdAt).toLocaleDateString()}</small></span><span>{user.email}</span><span>{user.role}</span><span className={`badge ${user.isActive ? 'completed' : 'cancelled'}`}>{user.isActive ? 'Active' : 'Inactive'}</span></button>)}</section>
    {creating && <UserForm currentRole={me.user.role} onClose={() => setCreating(false)} onSaved={() => { setCreating(false); void load(); onMessage('Employee added.') }} />}
    {editing && <UserForm currentRole={me.user.role} user={editing} onClose={() => setEditing(null)} onSaved={() => { setEditing(null); void load(); onMessage('Employee updated.') }} />}
  </>
}

function UserForm({ currentRole, user, onClose, onSaved }: { currentRole: Role; user?: UserAccount; onClose: () => void; onSaved: () => void }) {
  const [error, setError] = useState('')
  const roleOptions = currentRole === 'Owner' ? ['Owner', 'Manager', 'Employee'] : ['Employee']
  const submit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const f = Object.fromEntries(new FormData(e.currentTarget))
    const body = user
      ? { displayName: f.displayName, role: f.role, isActive: f.isActive === 'on' }
      : { displayName: f.displayName, email: f.email, password: f.password, role: f.role }
    try {
      await api(user ? `/api/users/${user.id}` : '/api/users', { method: user ? 'PATCH' : 'POST', body: JSON.stringify(body) })
      onSaved()
    } catch (x) {
      setError(x instanceof Error ? x.message : 'Unable to save employee.')
    }
  }

  return <Modal onClose={onClose}><h2>{user ? 'Edit employee' : 'Add employee'}</h2><form onSubmit={submit}>
    <label>Name<input name="displayName" required defaultValue={user?.displayName} /></label>
    {!user && <label>Email<input name="email" type="email" required /></label>}
    {!user && <label>Temporary password<input name="password" type="password" required minLength={12} /></label>}
    <label>Role<select name="role" defaultValue={user?.role ?? 'Employee'}>{roleOptions.map(role => <option key={role} value={role}>{role}</option>)}</select></label>
    {user && <label className="check"><input type="checkbox" name="isActive" defaultChecked={user.isActive} /> Active account</label>}
    {error && <p className="error">{error}</p>}
    <div className="actions"><button type="button" onClick={onClose}>Cancel</button><button className="primary">Save</button></div>
  </form></Modal>
}

function JobDetail({ id, statuses, canDeleteDocuments, onClose, onChanged }: any) {
  const [data, setData] = useState<any>()
  const [note, setNote] = useState('')
  const [item, setItem] = useState('')
  const load = async () => setData(await api<any>(`/api/jobs/${id}`))
  useEffect(() => { void load() }, [id])
  if (!data) return <Modal onClose={onClose}><Loading /></Modal>
  const job = data.job

  const changeStatus = async (event: any) => {
    await api(`/api/jobs/${id}/status`, { method: 'POST', body: JSON.stringify({ jobStatusId: event.target.value }) })
    await load()
    onChanged()
  }
  const addNote = async (event: FormEvent) => {
    event.preventDefault()
    if (!note.trim()) return
    await api(`/api/jobs/${id}/notes`, { method: 'POST', body: JSON.stringify({ body: note }) })
    setNote('')
    await load()
  }
  const addItem = async (event: FormEvent) => {
    event.preventDefault()
    if (!item.trim()) return
    await api(`/api/jobs/${id}/items`, { method: 'POST', body: JSON.stringify({ name: item, quantity: 1, unit: 'each' }) })
    setItem('')
    await load()
  }

  return <Modal onClose={onClose}><div className="detail-head"><div><p className="eyebrow">{job.jobNumber}</p><h2>{job.title}</h2><p>{job.customer.name} - {job.site?.addressLine1 ?? 'No site selected'}</p></div><button className="icon" onClick={onClose} aria-label="Close">x</button></div>
    <label>Status<select value={job.jobStatusId} onChange={changeStatus}>{statuses.map((s: any) => <option value={s.id} key={s.id}>{s.name}</option>)}</select></label>
    <JobImages jobId={id} documents={data.documents} canDelete={canDeleteDocuments} onChanged={load} />
    <div className="detail-grid"><section><h3>Schedule</h3><p>{fmt(job.scheduledStart)}</p><h3>Items required</h3>{data.items.map((x: any) => <p className="item" key={x.id}>{x.quantity} {x.unit} - {x.name}</p>)}<form className="inline-form" onSubmit={addItem}><input value={item} onChange={e => setItem(e.target.value)} placeholder="Add item required" /><button>Add</button></form></section><section><h3>Activity</h3>{data.activity.slice(0, 5).map((x: any) => <p className="activity" key={x.id}><strong>{x.description}</strong><small>{fmt(x.createdAt)}</small></p>)}<form className="inline-form" onSubmit={addNote}><input value={note} onChange={e => setNote(e.target.value)} placeholder="Add a work note" /><button>Add</button></form></section></div>
  </Modal>
}

function JobImages({ jobId, documents, canDelete, onChanged }: { jobId: string; documents: any[]; canDelete: boolean; onChanged: () => Promise<void> }) {
  const [uploading, setUploading] = useState(false)
  const [preview, setPreview] = useState<any | null>(null)
  const images = useMemo(() => documents.filter(x => String(x.contentType).startsWith('image/')), [documents])

  const upload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return
    const form = new FormData()
    form.append('file', file)
    setUploading(true)
    try {
      await api(`/api/jobs/${jobId}/documents`, { method: 'POST', body: form })
      await onChanged()
    } finally {
      setUploading(false)
      event.target.value = ''
    }
  }

  const remove = async (documentId: string) => {
    await api(`/api/documents/${documentId}`, { method: 'DELETE' })
    await onChanged()
  }

  return <section className="image-section">
    <div className="panel-title"><h2>Job images</h2><label className="upload-button">{uploading ? 'Uploading...' : 'Add image'}<input type="file" accept="image/jpeg,image/png,image/webp" capture="environment" onChange={upload} disabled={uploading} /></label></div>
    {images.length ? <div className="image-grid">{images.map(doc => <ImageThumb document={doc} key={doc.id} onOpen={() => setPreview(doc)} />)}</div> : <Empty title="No images yet" body="Upload photos from desktop, tablet or phone." />}
    {preview && <ImageViewer document={preview} canDelete={canDelete} onClose={() => setPreview(null)} onDelete={async () => { await remove(preview.id); setPreview(null) }} />}
  </section>
}

function ImageThumb({ document, onOpen }: { document: any; onOpen: () => void }) {
  const [url, setUrl] = useState('')
  useEffect(() => {
    let active = true
    let objectUrl = ''
    apiBlobUrl(`/api/documents/${document.id}`).then(next => { objectUrl = next; if (active) setUrl(next) })
    return () => { active = false; if (objectUrl) URL.revokeObjectURL(objectUrl) }
  }, [document.id])
  return <button className="image-thumb" onClick={onOpen}>{url ? <img src={url} alt={document.fileName} /> : <div className="loader" />}<span>{document.fileName}</span></button>
}

function ImageViewer({ document, canDelete, onClose, onDelete }: { document: any; canDelete: boolean; onClose: () => void; onDelete: () => Promise<void> }) {
  const [url, setUrl] = useState('')
  useEffect(() => {
    let objectUrl = ''
    apiBlobUrl(`/api/documents/${document.id}`).then(next => { objectUrl = next; setUrl(next) })
    return () => { if (objectUrl) URL.revokeObjectURL(objectUrl) }
  }, [document.id])
  return <div className="image-viewer" role="dialog" aria-modal="true"><div className="image-viewer-bar"><strong>{document.fileName}</strong><div>{canDelete && <button onClick={onDelete}>Delete</button>}<button onClick={onClose}>Close</button></div></div>{url ? <img src={url} alt={document.fileName} /> : <Loading />}</div>
}

function JobForm({ statuses, onClose, onCreated }: any) {
  const [customers, setCustomers] = useState<any[]>([])
  const [error, setError] = useState('')
  useEffect(() => { api<any>('/api/customers?pageSize=100').then(x => setCustomers(x.items)) }, [])
  const submit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const f = Object.fromEntries(new FormData(e.currentTarget))
    try {
      await api('/api/jobs', { method: 'POST', body: JSON.stringify({ ...f, scheduledStart: f.scheduledStart ? new Date(String(f.scheduledStart)).toISOString() : null, jobStatusId: f.jobStatusId, customerId: f.customerId }) })
      onCreated()
    } catch (x) {
      setError(x instanceof Error ? x.message : 'Unable to create job.')
    }
  }
  return <Modal onClose={onClose}><h2>Create a job</h2><form onSubmit={submit}><label>Title<input name="title" required /></label><label>Customer<select name="customerId" required><option value="">Choose customer</option>{customers.map(c => <option value={c.id} key={c.id}>{c.name}</option>)}</select></label><label>Status<select name="jobStatusId" defaultValue={statuses[0]?.id}>{statuses.map((s: any) => <option value={s.id} key={s.id}>{s.name}</option>)}</select></label><label>Scheduled start<input type="datetime-local" name="scheduledStart" /></label><label>Description<textarea name="description" /></label>{error && <p className="error">{error}</p>}<div className="actions"><button type="button" onClick={onClose}>Cancel</button><button className="primary">Create job</button></div></form></Modal>
}

function CustomerForm({ onClose, onCreated }: any) {
  const [error, setError] = useState('')
  const submit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    try {
      await api('/api/customers', { method: 'POST', body: JSON.stringify(Object.fromEntries(new FormData(e.currentTarget))) })
      onCreated()
    } catch (x) {
      setError(x instanceof Error ? x.message : 'Unable to create customer.')
    }
  }
  return <Modal onClose={onClose}><h2>Add customer</h2><form onSubmit={submit}><label>Name<input name="name" required /></label><label>Company<input name="companyName" /></label><label>Email<input type="email" name="email" /></label><label>Phone<input name="phone" /></label><label>Town / city<input name="city" /></label>{error && <p className="error">{error}</p>}<div className="actions"><button type="button" onClick={onClose}>Cancel</button><button className="primary">Add customer</button></div></form></Modal>
}

function Modal({ children, onClose }: any) {
  return <div className="overlay" role="dialog" aria-modal="true"><div className="modal">{children}</div><button className="overlay-close" aria-label="Close" onClick={onClose} /></div>
}
function Metric({ value, label, danger }: any) { return <section className={`metric ${danger ? 'danger' : ''}`}><strong>{value}</strong><span>{label}</span></section> }
function Empty({ title, body }: any) { return <div className="empty"><strong>{title}</strong><p>{body}</p></div> }
function Loading() { return <div className="loading"><div className="loader" />Loading...</div> }
function ErrorBox({ text }: { text: string }) { return <div className="error box">{text}</div> }
function colour(status: string) { return status.toLowerCase().replaceAll(' ', '-') }
