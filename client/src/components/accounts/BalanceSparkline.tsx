import { useMemo } from 'react'
import { mockSparkline, clamp } from '@/lib/utils'

interface BalanceSparklineProps {
  balance: number
  accountId: string
  positive?: boolean
  height?: number
  width?: number
}

export function BalanceSparkline({
  balance,
  accountId,
  positive = true,
  height = 40,
  width = 120,
}: BalanceSparklineProps) {
  const data = useMemo(() => mockSparkline(balance, accountId, 16), [balance, accountId])

  if (data.length < 2) return null

  const min = Math.min(...data)
  const max = Math.max(...data)
  const range = max - min || 1

  const padding = { x: 2, y: 4 }
  const innerW = width - padding.x * 2
  const innerH = height - padding.y * 2

  const points = data.map((v, i) => {
    const x = padding.x + (i / (data.length - 1)) * innerW
    const y = padding.y + (1 - (v - min) / range) * innerH
    return `${x},${y}`
  })

  const polyline = points.join(' ')

  // Fill area under the line
  const firstPt = points[0]
  const lastPt = points[points.length - 1]
  const [lastX] = lastPt.split(',')
  const [firstX] = firstPt.split(',')
  const fillPath = `M ${firstX},${height} L ${polyline.replace(',', ' L ')} L ${lastX},${height} Z`

  const strokeColor = positive ? '#22C55E' : '#EF4444'
  const fillColor = positive ? '#22C55E' : '#EF4444'

  // Render last point dot
  const [dotX, dotY] = points[points.length - 1].split(',').map(Number)

  return (
    <svg
      width={width}
      height={height}
      viewBox={`0 0 ${width} ${height}`}
      fill="none"
      className="overflow-visible"
      aria-hidden="true"
    >
      {/* Fill */}
      <path d={fillPath} fill={fillColor} fillOpacity={0.06} />
      {/* Line */}
      <polyline
        points={polyline}
        stroke={strokeColor}
        strokeWidth={1.5}
        strokeLinecap="round"
        strokeLinejoin="round"
        fill="none"
        strokeOpacity={0.7}
      />
      {/* End dot */}
      <circle cx={clamp(dotX, 0, width)} cy={clamp(dotY, 0, height)} r={2.5} fill={strokeColor} />
    </svg>
  )
}
