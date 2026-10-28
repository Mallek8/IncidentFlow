export function UsersPage() {
  return (
    <div className="space-y-4">
      <h1 className="font-display text-3xl text-navy-900">User management</h1>
      <div className="rounded-2xl border border-line bg-white p-6 shadow-sm text-slate-600">
        <p>
          Seeded development users are available via Identity. Dedicated admin user-management APIs
          (list/update roles) will extend this screen; for now use the seeded accounts documented in the README.
        </p>
        <ul className="mt-4 list-disc space-y-1 pl-5 text-sm">
          <li>admin@incidentflow.local / Admin123!</li>
          <li>tech1@incidentflow.local / Tech123!</li>
          <li>employee@incidentflow.local / Employee123!</li>
        </ul>
      </div>
    </div>
  )
}
