import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '../../libs/components/Card'
import { Form, FormControl, FormDescription, FormField, FormItem, FormLabel, FormMessage } from '../../libs/components/Form'
import { Input } from '../../libs/components/Input'
import { z } from 'zod'
import { useForm } from 'react-hook-form'
import { zodResolver } from "@hookform/resolvers/zod"
import { Button } from '../../libs/components/Button'
import { httpClient } from '../../libs/http_client'
import { ApiEndpoints } from '../../libs/constants'
import axios from 'axios'
import { ErrorResponse, ValidationError } from '../../libs/models'
import { useAlertDialog } from '../../stores/alertdialog'
import translate from '../../libs/constants'
import { convertKeysToCamelCase } from '../../libs/utils'


export const Route = createFileRoute('/tools/sign-in')({
    component: RouteComponent,
})

function RouteComponent() {
    const alertDialog = useAlertDialog();
    const navigate = useNavigate();

    const formSchema = z.object({
        username: z.string(),
        password: z.string()
    })

    const form = useForm<z.infer<typeof formSchema>>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            username: "",
            password: "",
        },
    })

    // 2. Define a submit handler.
    async function onSubmit(values: z.infer<typeof formSchema>) {
        try {
            await httpClient.post(ApiEndpoints.AdminSignIn, values)
            navigate({
                to: '/tools/admin'
            })
        } catch (err) {
            if (axios.isAxiosError(err)) {
                let body = (convertKeysToCamelCase(err.response?.data) as ErrorResponse)
                let code = body?.error?.code ?? 'UNKNOWN'
                if (err.status == 400) {
                    if (typeof body.error.details === 'string') {
                        await alertDialog.setMessageAsync({ message: translate.t(code) ?? err?.message ?? 'Something went wrong', title: 'Server Error' })
                    } else {
                        const detailsError = body.error.details as ValidationError[];
                        const groupedErrors = detailsError.reduce((acc, error) => {
                            if (!acc[error.propertyName]) {
                                acc[error.propertyName] = [];
                            }
                            acc[error.propertyName].push(error);
                            return acc;
                        }, {} as Record<string, ValidationError[]>);

                        Object.entries(groupedErrors).forEach(([propertyName, errors]) => {
                            const firstErrorMessage = errors[0]?.errorMessage ?? 'Something went wrong';
                            form.setError(propertyName.toLowerCase() as any, { message: firstErrorMessage });
                        });
                    }
                } else {
                    console.error('Axios error:', err.message)
                    await alertDialog.setMessageAsync({ message: translate.t(code) ?? err?.message ?? 'Something went wrong', title: 'Server Error' })
                }
            } else {
                await alertDialog.setMessageAsync({ message: 'Something went wrong, ex: ' + err, title: 'Server Error' })
                console.error('Unknown error:', err) // todo send error to server
            }
        }
    }

    return <>
        <div className="container mx-auto p-4 max-w-md">
            <Card>
                <CardHeader>
                    <CardTitle>
                        SignIn
                    </CardTitle>
                </CardHeader>
                <CardContent>
                    <Form  {...form} >
                        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-2">
                            <FormField
                                control={form.control}
                                name="username"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Username</FormLabel>
                                        <FormControl>
                                            <Input placeholder="Username" {...field} />
                                        </FormControl>
                                        {/* <FormDescription>
                  This is your public display name.
                </FormDescription> */}
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />

                            <FormField
                                control={form.control}
                                name="password"
                                render={({ field }) => (
                                    <FormItem>
                                        <FormLabel>Password</FormLabel>
                                        <FormControl>
                                            <Input placeholder="Password" type="password" {...field} />
                                        </FormControl>
                                        {/* <FormDescription>
                                    This is your public display name.
                                    </FormDescription> */}
                                        <FormMessage />
                                    </FormItem>
                                )}
                            />
                            <Button disabled={form.formState.isSubmitting} type="submit" >Submit</Button>
                        </form>
                    </Form>
                </CardContent>
                {/* <CardFooter>
                    Footer
                </CardFooter> */}
            </Card>
        </div>

    </>
}
