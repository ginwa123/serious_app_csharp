import { createFileRoute, Outlet } from '@tanstack/react-router'

export const Route = createFileRoute('/tools/')({
  component: RouteComponent,

})

function RouteComponent() {
  return <div>
    dari root
    <Outlet />
  </div>
}
