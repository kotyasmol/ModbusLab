import { expect, test } from "@playwright/test";

async function login(page: import("@playwright/test").Page, userName: string, password: string) {
  await page.goto("/");
  await page.locator('input[autocomplete="username"]').fill(userName);
  await page.locator('input[autocomplete="current-password"]').fill(password);
  await page.locator(".auth-form").locator("button").click();
}

test("admin can manage users and disabled users cannot login", async ({ page }) => {
  const suffix = Date.now();
  const userName = `e2e-user-${suffix}`;
  const email = `${userName}@local.test`;

  await login(page, "admin", "Admin123!");

  await expect(page.getByRole("button", { name: "Users" })).toBeVisible();
  await page.getByRole("button", { name: "Users" }).click();
  await expect(page.getByTestId("create-user-form")).toBeVisible();

  await page.getByTestId("create-user-name").fill(userName);
  await page.getByTestId("create-user-email").fill(email);
  await page.getByTestId("create-user-password").fill("Password123!");
  await page.getByTestId("create-user-role").selectOption("Viewer");
  await page.getByTestId("create-user-form").getByRole("button", { name: "Create user" }).click();

  const userRow = page.getByTestId(`user-row-${userName}`);
  await expect(userRow).toBeVisible();

  await page.getByTestId(`user-role-${userName}`).selectOption("Engineer");
  await expect(page.getByTestId(`user-role-${userName}`)).toHaveValue("Engineer");

  await page.getByTestId(`user-enabled-${userName}`).click();
  await expect(page.getByTestId(`user-enabled-${userName}`)).not.toBeChecked();

  await page.getByRole("button", { name: "Logout" }).click();
  await login(page, userName, "Password123!");
  await expect(page.locator(".form-error")).toContainText("disabled");
});

test("viewer cannot see users navigation", async ({ page }) => {
  await login(page, "viewer", "Viewer123!");

  await expect(page.getByRole("button", { name: "Users" })).toHaveCount(0);
  await expect(page.getByRole("button", { name: "Audit logs" })).toHaveCount(0);
});
