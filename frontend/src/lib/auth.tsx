import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import { initializeApp, type FirebaseApp } from 'firebase/app'
import {
  getAuth,
  onAuthStateChanged,
  signInWithEmailAndPassword,
  signOut as firebaseSignOut,
  type Auth,
  type User as FirebaseUser,
} from 'firebase/auth'
import { ApiError, api, setAuthorizationProvider } from './api'
import { config, firebaseConfigured } from './config'
import type { AdminSession } from './types'

const DEV_SESSION_KEY = 'movoq.admin.devSession'

export type AuthState =
  | { status: 'loading' }
  | { status: 'signedOut' }
  /** Authenticated with Firebase, but the API does not recognise the email as an operator. */
  | { status: 'denied'; email: string; message: string }
  | { status: 'ready'; session: AdminSession; devMode: boolean }

interface AuthContextValue {
  state: AuthState
  signInWithPassword: (email: string, password: string) => Promise<void>
  signInAsDeveloper: () => Promise<void>
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

let firebaseApp: FirebaseApp | null = null

function auth(): Auth {
  if (!firebaseConfigured) {
    throw new Error(
      'Firebase is not configured. Set VITE_FIREBASE_API_KEY and VITE_FIREBASE_PROJECT_ID in .env.local.',
    )
  }

  firebaseApp ??= initializeApp(config.firebase)
  return getAuth(firebaseApp)
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>({ status: 'loading' })

  // Read by the API client on every request, so it must not be captured stale in a closure.
  const tokenSource = useRef<(() => Promise<Record<string, string>>) | null>(null)

  useEffect(() => {
    setAuthorizationProvider(() => tokenSource.current?.() ?? Promise.resolve({}))
  }, [])

  const resolveSession = useCallback(async (email: string, devMode: boolean) => {
    try {
      const session = await api.signIn()
      setState({ status: 'ready', session, devMode })
    } catch (error) {
      if (error instanceof ApiError && (error.isForbidden || error.isUnauthorized)) {
        setState({ status: 'denied', email, message: error.message })
        return
      }

      throw error
    }
  }, [])

  const useDevSession = useCallback(async () => {
    tokenSource.current = async () => ({ Authorization: 'Bearer test' })
    await resolveSession('dev@localhost', true)
  }, [resolveSession])

  const useFirebaseSession = useCallback(
    async (user: FirebaseUser) => {
      // Ask Firebase for the token per request: it refreshes silently and expires after an hour.
      tokenSource.current = async () => ({ Authorization: `Bearer ${await user.getIdToken()}` })
      await resolveSession(user.email ?? '', false)
    },
    [resolveSession],
  )

  useEffect(() => {
    if (config.allowDevSignIn && sessionStorage.getItem(DEV_SESSION_KEY) === 'true') {
      void useDevSession().catch(() => {
        sessionStorage.removeItem(DEV_SESSION_KEY)
        setState({ status: 'signedOut' })
      })
      return
    }

    if (!firebaseConfigured) {
      setState({ status: 'signedOut' })
      return
    }

    return onAuthStateChanged(auth(), (user) => {
      if (!user) {
        tokenSource.current = null
        setState({ status: 'signedOut' })
        return
      }

      setState({ status: 'loading' })
      void useFirebaseSession(user).catch(() => setState({ status: 'signedOut' }))
    })
  }, [useDevSession, useFirebaseSession])

  const value = useMemo<AuthContextValue>(
    () => ({
      state,
      signInWithPassword: async (email, password) => {
        setState({ status: 'loading' })
        const credential = await signInWithEmailAndPassword(auth(), email, password)
        await useFirebaseSession(credential.user)
      },
      signInAsDeveloper: async () => {
        setState({ status: 'loading' })
        sessionStorage.setItem(DEV_SESSION_KEY, 'true')
        await useDevSession()
      },
      signOut: async () => {
        sessionStorage.removeItem(DEV_SESSION_KEY)
        tokenSource.current = null

        if (firebaseConfigured) {
          await firebaseSignOut(auth())
        }

        setState({ status: 'signedOut' })
      },
    }),
    [state, useDevSession, useFirebaseSession],
  )

  return <AuthContext value={value}>{children}</AuthContext>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside an AuthProvider.')
  }
  return context
}

/** Convenience for screens that only render once a session exists. */
export function useAdminSession(): AdminSession {
  const { state } = useAuth()
  if (state.status !== 'ready') {
    throw new Error('No admin session. This screen must render inside the authenticated shell.')
  }
  return state.session
}
