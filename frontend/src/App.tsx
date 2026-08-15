import { Navigate, Route, Routes } from 'react-router'
import { Loader2 } from 'lucide-react'
import { useAuth } from './lib/auth'
import { AdminLayout } from './layouts/AdminLayout'
import { Activity } from './screens/Activity'
import { Audit } from './screens/Audit'
import { Dashboard } from './screens/Dashboard'
import { Draws } from './screens/Draws'
import { Fraud } from './screens/Fraud'
import { Points } from './screens/Points'
import { Prizes } from './screens/Prizes'
import { Rewards } from './screens/Rewards'
import { Roles } from './screens/Roles'
import { Settings } from './screens/Settings'
import { SignIn } from './screens/SignIn'
import { Sponsors } from './screens/Sponsors'
import { Tickets } from './screens/Tickets'
import { Users } from './screens/Users'
import {
  AdsPreview,
  AiCoachPreview,
  CampaignsPreview,
} from './screens/previews/Commerce'
import {
  AnalyticsPreview,
  AnnouncementsPreview,
  ContentPreview,
  SupportPreview,
  TeamsPreview,
} from './screens/previews/Operations'

export function App() {
  const { state } = useAuth()

  if (state.status === 'loading') {
    return (
      <div className="flex min-h-screen items-center justify-center gap-3 bg-neutral-50 text-sm text-neutral-500">
        <Loader2 className="h-5 w-5 animate-spin" />
        Signing in...
      </div>
    )
  }

  if (state.status !== 'ready') {
    return (
      <Routes>
        <Route path="/signin" element={<SignIn />} />
        <Route path="*" element={<Navigate to="/signin" replace />} />
      </Routes>
    )
  }

  return (
    <Routes>
      <Route path="/signin" element={<Navigate to="/admin" replace />} />
      <Route path="/admin" element={<AdminLayout />}>
        <Route index element={<Dashboard />} />
        <Route path="users" element={<Users />} />
        <Route path="activity" element={<Activity />} />
        <Route path="points" element={<Points />} />
        <Route path="fraud" element={<Fraud />} />
        <Route path="audit" element={<Audit />} />
        <Route path="settings" element={<Settings />} />
        <Route path="tickets" element={<Tickets />} />
        <Route path="rewards" element={<Rewards />} />
        <Route path="draws" element={<Draws />} />
        <Route path="prizes" element={<Prizes />} />
        <Route path="sponsors" element={<Sponsors />} />
        <Route path="teams" element={<TeamsPreview />} />
        <Route path="ai-coach" element={<AiCoachPreview />} />
        <Route path="ads" element={<AdsPreview />} />
        <Route path="campaigns" element={<CampaignsPreview />} />
        <Route path="announcements" element={<AnnouncementsPreview />} />
        <Route path="support" element={<SupportPreview />} />
        <Route path="analytics" element={<AnalyticsPreview />} />
        <Route path="content" element={<ContentPreview />} />
        <Route path="roles" element={<Roles />} />
      </Route>
      <Route path="*" element={<Navigate to="/admin" replace />} />
    </Routes>
  )
}
