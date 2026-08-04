import type { Role } from "@/shared/types/role";

/** Mirrors the backend's UserDto JSON shape (GET /users, GET /users/{id}). */
export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: Role;
  isActive: boolean;
  createdAt: string;
}

/** Body for POST /users. */
export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  role: string;
}

/** Body for PUT /users/{id}. */
export interface UpdateUserRequest {
  name: string;
  email: string;
  role: string;
  isActive: boolean;
}
