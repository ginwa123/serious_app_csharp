import { createMiddleware } from '@tanstack/start'


const loggingMiddleware = createMiddleware().client(async ({ next }) => {
    console.log('Request sent')
    const result = await next()
    console.log('Response received')
    return result
})