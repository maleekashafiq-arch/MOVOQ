/**
 * Browser-visible configuration. An empty API base URL means "same origin", which in development routes
 * through the Vite proxy defined in vite.config.ts.
 */
export const config = {
  apiBaseUrl: (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, ''),
  firebase: {
    apiKey: import.meta.env.VITE_FIREBASE_API_KEY ?? '',
    authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN ?? '',
    projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID ?? '',
    appId: import.meta.env.VITE_FIREBASE_APP_ID ?? '',
  },
  /**
   * The dev bypass is only offered when the build is a development build AND the flag is on, so a
   * production bundle can never present it however the environment is configured.
   */
  allowDevSignIn:
    import.meta.env.DEV && import.meta.env.VITE_ALLOW_DEV_SIGN_IN !== 'false',
} as const

export const firebaseConfigured =
  config.firebase.apiKey.length > 0 && config.firebase.projectId.length > 0
