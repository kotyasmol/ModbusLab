import { type FormEvent, useState } from "react";
import { useAuth } from "../shared/auth/AuthContext";

type RegisterPageProps = {
  onLoginClick: () => void;
};

function getErrorMessage(error: unknown): string {
  if (typeof error === "object" && error !== null && "message" in error) {
    const message = (error as { message?: unknown }).message;
    if (typeof message === "string") {
      return message;
    }
  }

  return "Не удалось создать аккаунт.";
}

export function RegisterPage({ onLoginClick }: RegisterPageProps) {
  const { register } = useAuth();
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await register({
        userName,
        email: email.trim() || null,
        password,
      });
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
          <h1>Регистрация</h1>
          <p className="muted">После регистрации вход выполнится автоматически.</p>
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
            Email
            <input
              autoComplete="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>
          <label>
            Пароль
            <input
              autoComplete="new-password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>

          {error && <div className="form-error">{error}</div>}

          <button className="primary-button full-width" disabled={isSubmitting}>
            {isSubmitting ? "Создаем..." : "Создать аккаунт"}
          </button>
        </form>

        <button className="link-button" onClick={onLoginClick}>
          Уже есть аккаунт
        </button>
      </section>
    </main>
  );
}
