import type { Config } from 'tailwindcss'
import animate from 'tailwindcss-animate'

const config: Config = {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // ── Core backgrounds ──────────────────────────────────────────────────
        bg: {
          primary: '#0F1117',
          card: '#1A1D2E',
          elevated: '#222639',
          hover: '#252840',
        },
        // ── Gold accent (Asgardian) ───────────────────────────────────────────
        gold: {
          DEFAULT: '#D4A84B',
          muted: '#8B6B2A',
          light: '#F0C878',
          dim: '#6B5020',
        },
        // ── Blue accent (interactive) ─────────────────────────────────────────
        blue: {
          accent: '#4C9AFF',
          dim: '#2A5FA0',
          muted: '#1A3A68',
        },
        // ── Status colors ─────────────────────────────────────────────────────
        worthy: '#22C55E',
        unworthy: '#F59E0B',
        banished: '#EF4444',
        // ── Text ──────────────────────────────────────────────────────────────
        text: {
          primary: '#E8EAF0',
          secondary: '#8892A4',
          muted: '#545E72',
          gold: '#D4A84B',
        },
        // ── Border ────────────────────────────────────────────────────────────
        border: {
          DEFAULT: '#2A2F45',
          gold: '#D4A84B33',
          subtle: '#1E2235',
        },
      },
      fontFamily: {
        display: ['Cinzel', 'Georgia', 'serif'],
        mono: ['JetBrains Mono', 'Fira Code', 'Consolas', 'monospace'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        'amount-sm': ['0.875rem', { fontFamily: 'JetBrains Mono', letterSpacing: '-0.02em' }],
        'amount-md': ['1rem', { fontFamily: 'JetBrains Mono', letterSpacing: '-0.02em' }],
        'amount-lg': ['1.25rem', { fontFamily: 'JetBrains Mono', letterSpacing: '-0.03em' }],
        'amount-xl': ['1.75rem', { fontFamily: 'JetBrains Mono', letterSpacing: '-0.03em' }],
        'amount-2xl': ['2.25rem', { fontFamily: 'JetBrains Mono', letterSpacing: '-0.04em' }],
        'amount-3xl': ['3rem', { fontFamily: 'JetBrains Mono', letterSpacing: '-0.04em' }],
      },
      keyframes: {
        shimmer: {
          '0%': { backgroundPosition: '-200% 0' },
          '100%': { backgroundPosition: '200% 0' },
        },
        'fade-in': {
          '0%': { opacity: '0', transform: 'translateY(4px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        'glow-pulse': {
          '0%, 100%': { boxShadow: '0 0 8px #D4A84B22' },
          '50%': { boxShadow: '0 0 20px #D4A84B44' },
        },
        // Bifrost rainbow shimmer — used on the CSV drop zone hover state
        bifrost: {
          '0%': { backgroundPosition: '0% 50%' },
          '50%': { backgroundPosition: '100% 50%' },
          '100%': { backgroundPosition: '0% 50%' },
        },
      },
      animation: {
        shimmer: 'shimmer 2s linear infinite',
        'fade-in': 'fade-in 0.3s ease-out',
        'glow-pulse': 'glow-pulse 3s ease-in-out infinite',
        bifrost: 'bifrost 3s ease infinite',
      },
      backgroundImage: {
        'gold-shimmer':
          'linear-gradient(90deg, transparent 0%, #D4A84B18 50%, transparent 100%)',
        'card-gradient': 'linear-gradient(135deg, #1A1D2E 0%, #1E2235 100%)',
        'hero-gradient': 'linear-gradient(135deg, #1A1D2E 0%, #0F1117 60%, #1A1D2E 100%)',
        'bifrost-gradient':
          'linear-gradient(135deg, #7B2FBE, #4F46E5, #0EA5E9, #10B981, #F59E0B, #EF4444, #7B2FBE)',
      },
      boxShadow: {
        card: '0 1px 3px #00000040, 0 0 0 1px #2A2F45',
        'card-hover': '0 4px 16px #00000060, 0 0 0 1px #D4A84B44',
        'gold-glow': '0 0 20px #D4A84B33',
        'inner-glow': 'inset 0 1px 0 #ffffff08',
      },
    },
  },
  plugins: [animate],
}

export default config
