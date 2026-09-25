export interface AuthenticatedUser {
    id: string;
    email: string;
    roles: string[];
}

export interface LoginResponse {
    accessToken?: string;
    expiresAtUtc?: string;
    requiresMfa?: boolean;
    user?: AuthenticatedUser;
}
