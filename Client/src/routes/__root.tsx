import * as React from 'react'
import { Link, Outlet, createRootRoute } from '@tanstack/react-router'
import { TanStackRouterDevtools } from '@tanstack/router-devtools'
import { useAlertDialog } from '../stores/alertdialog'
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '../libs/components/AlertDialog'
import {
  useQuery,
  useMutation,
  useQueryClient,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query'
export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {

  const queryClient = new QueryClient()
  const alertDialog = useAlertDialog()
  const isOpenAlertDialog = alertDialog.isOpen
  const titleAlertDialog = alertDialog.title
  const messageAlertDialog = alertDialog.message

  const handleCLose = () => {
    alertDialog.close()
  }
  return (
    <>
      {/* <div className="p-2 flex gap-2 text-lg">
        <Link
          to="/"
          activeProps={{
            className: 'font-bold',
          }}
          activeOptions={{ exact: true }}
        >
          Home
        </Link>{' '}
        <Link
          to="/about"
          activeProps={{
            className: 'font-bold',
          }}
        >
          About
        </Link>
      </div>
      <hr /> */}
      {/* {!isOpenAlertDialog && <div className='fixed top-0 left-0 right-0 bottom-0 z-50 bg-black/50' >SSS</div>} */}
      <AlertDialog open={isOpenAlertDialog} >
        {/* <AlertDialogTrigger>Open</AlertDialogTrigger> */}
        <AlertDialogContent className='bg-white'>
          <AlertDialogHeader>
            <AlertDialogTitle>{titleAlertDialog}</AlertDialogTitle>
            <AlertDialogDescription>
              {messageAlertDialog}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            {/* <AlertDialogCancel>Cancel</AlertDialogCancel> */}
            <AlertDialogAction onClick={handleCLose}>Ok</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <QueryClientProvider client={queryClient}>
        <Outlet />

      </QueryClientProvider>
      <TanStackRouterDevtools position="bottom-right" />
    </>
  )
}
