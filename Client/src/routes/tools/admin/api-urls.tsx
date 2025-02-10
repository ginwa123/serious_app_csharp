import { createFileRoute } from '@tanstack/react-router'
import { withAuthAdmin } from '.'
import { ColumnDef, flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../../../libs/components/Table'
import { ApiUrl, fetchApiUrls } from '../../../services/api-urls'
import { useInfiniteQuery } from '@tanstack/react-query'
import { useEffect, useRef, useCallback } from 'react'
import { cn } from '../../../libs/utils'

export const Route = createFileRoute('/tools/admin/api-urls')({
    component: withAuthAdmin(RouteComponent),
})

export const columns: ColumnDef<ApiUrl>[] = [
    { accessorKey: "id", header: "Id" },
    { accessorKey: "url", header: "Url" },
    { accessorKey: "method", header: "Method" },
    { accessorKey: "createdAt", header: "Created At" },
    { accessorKey: "updatedAt", header: "Updated At" },
]

interface DataTableProps<TData, TValue> {
    columns: ColumnDef<TData, TValue>[]
    data: TData[]
    datatableRef: React.RefObject<HTMLTableSectionElement>
}

export function DataTable<TData, TValue>({ columns, data, datatableRef }: DataTableProps<TData, TValue>) {
    const table = useReactTable({
        data,
        columns,
        getCoreRowModel: getCoreRowModel(),
        enableColumnPinning: true
    })

    return (
        <div className="relative w-full border rounded-md overflow-auto max-h-[500px]">
            <Table className="min-w-[800px]">
                {/* ✅ Sticky header */}
                <TableHeader className="bg-gray-100">
                    {table.getHeaderGroups().map((headerGroup) => (
                        <TableRow key={headerGroup.id} className="bg-green-100">
                            {headerGroup.headers.map((header, idx) => (
                                <TableHead key={header.id}
                                    className={cn(
                                        "px-4 py-3 border border-gray-500 sticky top-0 bg-white z-20",
                                        idx === 0 && "sticky left-0 bg-white z-30"
                                    )}
                                >
                                    {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                                </TableHead>
                            ))}
                        </TableRow>
                    ))}
                </TableHeader>

                {/* ✅ Scrollable body */}
                <TableBody ref={datatableRef} className=" max-h-[500px] overflow-auto">
                    {table.getRowModel().rows.length ? (
                        table.getRowModel().rows.map((row) => (
                            <TableRow key={row.id} className="even:bg-gray-50">
                                {row.getVisibleCells().map((cell, idx) => (
                                    <TableCell key={cell.id}
                                        className={cn(
                                            // "px-4 py-2 border border-gray-500",
                                            // idx === 0 && "sticky left-0 bg-white z-30"
                                        )}
                                    >
                                        {flexRender(cell.column.columnDef.cell, cell.getContext())}
                                    </TableCell>
                                ))}
                            </TableRow>
                        ))
                    ) : (
                        <TableRow>
                            <TableCell colSpan={columns.length} className="h-24 text-center">
                                No results.
                            </TableCell>
                        </TableRow>
                    )}
                </TableBody>
            </Table>
        </div>
    )
}

function RouteComponent() {
    const datatableRef = useRef<HTMLTableSectionElement>(null)

    const { data, fetchNextPage, hasNextPage, isFetchingNextPage } = useInfiniteQuery({
        queryKey: ['admin/get-api-urls'],
        queryFn: async ({ pageParam = 1 }) => {
            console.log(`[QUERY] Fetching page: ${pageParam}`);
            return fetchApiUrls({ page: pageParam, pageSize: 10 });
        },
        getNextPageParam: (lastPage) => {
            const { page, totalRecord, pageSize } = lastPage.data.meta;
            return page * pageSize < totalRecord ? page + 1 : undefined;
        },
        initialPageParam: 1
    });

    const handleScroll = useCallback(() => {
        if (!datatableRef.current) return;

        const { scrollTop, scrollHeight, clientHeight } = datatableRef.current;
        console.log(`[SCROLL] Top: ${scrollTop}, Height: ${scrollHeight}, Client: ${clientHeight}`);

        if (scrollTop + clientHeight >= scrollHeight - 50 && hasNextPage && !isFetchingNextPage) {
            console.log("[SCROLL] Fetching next page");
            fetchNextPage();
        }
    }, [hasNextPage, isFetchingNextPage, fetchNextPage]);

    useEffect(() => {
        const div = datatableRef.current;
        if (div) {
            div.addEventListener("scroll", handleScroll);
            return () => div.removeEventListener("scroll", handleScroll);
        }
    }, [handleScroll]);

    return (
        <div className="p-4">
            <h2 className="text-xl font-semibold">API URL Management</h2>

            {/* ✅ Wrapper div for scroll */}
            <div className="relative max-h-[500px] overflow-auto border border-black mt-4" ref={datatableRef}>
                <DataTable
                    data={data?.pages.flatMap(page => page.data.items) ?? []}
                    columns={columns}
                    datatableRef={datatableRef}
                />
                {isFetchingNextPage && <div className="text-center py-2">Loading more...</div>}
            </div>
        </div>
    );
}
