import axios from "axios"
import { ApiEndpoints } from "../libs/constants"
import { httpClient } from "../libs/http_client"
import { BaseResponse, ErrorResponse, Meta } from "../libs/models"
import { convertKeysToCamelCase } from "../libs/utils";


export interface ApiUrlPermission {
    id: string;
    api_url_id: string;
    user_permission_id: string;
    user_permission_name: string;
}

export interface ApiUrl {
    id: string;
    url: string;
    method: string;
    created_at: string;
    updated_at: string;
    api_url_permissions: ApiUrlPermission[];
}

export interface DataFetchApiUrls {
    items: ApiUrl[];
    meta: Meta;
}

export const fetchApiUrls = async (p0: { page: number; pageSize: number; }) => {
    try {
        const queryParams = new URLSearchParams({
            page: p0.page.toString(),
            page_size: p0.pageSize.toString()
        });

        let data = await httpClient.get(`${ApiEndpoints.API_URLS}?${queryParams.toString()}`)
        const resBody = convertKeysToCamelCase(data.data) as BaseResponse<DataFetchApiUrls>
        return resBody
    } catch (err) {
        throw err
    }
}