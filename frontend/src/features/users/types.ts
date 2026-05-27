export type AppUserDto = {
  id: string;
  userName: string;
  email: string | null;
  role: string;
  isEnabled: boolean;
  createdAtUtc: string;
};

export type CreateUserRequest = {
  userName: string;
  email: string | null;
  password: string;
  role: string;
};

export type ChangeUserRoleRequest = {
  role: string;
};

export type ChangeUserStatusRequest = {
  isEnabled: boolean;
};
