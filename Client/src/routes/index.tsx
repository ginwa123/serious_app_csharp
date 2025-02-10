import * as React from 'react'
import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/')({
  component: HomeComponent,
})

function HomeComponent() {
  return (
    <div className="bg-green-400">
      <h3>Welcome Home! </h3>
    </div>
  )
}
