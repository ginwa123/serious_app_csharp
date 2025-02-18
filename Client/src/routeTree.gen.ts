/* eslint-disable */

// @ts-nocheck

// noinspection JSUnusedGlobalSymbols

// This file was automatically generated by TanStack Router.
// You should NOT make any changes in this file as it will be overwritten.
// Additionally, you should also exclude this file from your linter and/or formatter to prevent it from being checked or modified.

// Import Routes

import { Route as rootRoute } from './routes/__root'
import { Route as AboutImport } from './routes/about'
import { Route as IndexImport } from './routes/index'
import { Route as ToolsIndexImport } from './routes/tools/index'
import { Route as ToolsSignInImport } from './routes/tools/sign-in'
import { Route as ToolsAdminIndexImport } from './routes/tools/admin/index'
import { Route as ToolsAdminApiUrlsImport } from './routes/tools/admin/api-urls'

// Create/Update Routes

const AboutRoute = AboutImport.update({
  id: '/about',
  path: '/about',
  getParentRoute: () => rootRoute,
} as any)

const IndexRoute = IndexImport.update({
  id: '/',
  path: '/',
  getParentRoute: () => rootRoute,
} as any)

const ToolsIndexRoute = ToolsIndexImport.update({
  id: '/tools/',
  path: '/tools/',
  getParentRoute: () => rootRoute,
} as any)

const ToolsSignInRoute = ToolsSignInImport.update({
  id: '/tools/sign-in',
  path: '/tools/sign-in',
  getParentRoute: () => rootRoute,
} as any)

const ToolsAdminIndexRoute = ToolsAdminIndexImport.update({
  id: '/tools/admin/',
  path: '/tools/admin/',
  getParentRoute: () => rootRoute,
} as any)

const ToolsAdminApiUrlsRoute = ToolsAdminApiUrlsImport.update({
  id: '/tools/admin/api-urls',
  path: '/tools/admin/api-urls',
  getParentRoute: () => rootRoute,
} as any)

// Populate the FileRoutesByPath interface

declare module '@tanstack/react-router' {
  interface FileRoutesByPath {
    '/': {
      id: '/'
      path: '/'
      fullPath: '/'
      preLoaderRoute: typeof IndexImport
      parentRoute: typeof rootRoute
    }
    '/about': {
      id: '/about'
      path: '/about'
      fullPath: '/about'
      preLoaderRoute: typeof AboutImport
      parentRoute: typeof rootRoute
    }
    '/tools/sign-in': {
      id: '/tools/sign-in'
      path: '/tools/sign-in'
      fullPath: '/tools/sign-in'
      preLoaderRoute: typeof ToolsSignInImport
      parentRoute: typeof rootRoute
    }
    '/tools/': {
      id: '/tools/'
      path: '/tools'
      fullPath: '/tools'
      preLoaderRoute: typeof ToolsIndexImport
      parentRoute: typeof rootRoute
    }
    '/tools/admin/api-urls': {
      id: '/tools/admin/api-urls'
      path: '/tools/admin/api-urls'
      fullPath: '/tools/admin/api-urls'
      preLoaderRoute: typeof ToolsAdminApiUrlsImport
      parentRoute: typeof rootRoute
    }
    '/tools/admin/': {
      id: '/tools/admin/'
      path: '/tools/admin'
      fullPath: '/tools/admin'
      preLoaderRoute: typeof ToolsAdminIndexImport
      parentRoute: typeof rootRoute
    }
  }
}

// Create and export the route tree

export interface FileRoutesByFullPath {
  '/': typeof IndexRoute
  '/about': typeof AboutRoute
  '/tools/sign-in': typeof ToolsSignInRoute
  '/tools': typeof ToolsIndexRoute
  '/tools/admin/api-urls': typeof ToolsAdminApiUrlsRoute
  '/tools/admin': typeof ToolsAdminIndexRoute
}

export interface FileRoutesByTo {
  '/': typeof IndexRoute
  '/about': typeof AboutRoute
  '/tools/sign-in': typeof ToolsSignInRoute
  '/tools': typeof ToolsIndexRoute
  '/tools/admin/api-urls': typeof ToolsAdminApiUrlsRoute
  '/tools/admin': typeof ToolsAdminIndexRoute
}

export interface FileRoutesById {
  __root__: typeof rootRoute
  '/': typeof IndexRoute
  '/about': typeof AboutRoute
  '/tools/sign-in': typeof ToolsSignInRoute
  '/tools/': typeof ToolsIndexRoute
  '/tools/admin/api-urls': typeof ToolsAdminApiUrlsRoute
  '/tools/admin/': typeof ToolsAdminIndexRoute
}

export interface FileRouteTypes {
  fileRoutesByFullPath: FileRoutesByFullPath
  fullPaths:
    | '/'
    | '/about'
    | '/tools/sign-in'
    | '/tools'
    | '/tools/admin/api-urls'
    | '/tools/admin'
  fileRoutesByTo: FileRoutesByTo
  to:
    | '/'
    | '/about'
    | '/tools/sign-in'
    | '/tools'
    | '/tools/admin/api-urls'
    | '/tools/admin'
  id:
    | '__root__'
    | '/'
    | '/about'
    | '/tools/sign-in'
    | '/tools/'
    | '/tools/admin/api-urls'
    | '/tools/admin/'
  fileRoutesById: FileRoutesById
}

export interface RootRouteChildren {
  IndexRoute: typeof IndexRoute
  AboutRoute: typeof AboutRoute
  ToolsSignInRoute: typeof ToolsSignInRoute
  ToolsIndexRoute: typeof ToolsIndexRoute
  ToolsAdminApiUrlsRoute: typeof ToolsAdminApiUrlsRoute
  ToolsAdminIndexRoute: typeof ToolsAdminIndexRoute
}

const rootRouteChildren: RootRouteChildren = {
  IndexRoute: IndexRoute,
  AboutRoute: AboutRoute,
  ToolsSignInRoute: ToolsSignInRoute,
  ToolsIndexRoute: ToolsIndexRoute,
  ToolsAdminApiUrlsRoute: ToolsAdminApiUrlsRoute,
  ToolsAdminIndexRoute: ToolsAdminIndexRoute,
}

export const routeTree = rootRoute
  ._addFileChildren(rootRouteChildren)
  ._addFileTypes<FileRouteTypes>()

/* ROUTE_MANIFEST_START
{
  "routes": {
    "__root__": {
      "filePath": "__root.tsx",
      "children": [
        "/",
        "/about",
        "/tools/sign-in",
        "/tools/",
        "/tools/admin/api-urls",
        "/tools/admin/"
      ]
    },
    "/": {
      "filePath": "index.tsx"
    },
    "/about": {
      "filePath": "about.tsx"
    },
    "/tools/sign-in": {
      "filePath": "tools/sign-in.tsx"
    },
    "/tools/": {
      "filePath": "tools/index.tsx"
    },
    "/tools/admin/api-urls": {
      "filePath": "tools/admin/api-urls.tsx"
    },
    "/tools/admin/": {
      "filePath": "tools/admin/index.tsx"
    }
  }
}
ROUTE_MANIFEST_END */
