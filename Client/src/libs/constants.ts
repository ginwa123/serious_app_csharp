import translate from "i18next";
import { initReactI18next } from "react-i18next";


export enum ApiEndpoints {
    AdminSignIn = '/api/v1/users/auth/admin/sign-in',
    AdminCheck = '/api/v1/users/auth/admin/check',
    API_URLS = '/api/v1/api-urls',

}

export enum Languages {
    EN = 'en',
    FR = 'fr',
    ID = 'id',
}

export enum CodeServerEnglish {
    InternalServerError = "InternalServerError",
    GetUserByIdError = "U1",
    BadRequest = "BadRequest",

    // SIGN IN
    SignInFailedUserNotFound = "SIGNIN_U1",
    SignInFailedInvalidPassword = "SIGNIN_U2",
    SignInFailedUserNotAdmin = "SIGNIN_U3",

    // API URL
    GetApiUrlByIdError = "API_URLS_U1",
    CreateApiUrlFailed = "API_URLS_U2",
    CreateApiUrlFailedConflict = "API_URLS_U3",
    DeleteApiUrlError = "API_URLS_U4",
    UpdateApiUrlErrorNotFound = "API_URLS_U5",
    UpdateApiUrlFailed = "API_URLS_U6",

    FailedGetLock = "FAILED_GET_LOCK",
    Unauthorized = "UNAUTHORIZED",
    NoTokenAuthorization = "NO_TOKEN_AUTHORIZATION",
}

const resources = {
    en: {
        translation: {
            "UNKNOWN": "Unknown Error",
            "InternalServerError": "Internal Server Error",
            "BadRequest": "Bad Request",
            "U1": "User not found",
            "SIGNIN_U1": "Failed to get user by id",
            "SIGNIN_U2": "Invalid password",
            "SIGNIN_U3": "User is not admin",
            "API_URLS_U1": "Failed to get api url by id",
            "API_URLS_U2": "Failed to create api url",
            "API_URLS_U3": "Failed to create api url, conflict",
            "API_URLS_U4": "Failed to delete api url",
            "API_URLS_U5": "Failed to update api url, not found",
            "API_URLS_U6": "Failed to update api url",
            "FAILED_GET_LOCK": "Failed to get lock",
            "UNAUTHORIZED": "Unauthorized",
            "NO_TOKEN_AUTHORIZATION": "No token authorization"
        }
    },
    fr: {
        translation: {
        }
    }
};

translate
    .use(initReactI18next) // passes i18n down to react-i18next
    .init({
        resources,
        lng: "en", // language to use, more information here: https://www.i18next.com/overview/configuration-options#languages-namespaces-resources
        // you can use the i18n.changeLanguage function to change the language manually: https://www.i18next.com/overview/api#changelanguage
        // if you're using a language detector, do not define the lng option

        interpolation: {
            escapeValue: false // react already safes from xss
        }
    });

export default translate;