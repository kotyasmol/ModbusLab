type Point = { label: string; value: number };

type MiniChartProps = { title: string; points: Point[]; color?: string };

function buildPath(points: Point[], width: number, height: number) {
  if (!points.length) return "";
  const max = Math.max(...points.map((p) => p.value), 1);
  const min = Math.min(...points.map((p) => p.value), 0);
  const range = Math.max(max - min, 1);

  return points
    .map((p, i) => {
      const x = (i / Math.max(points.length - 1, 1)) * width;
      const y = height - ((p.value - min) / range) * height;
      return `${i === 0 ? "M" : "L"}${x.toFixed(2)} ${y.toFixed(2)}`;
    })
    .join(" ");
}

export function MiniLineChart({ title, points, color = "#7f8cff" }: MiniChartProps) {
  const width = 260;
  const height = 80;
  const path = buildPath(points, width, height);

  return (
    <section className="panel chart-card">
      <div className="panel-header compact">
        <h3>{title}</h3>
      </div>
      {points.length === 0 ? (
        <p className="muted">No data</p>
      ) : (
        <svg viewBox={`0 0 ${width} ${height}`} className="mini-chart" role="img" aria-label={title}>
          <path d={path} fill="none" stroke={color} strokeWidth="3" />
        </svg>
      )}
    </section>
  );
}
