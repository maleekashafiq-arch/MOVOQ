import { useState, type FormEvent, type ReactNode } from 'react'
import { AlertTriangle, Loader2, Trophy } from 'lucide-react'
import { useAuth } from '../lib/auth'
import { config, firebaseConfigured } from '../lib/config'

export function SignIn() {
  const { state, signInWithPassword, signInAsDeveloper, signOut } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)
    setBusy(true)

    try {
      await signInWithPassword(email.trim(), password)
    } catch (cause) {
      setError(describe(cause))
      setBusy(false)
    }
  }

  const startDevSession = async () => {
    setError(null)
    setBusy(true)

    try {
      await signInAsDeveloper()
    } catch (cause) {
      setError(describe(cause))
      setBusy(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-neutral-50 p-6">
      <div className="w-full max-w-md">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br from-[#00C9A7] to-[#3A86FF]">
            <Trophy className="h-7 w-7 text-white" />
          </div>
          <h1 className="mt-4 text-2xl font-bold text-neutral-900">MOVOQ Admin</h1>
          <p className="mt-1 text-sm text-neutral-500">
            Operator email and password. Only addresses on the allowlist can open this app.
          </p>
        </div>

        <div className="rounded-xl border border-neutral-200 bg-white p-6 shadow-sm">
          {state.status === 'denied' ? (
            <AccessDenied email={state.email} message={state.message} onBack={() => void signOut()} />
          ) : (
            <>
              <form onSubmit={submit} className="space-y-4">
                <Field label="Email">
                  <input
                    type="email"
                    required
                    autoComplete="username"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    disabled={!firebaseConfigured || busy}
                    className="w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none transition-colors focus:border-[#3A86FF] disabled:bg-neutral-50 disabled:text-neutral-400"
                  />
                </Field>

                <Field label="Password">
                  <input
                    type="password"
                    required
                    autoComplete="current-password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    disabled={!firebaseConfigured || busy}
                    className="w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none transition-colors focus:border-[#3A86FF] disabled:bg-neutral-50 disabled:text-neutral-400"
                  />
                </Field>

                {error && (
                  <p className="flex items-start gap-2 rounded-lg bg-red-50 p-3 text-sm text-red-700">
                    <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0" />
                    {error}
                  </p>
                )}

                <button
                  type="submit"
                  disabled={!firebaseConfigured || busy}
                  className="flex w-full items-center justify-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2.5 text-sm font-semibold text-white transition-opacity hover:opacity-90 disabled:opacity-50"
                >
                  {busy && <Loader2 className="h-4 w-4 animate-spin" />}
                  Sign in
                </button>
              </form>

              {!firebaseConfigured && (
                <p className="mt-4 rounded-lg bg-neutral-50 p-3 text-sm text-neutral-600">
                  Firebase is not configured. Copy <code>.env.example</code> to{' '}
                  <code>.env.local</code> and fill in the web app keys to enable password sign-in.
                </p>
              )}

              {config.allowDevSignIn && (
                <div className="mt-6 border-t border-neutral-200 pt-6">
                  <button
                    type="button"
                    onClick={() => void startDevSession()}
                    disabled={busy}
                    className="w-full rounded-lg border border-neutral-200 px-4 py-2.5 text-sm font-medium text-neutral-700 transition-colors hover:bg-neutral-50 disabled:opacity-50"
                  >
                    Continue as local developer
                  </button>
                  <p className="mt-2 text-center text-xs text-neutral-500">
                    Skips Firebase. Development builds on this machine only — never a production path.
                  </p>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}

function AccessDenied({
  email,
  message,
  onBack,
}: {
  email: string
  message: string
  onBack: () => void
}) {
  return (
    <div className="text-center">
      <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
        <AlertTriangle className="h-6 w-6 text-red-600" />
      </div>
      <h2 className="mt-4 font-semibold text-neutral-900">No back-office access</h2>
      <p className="mt-2 text-sm text-neutral-600">{message}</p>
      <p className="mt-4 rounded-lg bg-neutral-50 p-3 text-left text-sm text-neutral-600">
        You signed in as <span className="font-medium text-neutral-900">{email}</span>. A SuperAdmin
        must add this address to the operator list before you can continue.
      </p>
      <button
        type="button"
        onClick={onBack}
        className="mt-4 w-full rounded-lg border border-neutral-200 px-4 py-2.5 text-sm font-medium text-neutral-700 transition-colors hover:bg-neutral-50"
      >
        Sign in with a different account
      </button>
    </div>
  )
}

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-neutral-700">{label}</span>
      {children}
    </label>
  )
}

function describe(cause: unknown) {
  const code = (cause as { code?: string })?.code

  switch (code) {
    case 'auth/invalid-credential':
    case 'auth/wrong-password':
    case 'auth/user-not-found':
      return 'That email and password combination was not recognised.'
    case 'auth/too-many-requests':
      return 'Too many attempts. Wait a moment and try again.'
    case 'auth/network-request-failed':
      return 'Could not reach the authentication service. Check your connection.'
    default:
      return cause instanceof Error ? cause.message : 'Sign-in failed.'
  }
}
