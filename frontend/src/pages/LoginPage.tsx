import { type FormEvent, useState } from "react";
import { useAuth } from "../shared/auth/useAuth";

type LoginPageProps = {
  onRegisterClick: () => void;
};

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === "object" && error !== null && "message" in error) {
    const message = (error as { message?: unknown }).message;
    if (typeof message === "string") {
      return message;
    }
  }

  return "Invalid login or password.";
}

export function LoginPage({ onRegisterClick }: LoginPageProps) {
  const { login } = useAuth();
  const [userName, setUserName] = useState("admin");
  const [password, setPassword] = useState("Admin123!");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await login({ userName, password });
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-card">
        <div>
          <p className="shell-eyebrow">ModbusLab</p>
          <h1>Вход в панель</h1>
          <p className="muted">Demo: admin / Admin123!, engineer / Engineer123!, viewer / Viewer123!</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            Логин
            <input
              autoComplete="username"
              value={userName}
              onChange={(event) => setUserName(event.target.value)}
            />
          </label>
          <label>
            Пароль
            <input
              autoComplete="current-password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>

          {error && <div className="form-error">{error}</div>}

          <button className="primary-button full-width" disabled={isSubmitting}>
            {isSubmitting ? "Входим..." : "Войти"}
          </button>
        </form>

        <button className="link-button" onClick={onRegisterClick}>
          Создать новый аккаунт
        </button>
      </section>
    </main>
  );
}
