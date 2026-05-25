type HeroHeaderProps = {
  realtimeStatus: string;
};

export function HeroHeader({ realtimeStatus }: HeroHeaderProps) {
  return (
    <section className="hero">
      <div className="hero-main">
        <p className="eyebrow">Industrial Platform</p>
        <h1>ModbusLab</h1>
        <p className="hero-description">
          Web-платформа для симуляции, мониторинга и автоматизированного
          тестирования Modbus-устройств.
        </p>
      </div>

      <div className="status-card">
        <div>
          <span className="status-label">Backend</span>
          <strong>ASP.NET Core + PostgreSQL</strong>
        </div>
        <span className="status-dot">SignalR: {realtimeStatus}</span>
      </div>
    </section>
  );
}