import { useQuery } from '@tanstack/react-query'
import { createFileRoute, Navigate, useNavigate } from '@tanstack/react-router'
import { httpClient } from '../../../libs/http_client'
import { ApiEndpoints } from '../../../libs/constants'
import axios from 'axios'
import { ErrorResponse } from '../../../libs/models'
import { useState } from 'react'
import { Card, CardHeader, CardTitle } from '../../../libs/components/Card'


export const withAuthAdmin = (Component: any) => {
    return (props: any) => {
        const navigate = useNavigate()
        const [isValid, setIsValid] = useState(false)
        // const navigate = useNavigate()
        const { isFetching, error, data } = useQuery({
            queryKey: ['admin/check'], queryFn: async () => {
                try {
                    let data = await httpClient.post(ApiEndpoints.AdminCheck, {})
                    return data
                } catch (err) {
                    console.log("apa, aaa", err)

                    if (axios.isAxiosError(err)) {
                        const body = err.response?.data as ErrorResponse
                        console.log("apa,", body)
                        throw new Error(body?.error?.details as string)
                    }
                    throw err
                }
            },
        })

        if (isFetching) {
            return <div>Loading...</div>
        }
        console.log("GUARD, STATUS DATA", data)

        if (data) {
            const result = <Component {...props} />
            return result
        }

        navigate({
            to: '/tools/sign-in'
        })
        return <div>Access Denied</div>
    }
}

export const Route = createFileRoute('/tools/admin/')({
    component: withAuthAdmin(RouteComponent),

})

function RouteComponent() {
    const navigate = useNavigate()
    return <div className='p-2'>
        <div className='mb-2'>
            <Card>
                <CardHeader>
                    <CardTitle>Admin Tools</CardTitle>
                </CardHeader>
            </Card>
        </div>

        <div className='flex flex-col gap-2'>
            <Card className='cursor-pointer' onClick={() => navigate({
                to: '/tools/admin/api-urls',
                search: {
                    page: 1,
                    page_size: 10
                }
            })}>
                <CardHeader>
                    <CardTitle>Api Url</CardTitle>
                </CardHeader>
            </Card>
            <Card>
                <CardHeader>
                    <CardTitle>User management</CardTitle>
                </CardHeader>
            </Card>

        </div>
    </div>
}
