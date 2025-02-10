
export interface ErrorModel {
    code: string;
    message: string;
    log: string;
    details: any;
}

export interface ErrorResponse {
    error: ErrorModel;
}

export interface ValidationError {
    propertyName: string; // Name of the property being validated
    errorMessage: string; // Detailed error message
    attemptedValue: string | null; // The value that caused the error
    customState: any; // Custom state, can be null or any type
    severity: number; // Severity level of the error
    errorCode: string; // Code identifying the type of validation error
    formattedMessagePlaceholderValues: FormattedMessagePlaceholderValues; // Placeholder values for the formatted message
}

export interface FormattedMessagePlaceholderValues {
    PropertyName: string; // Name of the property
    PropertyValue: string; // The attempted value of the property
    PropertyPath: string; // Path of the property
}

export interface BaseResponse<T> {
    message: string;
    log: string;
    data: T;
}


export interface Meta {
    totalRecord: number;
    pageSize: number;
    page: number;
}
